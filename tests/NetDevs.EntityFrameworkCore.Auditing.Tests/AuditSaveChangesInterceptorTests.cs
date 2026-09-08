using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetDevs.Domain.Abstractions.Auditing;
using NetDevs.EntityFrameworkCore.Auditing.Abstractions;
using NetDevs.EntityFrameworkCore.Auditing.Extensions;
using NetDevs.EntityFrameworkCore.Auditing.Interceptors;
using NetDevs.EntityFrameworkCore.Auditing.Providers;

using Xunit;

namespace NetDevs.EntityFrameworkCore.Auditing.Tests;

public sealed class AuditSaveChangesInterceptorTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 7, 6, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public void AddEfCoreAuditing_registers_auditing_services()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ICurrentUserProvider>(new TestCurrentUserProvider("tester"));
        services.AddEfCoreAuditing();

        using var provider = services.BuildServiceProvider(validateScopes: true);

        Assert.IsType<SystemDateTimeProvider>(
            provider.GetRequiredService<IDateTimeProvider>());

        using var scope = provider.CreateScope();

        Assert.IsType<AuditSaveChangesInterceptor>(
            scope.ServiceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        Assert.IsType<EntityLifecycleInterceptor>(
            scope.ServiceProvider.GetRequiredService<EntityLifecycleInterceptor>());
    }

    [Fact]
    public async Task SaveChangesAsync_sets_creation_audit_values_for_added_entities()
    {
        await using var scope = CreateScope("creator", FixedUtcNow);
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var entity = new TestEntity
        {
            Name = "Created entity"
        };

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(FixedUtcNow, entity.CreatedAt);
        Assert.Null(entity.UpdatedBy);
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_sets_creation_audit_values_for_created_auditable_entities()
    {
        await using var scope = CreateScope("creator", FixedUtcNow);
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new TestCreatedEntity { Name = "Immutable history" };

        context.CreatedEntities.Add(entity);
        await context.SaveChangesAsync();

        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(FixedUtcNow, entity.CreatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_sets_update_audit_values_for_modified_entities()
    {
        var createdAt = FixedUtcNow.AddDays(-1);
        var updatedAt = FixedUtcNow;

        await using var scope = CreateScope("creator", createdAt);
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var currentUserProvider = scope.ServiceProvider.GetRequiredService<TestCurrentUserProvider>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<TestDateTimeProvider>();

        var entity = new TestEntity
        {
            Name = "Before update",
        };

        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        currentUserProvider.UserName = "updater";
        dateTimeProvider.UtcNowValue = updatedAt;
        entity.Name = "After update";
        await context.SaveChangesAsync();

        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal("updater", entity.UpdatedBy);
        Assert.Equal(updatedAt, entity.UpdatedAt);
    }

    [Fact]
    public void SaveChanges_ignores_entities_without_auditing_contracts()
    {
        using var scope = CreateScope("creator", FixedUtcNow);
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var entity = new PlainEntity { Name = "Plain entity" };

        context.PlainEntities.Add(entity);
        int changes = context.SaveChanges();

        Assert.Equal(1, changes);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal("Plain entity", entity.Name);
    }

    [Fact]
    public void SystemDateTimeProvider_returns_current_utc_time()
    {
        var provider = new SystemDateTimeProvider();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        DateTimeOffset result = provider.UtcNow();

        DateTimeOffset after = DateTimeOffset.UtcNow;
        Assert.InRange(result, before, after);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    private static AsyncServiceScope CreateScope(string userName, DateTimeOffset utcNow)
    {
        var services = new ServiceCollection();

        services.AddSingleton(new TestCurrentUserProvider(userName));
        services.AddSingleton<ICurrentUserProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<TestCurrentUserProvider>());
        services.AddSingleton(new TestDateTimeProvider(utcNow));
        services.AddSingleton<IDateTimeProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<TestDateTimeProvider>());
        services.AddEfCoreAuditing();
        services.AddSingleton<IDateTimeProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<TestDateTimeProvider>());

        services.AddDbContext<TestDbContext>((serviceProvider, options) =>
        {
            options
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddNetDevsAuditingInterceptors(serviceProvider);
        });

        return services
            .BuildServiceProvider(validateScopes: true)
            .CreateAsyncScope();
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options)
        : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();

        public DbSet<PlainEntity> PlainEntities => Set<PlainEntity>();

        public DbSet<TestCreatedEntity> CreatedEntities => Set<TestCreatedEntity>();
    }

    private sealed class TestEntity : AuditableGuidEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestCreatedEntity : CreatedAuditableEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PlainEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestCurrentUserProvider(string userName) : ICurrentUserProvider
    {
        public string UserName { get; set; } = userName;

        public string GetUserName()
        {
            return UserName;
        }
    }

    private sealed class TestDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
    {
        public DateTimeOffset UtcNowValue { get; set; } = utcNow;

        public DateTimeOffset UtcNow()
        {
            return UtcNowValue;
        }
    }
}
