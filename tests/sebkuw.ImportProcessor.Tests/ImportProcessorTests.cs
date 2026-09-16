using System.Globalization;
using System.Text;

using Xunit;

namespace sebkuw.ImportProcessor.Tests;

public sealed class ImportProcessorTests
{
    [Fact]
    public async Task ProcessAsync_MapsQuotedRowsAndConvertsTypes()
    {
        var processor = CreateProcessor();

        var batch = await processor.ProcessAsync(new StringReader("name,quantity,note\r\n\"A, B\",12,\"line 1\nline \"\"2\"\"\""));

        var row = Assert.Single(batch.Rows);
        Assert.True(row.IsValid);
        Assert.Equal(2, row.RowNumber);
        Assert.Equal("A, B", row.Value!.Name);
        Assert.Equal(12, row.Value.Quantity);
        Assert.Equal("line 1\nline \"2\"", row.Value.Note);
        Assert.Equal("A, B", row.SourceValues["name"]);
    }

    [Fact]
    public async Task ProcessAsync_UsesConfiguredDelimiterCultureAndCustomDelegate()
    {
        var map = new ImportMap<Row>(() => new Row())
            .Map<string>("name", (row, value) => row.Name = value.Trim())
            .Map("price", row => row.Price);
        var processor = new ImportProcessor<Row>(map);
        var options = new ImportOptions { Delimiter = ';', Culture = CultureInfo.GetCultureInfo("pl-PL") };

        var batch = await processor.ProcessAsync(new StringReader("name;price\n Widget ;12,50"), options);

        var value = Assert.Single(batch.Rows).Value!;
        Assert.Equal("Widget", value.Name);
        Assert.Equal(12.50m, value.Price);
    }

    [Fact]
    public async Task ProcessAsync_ReportsMissingHeadersConversionAndValidationErrors()
    {
        var map = new ImportMap<Row>(() => new Row())
            .Map("name", row => row.Name)
            .Map("quantity", row => row.Quantity)
            .ValidateWith(new RejectEmptyNameValidator());
        var processor = new ImportProcessor<Row>(map);

        var batch = await processor.ProcessAsync(new StringReader("name,name\n,invalid"));

        Assert.Contains(batch.Issues, issue => issue.Code == "DuplicateHeader");
        Assert.Contains(batch.Issues, issue => issue.Code == "MissingColumn" && issue.Column == "quantity");
        var row = Assert.Single(batch.Rows);
        Assert.False(row.IsValid);
        Assert.Contains(row.Issues, issue => issue.Code == "MissingColumn");
        Assert.Contains(row.Issues, issue => issue.Code == "NameRequired");
    }

    [Fact]
    public async Task ProcessAsync_ReportsConversionFailureAndContinuesWithNextRow()
    {
        var processor = CreateProcessor();

        var batch = await processor.ProcessAsync(new StringReader("name,quantity,note\nbad,nope,x\ngood,3,y"));

        Assert.Equal(2, batch.Rows.Count);
        Assert.Contains(batch.Rows[0].Issues, issue => issue.Code == "ConversionFailed" && issue.Column == "quantity");
        Assert.Equal(3, batch.Rows[1].Value!.Quantity);
    }

    [Fact]
    public async Task ProcessAsync_BatchLevelErrorPreventsWriting()
    {
        var writer = new RecordingWriter();
        var processor = CreateProcessor(writer);

        var batch = await processor.ProcessAsync(new StringReader("name,name,quantity\na,a,1"));

        Assert.Contains(batch.Issues, issue => issue.Code == "DuplicateHeader");
        Assert.Empty(writer.Values);
    }

    [Fact]
    public async Task ProcessAsync_PreviewChecksDuplicatesButDoesNotWriteOrMarkKeys()
    {
        var writer = new RecordingWriter();
        var store = new RecordingStore("existing");
        var processor = CreateProcessor(writer, store, useKey: true);

        var batch = await processor.ProcessAsync(
            new StringReader("name,quantity,note\nnew,1,x\nnew,2,y\nexisting,3,z"),
            new ImportOptions { Preview = true });

        Assert.True(batch.IsPreview);
        Assert.Equal(1, batch.ValidRowCount);
        Assert.Equal(2, batch.InvalidRowCount);
        Assert.Empty(writer.Values);
        Assert.Empty(store.MarkedKeys);
        Assert.Equal(2, store.CheckedKeys.Count);
    }

    [Fact]
    public async Task ProcessAsync_ExecutionWritesOnlyValidRowsAndMarksTheirKeys()
    {
        var writer = new RecordingWriter();
        var store = new RecordingStore("existing");
        var processor = CreateProcessor(writer, store, useKey: true);

        var batch = await processor.ProcessAsync(new StringReader("name,quantity,note\nnew,1,x\nexisting,2,y"));

        Assert.False(batch.IsPreview);
        Assert.Equal("new", Assert.Single(writer.Values).Name);
        Assert.Equal("new", Assert.Single(store.MarkedKeys));
    }

