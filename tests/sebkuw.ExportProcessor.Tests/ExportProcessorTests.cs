using System.Globalization;
using System.Text;

using Microsoft.EntityFrameworkCore;

using sebkuw.QueryableProcessor.Enums;
using sebkuw.QueryableProcessor.Models;

using Xunit;

namespace sebkuw.ExportProcessor.Tests;

public sealed class ExportProcessorTests
{
    [Fact]
    public async Task ExportCsvAsync_exports_all_filtered_rows_in_sort_order_by_default()
    {
        await using TestDbContext context = await CreateContextAsync();
        var request = CreatePublishedRequest(pageNumber: 1, pageSize: 1);
        await using var destination = new MemoryStream();

        CsvExportResult result = await context.Products.ExportCsvAsync(
            destination,
            request,
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap());

        Assert.Equal(2, result.RecordCount);
        Assert.Equal(["id", "name", "price", "note"], result.Columns);
        Assert.Equal(
            "ID,Product,Price,Note\r\n3,Desk,300,Office\r\n2,Notebook,20,Paper",
            ReadCsv(destination));
        Assert.True(destination.CanWrite);
    }

    [Fact]
    public async Task ExportCsvAsync_can_export_only_the_current_page_and_selected_columns()
    {
        await using TestDbContext context = await CreateContextAsync();
        var request = CreatePublishedRequest(pageNumber: 2, pageSize: 1);
        await using var destination = new MemoryStream();
        var options = new CsvExportOptions
        {
            Scope = CsvExportScope.CurrentPage,
            Columns = ["NAME", "id"]
        };

        CsvExportResult result = await context.Products.ExportCsvAsync(
            destination,
            request,
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            options);

        Assert.Equal(1, result.RecordCount);
        Assert.Equal(["name", "id"], result.Columns);
        Assert.Equal("Product,ID\r\nNotebook,2", ReadCsv(destination));
    }

    [Fact]
    public async Task ExportCsvAsync_escapes_csv_and_protects_formula_cells()
    {
        await using var context = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        context.Products.Add(new Product
        {
            Id = 1,
            Name = "=SUM(A1:A2)",
            Price = 12.5m,
            Status = ProductStatus.Published,
            Note = "A;\"B\"\nC"
        });
        await context.SaveChangesAsync();
        await using var destination = new MemoryStream();
        var options = new CsvExportOptions
        {
            Delimiter = ';',
            Culture = CultureInfo.GetCultureInfo("pl-PL"),
            Columns = ["name", "price", "note"]
        };

        await context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            options);

