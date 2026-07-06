using Microsoft.EntityFrameworkCore;
using NetDevs.QueryableProcessor.Enums;
using NetDevs.QueryableProcessor.Extensions;
using NetDevs.QueryableProcessor.Models;
using NetDevs.QueryableProcessor.Solvers;
using System.Text.Json;
using Xunit;

namespace NetDevs.QueryableProcessor.Tests;

public sealed class QueryableProcessorTests
{
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
                    Value = new[] { 1, 3, 4 }
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
    public void Paginate_throws_when_options_have_invalid_init_values()
    {
        IQueryable<TestProduct> products = CreateProducts().AsQueryable();
        var options = new PaginationOptions { PageNumber = 0, PageSize = 10 };

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            products.Paginate(options).ToList());

        Assert.Equal("pagination", exception.ParamName);
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
