namespace sebkuw.ImportProcessor;

/// <summary>Streams CSV input through mapping, validation, preview, writing, and idempotency stages.</summary>
public sealed class ImportProcessor<T>
{
    private readonly ImportMap<T> map;
    private readonly IImportWriter<T>? writer;
    private readonly IImportIdempotencyStore? idempotencyStore;

    /// <summary>Creates a processor with optional application-owned writing and idempotency adapters.</summary>
    public ImportProcessor(
        ImportMap<T> map,
        IImportWriter<T>? writer = null,
        IImportIdempotencyStore? idempotencyStore = null)
    {
        this.map = map ?? throw new ArgumentNullException(nameof(map));
        this.writer = writer;
        this.idempotencyStore = idempotencyStore;
    }

    /// <summary>Processes CSV data from a stream while leaving the stream open.</summary>
    public async Task<ImportBatch<T>> ProcessAsync(
        Stream stream,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        options ??= new ImportOptions();
        options.Validate();
        using var reader = new StreamReader(stream, options.Encoding, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await ProcessAsync(reader, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Processes CSV data from a text reader.</summary>
    public async Task<ImportBatch<T>> ProcessAsync(
        TextReader reader,
        ImportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        options ??= new ImportOptions();
        options.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        var rows = new List<ImportRow<T>>();
        var batchIssues = new List<ImportIssue>();
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        string[]? headers = null;
        var recordNumber = 0L;

        try
        {
            await foreach (var record in CsvRecordReader.ReadAsync(reader, options.Delimiter, cancellationToken).ConfigureAwait(false))
            {
                recordNumber++;
                if (recordNumber == 1 && options.HasHeader)
                {
                    headers = record.Select(value => value.Trim()).ToArray();
                    ValidateHeaders(headers, batchIssues);
                    continue;
                }

                headers ??= Enumerable.Range(1, record.Count).Select(index => index.ToString(options.Culture)).ToArray();
                rows.Add(await MapRowAsync(record, headers, recordNumber, options, seenKeys, cancellationToken).ConfigureAwait(false));
            }
        }
        catch (FormatException exception)
        {
            batchIssues.Add(new ImportIssue("MalformedCsv", exception.Message));
        }

        if (recordNumber == 0)
        {
            batchIssues.Add(new ImportIssue("EmptyDocument", "The CSV document is empty."));
        }

        var batch = new ImportBatch<T>(Guid.NewGuid(), options.Preview, rows, batchIssues);
        if (!options.Preview && writer is not null && batchIssues.All(issue => issue.Severity != ImportIssueSeverity.Error))
        {
            var validRows = rows.Where(row => row.IsValid).ToArray();
            await writer.WriteAsync(validRows.Select(row => row.Value!).ToArray(), cancellationToken).ConfigureAwait(false);
            if (idempotencyStore is not null)
            {
                foreach (var key in validRows.Select(row => row.IdempotencyKey).OfType<string>())
                {
                    await idempotencyStore.MarkProcessedAsync(key, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return batch;
    }

    private void ValidateHeaders(string[] headers, List<ImportIssue> issues)
    {
        foreach (var duplicate in headers.GroupBy(header => header, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
        {
            issues.Add(new ImportIssue("DuplicateHeader", $"Header '{duplicate.Key}' occurs more than once.", Column: duplicate.Key));
        }

        foreach (var binding in map.Bindings.Where(binding => binding.Required && !headers.Contains(binding.Column, StringComparer.OrdinalIgnoreCase)))
        {
            issues.Add(new ImportIssue("MissingColumn", $"Required column '{binding.Column}' is missing.", Column: binding.Column));
        }
    }

    private async ValueTask<ImportRow<T>> MapRowAsync(
        IReadOnlyList<string> record,
        string[] headers,
        long rowNumber,
        ImportOptions options,
        HashSet<string> seenKeys,
        CancellationToken cancellationToken)
    {
        var source = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < Math.Max(headers.Length, record.Count); index++)
        {
            var name = index < headers.Length ? headers[index] : $"#{index + 1}";
            source.TryAdd(name, index < record.Count ? record[index] : string.Empty);
        }

        var issues = new List<ImportIssue>();
        var value = map.Factory();
        foreach (var binding in map.Bindings)
        {
            if (!source.TryGetValue(binding.Column, out var rawValue))
            {
                if (binding.Required)
                {
                    issues.Add(new ImportIssue("MissingColumn", $"Required column '{binding.Column}' is missing.", Column: binding.Column));
                }

                continue;
            }

            try
            {
                binding.Assign(value, rawValue, options.Culture);
            }
            catch (Exception exception) when (exception is FormatException or InvalidCastException or NotSupportedException or OverflowException or ArgumentException)
            {
                issues.Add(new ImportIssue("ConversionFailed", $"Value in column '{binding.Column}' could not be converted: {exception.Message}", Column: binding.Column));
            }
        }

        foreach (var validator in map.Validators)
        {
            var validationIssues = await validator.ValidateAsync(value, rowNumber, cancellationToken).ConfigureAwait(false);
            if (validationIssues is not null)
            {
                issues.AddRange(validationIssues);
            }
        }

        var key = map.IdempotencyKeySelector?.Invoke(value);
        if (!string.IsNullOrWhiteSpace(key))
        {
            if (!seenKeys.Add(key) || (idempotencyStore is not null && await idempotencyStore.ContainsAsync(key, cancellationToken).ConfigureAwait(false)))
            {
                issues.Add(new ImportIssue("Duplicate", $"Idempotency key '{key}' has already been processed."));
            }
        }

        return new ImportRow<T>(rowNumber, value, source, issues, key);
    }
}
