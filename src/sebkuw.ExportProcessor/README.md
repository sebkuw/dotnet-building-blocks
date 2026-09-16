# sebkuw.ExportProcessor

Streaming CSV exports for .NET 10 and EF Core. The package applies a `sebkuw.QueryableProcessor` `RequestDto` before projection, writes rows incrementally, and exposes only application-allowlisted columns.

## Installation

```powershell
dotnet add package sebkuw.ExportProcessor
```

The package targets .NET 10, uses EF Core asynchronous query execution, and depends on `sebkuw.QueryableProcessor` for the shared request contract.

## Public API

- `CsvExportMap<T>` defines the allowed columns, headers, and optional formatters.
- `CsvExportOptions` selects columns, scope, delimiter, culture, encoding, headers, formula protection, and an optional row cap.
- `CsvExportScope` chooses all matching rows or the current request page.
- `ExportCsvAsync` applies the request and streams projected rows to a destination.
- `CsvExportResult` reports the record count and effective column order.

## Define safe export columns

Project only the fields needed by the export, then map the column names a client is allowed to request:

```csharp
using sebkuw.ExportProcessor;

var map = new CsvExportMap<ProductExportRow>()
    .Map("id", row => row.Id, header: "ID")
    .Map("name", row => row.Name, header: "Product")
    .Map("price", row => row.Price, formatter: (value, culture) => value.ToString("N2", culture));
```

The map is a case-insensitive allowlist. Unknown, blank, or duplicate requested columns are rejected instead of being resolved through reflection.

## Export all matching rows

```csharp
await dbContext.Products.ExportCsvAsync(
    httpContext.Response.Body,
    requestDto,
    product => new ProductExportRow(product.Id, product.Name, product.Price),
    map,
    new CsvExportOptions
    {
        Columns = ["name", "price"],
        Delimiter = ';'
    },
    cancellationToken);
```

`AllMatching` is the default scope: filters and sorting from `RequestDto` are applied, while pagination is ignored so every matching row is exported. Use `Scope = CsvExportScope.CurrentPage` to apply `PaginationOptions` as well. `MaxRows` can impose an additional server-side cap in either scope.

The query is projected by EF Core before asynchronous enumeration. CSV rows are then written incrementally to the supplied stream, which remains open. The result reports the exported record count and final column order.

## CSV behavior

- UTF-8 with a byte-order mark is the default for spreadsheet compatibility.
- Headers are included by default and may be disabled.
- Commas, configured delimiters, quotes, and line breaks are escaped according to CSV rules.
- Values beginning with `=`, `+`, `-`, or `@` are prefixed with an apostrophe by default to prevent spreadsheet formula injection.
- Default formatting uses invariant culture; both culture and per-column formatters are configurable.

Applications remain responsible for authorization, filename selection, response headers, and deciding which mapped columns a caller may select. The package never discovers entity properties from untrusted input.

## Limitations

- The source must support EF Core asynchronous enumeration.
- Selected columns control CSV output; the projection expression itself is static, so it should contain only fields intended for this export use case.
- The package does not buffer or return a downloadable file object. ASP.NET Core applications should set their own `Content-Type`, filename, and authorization policy before writing to the response stream.

## Development

- [Library instructions](AGENTS.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.ExportProcessor.Tests`

```powershell
dotnet test tests/sebkuw.ExportProcessor.Tests/sebkuw.ExportProcessor.Tests.csproj
```
