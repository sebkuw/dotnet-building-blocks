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

## Development

- [Library instructions](AGENTS.md)
- [Changelog](CHANGELOG.md)
- Tests: `tests/sebkuw.ImportProcessor.Tests`

```powershell
dotnet test tests/sebkuw.ImportProcessor.Tests/sebkuw.ImportProcessor.Tests.csproj
```