    [Fact]
    public async Task ProcessAsync_WritesBoundedBatchesAndLimitsRetainedRows()
    {
        var writer = new RecordingWriter();
        var processor = CreateProcessor(writer);
        var options = new ImportOptions
        {
            BatchSize = 2,
            BufferCapacity = 2,
            MaxRetainedRows = 1,
        };

        var batch = await processor.ProcessAsync(
            new StringReader("name,quantity\na,1\nb,2\nc,3\nd,4\ne,5"),
            options);

        Assert.Equal([2, 2, 1], writer.BatchSizes);
        Assert.Equal(["a", "b", "c", "d", "e"], writer.Values.Select(row => row.Name));
        Assert.Equal(5, batch.ProcessedRowCount);
        Assert.Equal(5, batch.ValidRowCount);
        Assert.Equal(0, batch.InvalidRowCount);
        Assert.Single(batch.Rows);
        Assert.False(batch.HasCompleteRowReport);
    }

    [Fact]
    public async Task ProcessAsync_HandlesLargeInputWithoutRetainingRowReports()
    {
        const int rowCount = 10_000;
        var csv = new StringBuilder("name,quantity\n");
        for (var index = 0; index < rowCount; index++)
            csv.Append("item-").Append(index).Append(',').Append(index).Append('\n');

        var writer = new CountingWriter();
        var processor = CreateProcessor(writer);
        var options = new ImportOptions
        {
            BatchSize = 128,
            BufferCapacity = 64,
            MaxDegreeOfParallelism = 4,
            MaxRetainedRows = 0,
        };

        var batch = await processor.ProcessAsync(new StringReader(csv.ToString()), options);

        Assert.Equal(rowCount, writer.Count);
        Assert.Equal(79, writer.CallCount);
        Assert.Equal(128, writer.MaximumBatchSize);
        Assert.Equal(rowCount, batch.ProcessedRowCount);
        Assert.Empty(batch.Rows);
        Assert.False(batch.HasCompleteRowReport);
    }

    [Fact]
    public async Task ProcessAsync_MapsConcurrentlyAndPreservesInputOrder()
    {
        var validator = new CoordinatedValidator(participants: 3);
        var map = new ImportMap<Row>(() => new Row())
            .Map("name", row => row.Name)
            .Map("quantity", row => row.Quantity)
            .ValidateWith(validator);
        var writer = new RecordingWriter();
        var processor = new ImportProcessor<Row>(map, writer);
        var options = new ImportOptions
        {
            BatchSize = 2,
            BufferCapacity = 3,
            MaxDegreeOfParallelism = 3,
        };
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var batch = await processor.ProcessAsync(
            new StringReader("name,quantity\nfirst,1\nsecond,2\nthird,3"),
            options,
            cancellation.Token);

        Assert.True(validator.MaximumConcurrency > 1);
        Assert.Equal(["first", "second", "third"], batch.Rows.Select(row => row.Value!.Name));
        Assert.Equal(["first", "second", "third"], writer.Values.Select(row => row.Name));
    }

