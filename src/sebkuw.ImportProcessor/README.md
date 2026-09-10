# sebkuw.ImportProcessor

Reusable .NET 10 pipeline for streaming CSV imports. The package handles CSV parsing, typed mapping, conversion, validation, preview, batch reporting, and idempotency coordination without knowing application entities, domain rules, EF Core, or upsert semantics.

## Installation

```powershell
dotnet add package sebkuw.ImportProcessor
```

## Define a mapping

Mappings may use writable-property expressions, assignment delegates, built-in type conversion, or a custom converter.

```csharp
using sebkuw.ImportProcessor;

var map = new ImportMap<ProductImport>(() => new ProductImport())
    .Map("sku", row => row.Sku)
    .Map("price", row => row.Price, (text, culture) => decimal.Parse(text, culture))
    .Map<bool>("enabled", (row, value) => row.Enabled = value)
    .ValidateWith(new ProductImportValidator())
    .UseIdempotencyKey(row => row.Sku);
```

Built-in conversion supports strings, nullable values, enums, and types with a `TypeConverter`, using the configured culture. Column matching is case-insensitive.

## Preview and execute

```csharp
var processor = new ImportProcessor<ProductImport>(map, writer, idempotencyStore);

await using var stream = File.OpenRead("products.csv");
ImportBatch<ProductImport> preview = await processor.ProcessAsync(
    stream,
    new ImportOptions { Preview = true },
    cancellationToken);
```

Preview performs parsing, mapping, validation, and duplicate checks but never invokes `IImportWriter<T>` or records idempotency keys. Set `Preview = false` to pass only valid values to the application-provided writer and then mark their keys as processed. If no writer is supplied, processing remains report-only.

The returned batch contains every row, source values, converted value, row issues, batch-level CSV/header issues, valid and invalid counts, a unique batch ID, and the preview flag.

## Extension points

- `IImportRowValidator<T>` integrates domain or application validation asynchronously.
- `IImportWriter<T>` owns persistence and transaction/upsert behavior outside this package.
- `IImportIdempotencyStore` checks and records application-defined keys.
- `UseIdempotencyKey` selects a key from the mapped row; duplicates within the file are also rejected.

The parser supports configurable delimiters, escaped quotes, CRLF/LF records, and quoted multiline fields. Input is consumed incrementally from `Stream` or `TextReader`; cancellation is observed throughout I/O, validation, writing, and idempotency calls. The processor retains row results because the batch is the requested report model, but it never buffers the entire source document before processing.

## Limitations

- The first record is treated as a header by default. Headerless files use one-based column names (`"1"`, `"2"`, ...).
- The package does not define domain entities, transactions, database access, authorization, or upsert rules.
- Writer and idempotency-store atomicity is owned by the application adapter.

## Development

- [Library instructions](AGENTS.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.ImportProcessor.Tests`

```powershell
dotnet test tests/sebkuw.ImportProcessor.Tests/sebkuw.ImportProcessor.Tests.csproj
```
