using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using sebkuw.QueryableProcessor.Enums;
using sebkuw.QueryableProcessor.Extensions;
using sebkuw.QueryableProcessor.Models;
using sebkuw.QueryableProcessor.Solvers;

using Xunit;

namespace sebkuw.QueryableProcessor.Tests;

public sealed class QueryableProcessorTests
{
    private static readonly int[] IncludedProductIds = [1, 3, 4];

    private static readonly JsonSerializerOptions CamelCaseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void ApplyFilters_filters_by_string_operations()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        List<string> names = products
            .ApplyFilters(
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Name),
                    Operation = FilterOperation.EndsWith,
                    Value = "Pro"
                }
            ])
            .Select(product => product.Name)
            .ToList();

        Assert.Equal(["Laptop Pro"], names);
    }

    [Fact]
    public void ApplyFilters_filters_by_in_and_not_in_operations()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        List<int> ids = products
            .ApplyFilters(
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Id),
                    Operation = FilterOperation.In,
                    Value = IncludedProductIds
                },
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Status),
                    Operation = FilterOperation.NotIn,
                    Value = new object[] { ProductStatus.Archived }
                }
            ])
            .Select(product => product.Id)
            .ToList();

        Assert.Equal([1, 3], ids);
    }

    [Fact]
    public void ApplyFilters_converts_string_value_to_enum()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        List<int> ids = products
            .ApplyFilters(
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Status),
                    Operation = FilterOperation.Equal,
                    Value = "Published"
                }
            ])
            .Select(product => product.Id)
            .ToList();

        Assert.Equal([2, 3], ids);
    }

    [Fact]
    public void ApplyFilters_filters_by_json_array_value()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();
        JsonElement idsJson = JsonDocument.Parse("[1,3]").RootElement.Clone();

        List<int> ids = products
            .ApplyFilters(
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Id),
                    Operation = FilterOperation.In,
                    Value = idsJson
                }
            ])
            .Select(product => product.Id)
            .ToList();

        Assert.Equal([1, 3], ids);
    }

    [Fact]
    public void ApplyFilters_supports_all_comparison_and_string_operations()
    {
        Assert.Equal([1, 2, 3], FilterIds(nameof(TestProduct.Status), FilterOperation.NotEqual, ProductStatus.Archived));
        Assert.Equal([1, 3], FilterIds(nameof(TestProduct.Price), FilterOperation.GreaterThan, 100m));
        Assert.Equal([2], FilterIds(nameof(TestProduct.Price), FilterOperation.LessThan, 100m));
        Assert.Equal([1, 3, 4], FilterIds(nameof(TestProduct.Price), FilterOperation.GreaterThanOrEqual, 100m));
        Assert.Equal([2, 4], FilterIds(nameof(TestProduct.Price), FilterOperation.LessThanOrEqual, 100m));
        Assert.Equal([1], FilterIds(nameof(TestProduct.Name), FilterOperation.Contains, "top"));
        Assert.Equal([2], FilterIds(nameof(TestProduct.Name), FilterOperation.StartsWith, "Note"));
    }

    [Fact]
    public void ApplyFilters_converts_supported_json_scalar_values()
    {
        Guid key = Guid.NewGuid();
        DateTime createdAt = new(2026, 8, 11, 12, 30, 0, DateTimeKind.Utc);
        var record = new ConversionRecord
        {
            IntValue = 42,
            LongValue = 9_007_199_254_740_991,
            Text = "value",
            Enabled = true,
            Key = key,
            CreatedAt = createdAt,
            Status = ProductStatus.Published
        };
        IQueryable<ConversionRecord> records = new[] { record }.AsQueryable();

        AssertJsonFilterMatch(records, nameof(ConversionRecord.IntValue), "42");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.LongValue), "9007199254740991");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.Text), "\"value\"");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.Enabled), "true");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.Key), $"\"{key}\"");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.CreatedAt), $"\"{createdAt:O}\"");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.Status), "\"Published\"");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.Status), "1");
        AssertJsonFilterMatch(records, nameof(ConversionRecord.OptionalValue), "null");
    }

    [Fact]
    public void ApplyFilters_handles_null_guid_and_invalid_inputs()
    {
        Guid key = Guid.NewGuid();
        var products = new[]
        {
            new TestProduct { Id = 1, Name = "One", Key = key },
            new TestProduct { Id = 2, Name = "Two", OptionalScore = 5 }
        }.AsQueryable();

        Assert.Single(products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = "key",
                Operation = FilterOperation.Equal,
                Value = key.ToString()
            }
        ]));
        Assert.Throws<FormatException>(() => products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = nameof(TestProduct.Key),
                Operation = FilterOperation.Equal,
                Value = "not-a-guid"
            }
        ]));
        Assert.Single(products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = nameof(TestProduct.OptionalScore),
                Operation = FilterOperation.Equal,
                Value = null!
            }
        ]));
        Assert.Empty(products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = nameof(TestProduct.Id),
                Operation = FilterOperation.Equal,
                Value = null!
            }
        ]));

        Assert.Throws<ArgumentException>(() => products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = "missing",
                Operation = FilterOperation.Equal,
                Value = 1
            }
        ]));
        Assert.Throws<ArgumentException>(() => products.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = nameof(TestProduct.Id),
                Operation = FilterOperation.In,
                Value = 1
            }
        ]));
    }

    [Fact]
    public void SortBy_sorts_by_nested_property()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        List<string> names = products
            .SortBy("asc_Category.Name")
            .Select(product => product.Name)
            .ToList();

        Assert.Equal(["Notebook", "Laptop Pro", "Desk", "Chair"], names);
    }

    [Fact]
    public void SortBy_accepts_angular_sort_contract_and_camel_case_property()
    {
        List<int> ids = CreateProducts()
            .AsQueryable()
            .SortBy("price desc")
            .Select(product => product.Id)
            .ToList();

        Assert.Equal([1, 3, 4, 2], ids);
    }

    [Fact]
    public void SortBy_supports_ascending_suffix_and_returns_source_for_invalid_values()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        Assert.Equal([2, 4, 3, 1], products.SortBy("price asc").Select(product => product.Id));
        Assert.Same(products, products.SortBy(string.Empty));
        Assert.Same(products, products.SortBy("price"));
        Assert.Same(products, products.SortBy("missing asc"));
        Assert.Same(products, products.SortBy("asc_"));
    }

    [Fact]
    public void Json_contract_matches_shared_ui_list_models()
    {
        const string json = """
            {
              "SortParam": "price desc",
              "Filters": [
                {
                  "PropertyPath": "name",
                  "Operation": 6,
                  "Value": "pro"
                }
              ],
              "PaginationOptions": {
                "PageNumber": 1,
                "PageSize": 25
              }
            }
            """;

        RequestDto request = JsonSerializer.Deserialize<RequestDto>(json)
            ?? throw new InvalidOperationException("Angular request contract did not deserialize.");

        Assert.Equal("price desc", request.SortParam);
        FilterCondition filter = Assert.Single(request.Filters!);
        Assert.Equal("name", filter.PropertyPath);
        Assert.Equal(FilterOperation.Contains, filter.Operation);
        Assert.Equal(1, request.PaginationOptions.PageNumber);
        Assert.Equal(25, request.PaginationOptions.PageSize);

        var response = new PaginationResponse<int>([1, 2], 3, 1, 2);
        string responseJson = JsonSerializer.Serialize(response, CamelCaseJsonOptions);
        using JsonDocument document = JsonDocument.Parse(responseJson);

        Assert.True(document.RootElement.TryGetProperty("Data", out _));
        Assert.True(document.RootElement.TryGetProperty("TotalItems", out _));
        Assert.True(document.RootElement.TryGetProperty("TotalPages", out _));
        Assert.True(document.RootElement.TryGetProperty("PageNumber", out _));
        Assert.True(document.RootElement.TryGetProperty("PageSize", out _));
        Assert.True(document.RootElement.TryGetProperty("HasNextPage", out _));
        Assert.False(document.RootElement.TryGetProperty("data", out _));
    }

    [Fact]
    public void Paginate_throws_when_options_have_invalid_init_values()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();
        var options = new PaginationOptions { PageNumber = 0, PageSize = 10 };

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            products.Paginate(options).ToList());

        Assert.Equal("pagination", exception.ParamName);
    }

    [Fact]
    public void ApplyRequest_applies_filters_sorting_and_optional_pagination()
    {
        var request = new RequestDto
        {
            SortParam = "price desc",
            Filters =
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Status),
                    Operation = FilterOperation.Equal,
                    Value = ProductStatus.Published
                }
            ],
            PaginationOptions = new PaginationOptions(2, 1)
        };

        IQueryable<TestProduct> products = CreateProducts().AsQueryable();

        Assert.Equal([2], products.ApplyRequest(request).Select(product => product.Id));
        Assert.Equal([3, 2], products.ApplyRequest(request, includePagination: false).Select(product => product.Id));
    }

    [Fact]
    public void Paginate_throws_when_page_size_exceeds_limit()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();
        var options = new PaginationOptions { PageNumber = 1, PageSize = 101 };

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            products.Paginate(options).ToList());

        Assert.Equal("pagination", exception.ParamName);
    }

    [Fact]
    public void Pagination_validates_constructor_and_zero_page_size()
    {
        Assert.Throws<ArgumentException>(() => new PaginationOptions(0, 10));
        Assert.Throws<ArgumentException>(() => new PaginationOptions(1, 0));

        IQueryable<TestProduct> products = CreateProducts().AsQueryable();
        var options = new PaginationOptions { PageNumber = 1, PageSize = 0 };

        Assert.Throws<ArgumentException>(() => products.Paginate(options).ToList());
    }

    [Fact]
    public async Task SolveRequest_filters_sorts_paginates_and_projects()
    {
        await using var context = new TestDbContext(
            new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        context.Products.AddRange(CreateProducts());
        await context.SaveChangesAsync();

        var request = new RequestDto
        {
            SortParam = "desc_Price",
            Filters =
            [
                new FilterCondition
                {
                    PropertyPath = nameof(TestProduct.Status),
                    Operation = FilterOperation.Equal,
                    Value = ProductStatus.Published
                }
            ],
            PaginationOptions = new PaginationOptions(1, 1)
        };

        PaginationResponse<ProductListItem> response = await context.Products
            .SolveRequest(
                request,
                product => new ProductListItem(product.Id, product.Name));

        ProductListItem item = Assert.Single(response.Data);
        Assert.Equal(3, item.Id);
        Assert.Equal("Desk", item.Name);
        Assert.Equal(2, response.TotalItems);
        Assert.Equal(2, response.TotalPages);
        Assert.True(response.HasNextPage);
        Assert.False(response.HasPreviousPage);
    }

    private static List<TestProduct> CreateProducts()
    {
        return
        [
            new TestProduct
            {
                Id = 1,
                Name = "Laptop Pro",
                Price = 1_500,
                Status = ProductStatus.Draft,
                Category = new TestCategory { Name = "Computers" }
            },
            new TestProduct
            {
                Id = 2,
                Name = "Notebook",
                Price = 20,
                Status = ProductStatus.Published,
                Category = new TestCategory { Name = "Accessories" }
            },
            new TestProduct
            {
                Id = 3,
                Name = "Desk",
                Price = 300,
                Status = ProductStatus.Published,
                Category = new TestCategory { Name = "Furniture" }
            },
            new TestProduct
            {
                Id = 4,
                Name = "Chair",
                Price = 100,
                Status = ProductStatus.Archived,
                Category = new TestCategory { Name = "Furniture" }
            }
        ];
    }

    private static List<int> FilterIds(string propertyPath, FilterOperation operation, object value)
    {
        return CreateProducts()
            .AsQueryable()
            .ApplyFilters(
            [
                new FilterCondition
                {
                    PropertyPath = propertyPath,
                    Operation = operation,
                    Value = value
                }
            ])
            .Select(product => product.Id)
            .ToList();
    }

    private static void AssertJsonFilterMatch(
        IQueryable<ConversionRecord> records,
        string propertyPath,
        string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement value = document.RootElement.Clone();
        ConversionRecord match = Assert.Single(records.ApplyFilters(
        [
            new FilterCondition
            {
                PropertyPath = propertyPath,
                Operation = FilterOperation.Equal,
                Value = value
            }
        ]));

        Assert.Same(records.Single(), match);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TestProduct> Products => Set<TestProduct>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestProduct>().OwnsOne(product => product.Category);
        }
    }

    private sealed class TestProduct
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public ProductStatus Status { get; set; }

        public TestCategory Category { get; set; } = new();

        public Guid Key { get; set; }

        public int? OptionalScore { get; set; }
    }

    private sealed class ConversionRecord
    {
        public int IntValue { get; init; }

        public long LongValue { get; init; }

        public string Text { get; init; } = string.Empty;

        public bool Enabled { get; init; }

        public Guid Key { get; init; }

        public DateTime CreatedAt { get; init; }

        public ProductStatus Status { get; init; }

        public int? OptionalValue { get; init; }
    }

    private sealed class TestCategory
    {
        public string Name { get; set; } = string.Empty;
    }

    private enum ProductStatus
    {
        Draft,
        Published,
        Archived
    }

    private sealed record ProductListItem(int Id, string Name);
}
