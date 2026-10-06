# sebkuw.ImportProcessor

Reusable .NET 10 pipeline for streaming CSV imports. The package handles CSV parsing, typed mapping, conversion, validation, preview, batch reporting, and idempotency coordination without knowing application entities, domain rules, EF Core, or upsert semantics.

## Installation

```powershell
dotnet add package sebkuw.ImportProcessor
```

## Define a mapping

For a minimal report-only preview with no persistence adapters:

```csharp
using sebkuw.ImportProcessor;

var map = new ImportMap<ProductImport>(() => new ProductImport())
    .Map("sku", row => row.Sku)
    .Map("price", row => row.Price);

using var input = new StringReader("sku,price\nSKU-1,12.50");
var result = await new ImportProcessor<ProductImport>(map)
    .ProcessAsync(input, new ImportOptions { Preview = true }, CancellationToken.None);

Console.WriteLine($"Valid: {result.ValidRowCount}; invalid: {result.InvalidRowCount}");

public sealed class ProductImport
{
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Enabled { get; set; }
}
```

For the following extended examples, provide an application-owned `ProductImportValidator`, writer, idempotency store, and cancellation token.

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

By default, the returned batch contains every row, source values, converted value, row issues, batch-level CSV/header issues, valid and invalid counts, a unique batch ID, and the preview flag. `ProcessedRowCount` and the valid and invalid counts always cover the complete input. `HasCompleteRowReport` indicates whether `Rows` contains every processed row.

## Large files

Use bounded buffering, incremental writes, limited row retention, and concurrent mapping or validation for large inputs:

```csharp
var options = new ImportOptions
{
    BatchSize = 1_000,
    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1),
    BufferCapacity = 2_000,
    MaxRetainedRows = 100,
};

ImportBatch<ProductImport> result = await processor.ProcessAsync(
    stream,
    options,
    cancellationToken);
```

- `BatchSize` limits values passed to each sequential `IImportWriter<T>.WriteAsync` call. Its default is `int.MaxValue` to preserve the original single-write behavior.
- `MaxDegreeOfParallelism` controls concurrent row mapping and validation. Its default is `1`.
- `BufferCapacity` bounds parsed rows waiting for ordered completion and must be at least `MaxDegreeOfParallelism`.
- `MaxRetainedRows` limits detailed row reports kept in memory. Set it to `0` for summary-only results. Its default is `int.MaxValue` for compatibility.

Mapped rows, retained reports, and writer calls preserve CSV order even when validation completes out of order. When parallelism is greater than one, the mapping factory, column delegates, converters, and validators must be safe to invoke concurrently. The writer and idempotency store are invoked sequentially, so a single EF Core `DbContext` may be used by the writer as long as it is not shared elsewhere concurrently.

With finite `BatchSize`, successfully written batches cannot be rolled back by the package if malformed CSV is discovered later. Use an application-owned transaction when all-or-nothing persistence is required. Duplicate detection within one file retains idempotency keys for the duration of the import.

## Extension points

- `IImportRowValidator<T>` integrates domain or application validation asynchronously.
- `IImportWriter<T>` owns persistence and transaction/upsert behavior outside this package.
- `IImportIdempotencyStore` checks and records application-defined keys.
- `UseIdempotencyKey` selects a key from the mapped row; duplicates within the file are also rejected.

The parser supports configurable delimiters, escaped quotes, CRLF/LF records, and quoted multiline fields. Input is consumed incrementally from `Stream` or `TextReader`; cancellation is observed throughout I/O, validation, writing, and idempotency calls. Configure finite `BatchSize`, `BufferCapacity`, and `MaxRetainedRows` values to keep document processing memory-bounded apart from the per-file idempotency-key set.

## Limitations

- The first record is treated as a header by default. Headerless files use one-based column names (`"1"`, `"2"`, ...).
- The package does not define domain entities, transactions, database access, authorization, or upsert rules.
- Writer and idempotency-store atomicity is owned by the application adapter.
- Configure mappings before processing and do not mutate them while an import is running. A factory must create a fresh reference-type row for each record; writable-property mappings do not support mutating value-type rows reliably.
- `required: true` requires the mapped column to exist, not a non-empty cell. Use row validators for required values and domain constraints.
- There is no maximum record or field length. Apply upload size limits; buffering one very large field still consumes memory. Source values and conversion-error messages may contain uploaded data, so choose what to expose in an API report.
- Idempotency uses case-sensitive keys and checks mapped rows even when they contain validation errors. The first occurrence reserves a key for that file; a later corrected occurrence with the same key is reported as a duplicate.
- Cancellation and adapter failures propagate to the caller. CSV format failures become batch issues; they do not roll back earlier writes. Preview and execution are separate reads, so reopen or rewind the source before executing after a preview.

## Source layout

- `Abstractions` contains application extension-point interfaces.
- `Configuration` contains import execution options.
- `Internal` contains the streaming CSV reader.
- `Mapping` contains declarative column mapping and conversion.
- `Models` contains row, batch, and issue result types.
- `Processing` contains the import pipeline.

The folders organize the source without changing the public `sebkuw.ImportProcessor` namespace.

## Development

Run the test command from the repository root. Full verification instructions are in the [repository guide](https://github.com/sebkuw/dotnet-building-blocks#verification).

- [Changelog](https://github.com/sebkuw/dotnet-building-blocks/blob/main/src/sebkuw.ImportProcessor/CHANGELOG.md)
- Tests: `tests/sebkuw.ImportProcessor.Tests`

```powershell
dotnet test tests/sebkuw.ImportProcessor.Tests/sebkuw.ImportProcessor.Tests.csproj
```