    [Fact]
    public async Task ProcessAsync_CancelsConcurrentValidation()
    {
        var validator = new CancellationValidator();
        var map = new ImportMap<Row>(() => new Row())
            .Map("name", row => row.Name)
            .Map("quantity", row => row.Quantity)
            .ValidateWith(validator);
        var processor = new ImportProcessor<Row>(map);
        var options = new ImportOptions { BufferCapacity = 2, MaxDegreeOfParallelism = 2 };
        using var cancellation = new CancellationTokenSource();

        var processing = processor.ProcessAsync(
            new StringReader("name,quantity\na,1\nb,2"),
            options,
            cancellation.Token);
        await validator.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => processing);
    }

    [Fact]
    public async Task ProcessAsync_HeaderlessInputUsesOneBasedColumnNames()
    {
        var map = new ImportMap<Row>(() => new Row()).Map("1", row => row.Name).Map("2", row => row.Quantity);

        var batch = await new ImportProcessor<Row>(map).ProcessAsync(
            new StringReader("item,5"),
            new ImportOptions { HasHeader = false });

        Assert.Equal("item", Assert.Single(batch.Rows).Value!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("name,quantity\n\"unfinished,1")]
    public async Task ProcessAsync_ReportsDocumentErrors(string csv)
    {
        var batch = await CreateProcessor().ProcessAsync(new StringReader(csv));

        Assert.True(batch.HasErrors);
        Assert.NotEmpty(batch.Issues);
    }

    [Fact]
    public async Task ProcessAsync_StreamLeavesInputOpen()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("name,quantity,note\na,1,b"));

        var batch = await CreateProcessor().ProcessAsync(stream);

        Assert.Single(batch.Rows);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public async Task ProcessAsync_ObservesCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateProcessor().ProcessAsync(new StringReader("name,quantity,note"), cancellationToken: source.Token));
    }

    [Fact]
    public void Map_RejectsDuplicateColumnsAndInvalidExpression()
    {
        var map = new ImportMap<Row>(() => new Row()).Map("name", row => row.Name);

        Assert.Throws<ArgumentException>(() => map.Map("NAME", row => row.Name));
        Assert.Throws<ArgumentException>(() => new ImportMap<Row>(() => new Row()).Map("length", row => row.Name.Length));
    }

    [Theory]
    [InlineData('\n')]
    [InlineData('\r')]
    [InlineData('"')]
    [InlineData('\0')]
    public async Task ProcessAsync_RejectsInvalidDelimiter(char delimiter)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateProcessor().ProcessAsync(new StringReader("x"), new ImportOptions { Delimiter = delimiter }));
    }

    [Fact]
    public async Task ProcessAsync_RejectsInvalidPipelineLimits()
    {
        var processor = CreateProcessor();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            processor.ProcessAsync(new StringReader("x"), new ImportOptions { BatchSize = 0 }));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            processor.ProcessAsync(new StringReader("x"), new ImportOptions { MaxDegreeOfParallelism = 0 }));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            processor.ProcessAsync(new StringReader("x"), new ImportOptions { BufferCapacity = 1, MaxDegreeOfParallelism = 2 }));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            processor.ProcessAsync(new StringReader("x"), new ImportOptions { MaxRetainedRows = -1 }));
    }

    private static ImportProcessor<Row> CreateProcessor(
        IImportWriter<Row>? writer = null,
        IImportIdempotencyStore? store = null,
        bool useKey = false)
    {
        var map = new ImportMap<Row>(() => new Row())
            .Map("name", row => row.Name)
            .Map("quantity", row => row.Quantity)
            .Map("note", row => row.Note, required: false);
        if (useKey)
            map.UseIdempotencyKey(row => row.Name);

        return new ImportProcessor<Row>(map, writer, store);
    }

    private sealed class Row
    {
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string Note { get; set; } = string.Empty;
    }

    private sealed class RejectEmptyNameValidator : IImportRowValidator<Row>
    {
        public ValueTask<IReadOnlyList<ImportIssue>> ValidateAsync(Row value, long rowNumber, CancellationToken cancellationToken)
        {
            IReadOnlyList<ImportIssue> issues = string.IsNullOrWhiteSpace(value.Name)
                ? [new ImportIssue("NameRequired", $"Name is required on row {rowNumber}.")]
                : [];
            return ValueTask.FromResult(issues);
        }
    }

    private sealed class RecordingWriter : IImportWriter<Row>
    {
        public List<Row> Values { get; } = [];

        public List<int> BatchSizes { get; } = [];

        public ValueTask WriteAsync(IReadOnlyList<Row> values, CancellationToken cancellationToken)
        {
            BatchSizes.Add(values.Count);
            Values.AddRange(values);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CountingWriter : IImportWriter<Row>
    {
        public int CallCount { get; private set; }

        public int Count { get; private set; }

        public int MaximumBatchSize { get; private set; }

        public ValueTask WriteAsync(IReadOnlyList<Row> values, CancellationToken cancellationToken)
        {
            CallCount++;
            Count += values.Count;
            MaximumBatchSize = Math.Max(MaximumBatchSize, values.Count);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CoordinatedValidator(int participants) : IImportRowValidator<Row>
    {
        private readonly Lock sync = new();
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int activeCount;
        private int startedCount;

        public int MaximumConcurrency { get; private set; }

        public async ValueTask<IReadOnlyList<ImportIssue>> ValidateAsync(
            Row value,
            long rowNumber,
            CancellationToken cancellationToken)
        {
            var active = Interlocked.Increment(ref activeCount);
            lock (sync)
                MaximumConcurrency = Math.Max(MaximumConcurrency, active);

            if (Interlocked.Increment(ref startedCount) >= participants)
                release.TrySetResult();

            try
            {
                await release.Task.WaitAsync(cancellationToken);
                if (value.Quantity == 1)
                    await Task.Delay(25, cancellationToken);

                return [];
            }
            finally
            {
                Interlocked.Decrement(ref activeCount);
            }
        }
    }

    private sealed class CancellationValidator : IImportRowValidator<Row>
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async ValueTask<IReadOnlyList<ImportIssue>> ValidateAsync(
            Row value,
            long rowNumber,
            CancellationToken cancellationToken)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return [];
        }
    }

    private sealed class RecordingStore(params string[] existing) : IImportIdempotencyStore
    {
        private readonly HashSet<string> existingKeys = [.. existing];

        public List<string> CheckedKeys { get; } = [];

        public List<string> MarkedKeys { get; } = [];

        public ValueTask<bool> ContainsAsync(string key, CancellationToken cancellationToken)
        {
            CheckedKeys.Add(key);
            return ValueTask.FromResult(existingKeys.Contains(key));
        }

        public ValueTask MarkProcessedAsync(string key, CancellationToken cancellationToken)
        {
            MarkedKeys.Add(key);
            return ValueTask.CompletedTask;
        }
    }
}