        Assert.Equal(
            "Product;Price;Note\r\n'=SUM(A1:A2);12,5;\"A;\"\"B\"\"\nC\"",
            ReadCsv(destination));
    }

    [Fact]
    public async Task ExportCsvAsync_supports_headerless_output_formula_opt_out_and_row_limit()
    {
        await using TestDbContext context = await CreateContextAsync();
        context.Products.Add(new Product { Id = 5, Name = "+value", Note = "", Price = 1 });
        await context.SaveChangesAsync();
        await using var destination = new MemoryStream();
        var request = CreateRequest(sortParam: "id desc");
        var options = new CsvExportOptions
        {
            IncludeHeader = false,
            ProtectFormulaCells = false,
            Columns = ["name"],
            MaxRows = 1,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };

        CsvExportResult result = await context.Products.ExportCsvAsync(
            destination,
            request,
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            options);

        Assert.Equal(1, result.RecordCount);
        Assert.Equal("+value", ReadCsv(destination));
    }

    [Fact]
    public async Task ExportCsvAsync_observes_cancellation()
    {
        await using TestDbContext context = await CreateContextAsync();
        await using var destination = new MemoryStream();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task ExportCsvAsync_rejects_unknown_duplicate_empty_and_unmapped_columns()
    {
        await using TestDbContext context = await CreateContextAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => ExportWithColumnsAsync(context, ["missing"]));
        await Assert.ThrowsAsync<ArgumentException>(() => ExportWithColumnsAsync(context, ["id", "ID"]));
        await Assert.ThrowsAsync<ArgumentException>(() => ExportWithColumnsAsync(context, []));
        await Assert.ThrowsAsync<ArgumentException>(() => ExportWithColumnsAsync(context, [""]));

        await using var destination = new MemoryStream();
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            new CsvExportMap<ProductRow>()));
    }

    [Fact]
    public void CsvExportMap_rejects_invalid_and_duplicate_mappings()
    {
        var map = new CsvExportMap<ProductRow>()
            .Map("id", row => row.Id);

        Assert.Throws<ArgumentException>(() => map.Map("ID", row => row.Id));
        Assert.Throws<ArgumentException>(() => new CsvExportMap<ProductRow>().Map("", row => row.Id));
        Assert.Throws<ArgumentException>(() => new CsvExportMap<ProductRow>().Map("id", row => row.Id, header: ""));
        Assert.Throws<ArgumentNullException>(() => new CsvExportMap<ProductRow>().Map<int>("id", null!));
    }

    [Theory]
    [InlineData('\n')]
    [InlineData('\r')]
    [InlineData('"')]
    [InlineData('\0')]
    public async Task ExportCsvAsync_rejects_invalid_delimiters(char delimiter)
    {
        await using TestDbContext context = await CreateContextAsync();
        await using var destination = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            new CsvExportOptions { Delimiter = delimiter }));
    }

    [Fact]
    public async Task ExportCsvAsync_rejects_invalid_options_and_destination()
    {
        await using TestDbContext context = await CreateContextAsync();
        await using var destination = new MemoryStream();
        await using var readOnly = new MemoryStream([1, 2, 3], writable: false);

        await Assert.ThrowsAsync<ArgumentException>(() => context.Products.ExportCsvAsync(
            readOnly,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap()));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ExportWithOptionsAsync(
            context,
            destination,
            new CsvExportOptions { MaxRows = 0 }));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => ExportWithOptionsAsync(
            context,
            destination,
            new CsvExportOptions { Scope = (CsvExportScope)99 }));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ExportWithOptionsAsync(
            context,
            destination,
            new CsvExportOptions { Culture = null! }));
        await Assert.ThrowsAsync<ArgumentNullException>(() => ExportWithOptionsAsync(
            context,
            destination,
            new CsvExportOptions { Encoding = null! }));
    }

    private static async Task<TestDbContext> CreateContextAsync()
    {
        var context = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        context.Products.AddRange(
            new Product { Id = 1, Name = "Laptop", Price = 1500, Status = ProductStatus.Draft, Note = null },
            new Product { Id = 2, Name = "Notebook", Price = 20, Status = ProductStatus.Published, Note = "Paper" },
            new Product { Id = 3, Name = "Desk", Price = 300, Status = ProductStatus.Published, Note = "Office" },
            new Product { Id = 4, Name = "Chair", Price = 100, Status = ProductStatus.Archived, Note = "Office" });
        await context.SaveChangesAsync();
        return context;
    }

    private static RequestDto CreatePublishedRequest(int pageNumber, int pageSize)
    {
        return CreateRequest(
            "price desc",
            [
                new FilterCondition
                {
                    PropertyPath = nameof(Product.Status),
                    Operation = FilterOperation.Equal,
                    Value = ProductStatus.Published
                }
            ],
            pageNumber,
            pageSize);
    }

    private static RequestDto CreateRequest(
        string? sortParam = null,
        IEnumerable<FilterCondition>? filters = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return new RequestDto
        {
            SortParam = sortParam,
            Filters = filters,
            PaginationOptions = new PaginationOptions(pageNumber, pageSize)
        };
    }

    private static CsvExportMap<ProductRow> CreateMap()
    {
        return new CsvExportMap<ProductRow>()
            .Map("id", row => row.Id, header: "ID")
            .Map("name", row => row.Name, header: "Product")
            .Map("price", row => row.Price, header: "Price", formatter: (value, culture) => value.ToString("0.##", culture))
            .Map("note", row => row.Note, header: "Note");
    }

    private static async Task<CsvExportResult> ExportWithColumnsAsync(
        TestDbContext context,
        IReadOnlyList<string> columns)
    {
        await using var destination = new MemoryStream();
        return await context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            new CsvExportOptions { Columns = columns });
    }

    private static Task<CsvExportResult> ExportWithOptionsAsync(
        TestDbContext context,
        Stream destination,
        CsvExportOptions options)
    {
        return context.Products.ExportCsvAsync(
            destination,
            CreateRequest(),
            product => new ProductRow(product.Id, product.Name, product.Price, product.Note),
            CreateMap(),
            options);
    }

    private static string ReadCsv(MemoryStream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
    }

    private sealed class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public ProductStatus Status { get; set; }

        public string? Note { get; set; }
    }

    private enum ProductStatus
    {
        Draft,
        Published,
        Archived,
    }

    private sealed record ProductRow(int Id, string Name, decimal Price, string? Note);
}
