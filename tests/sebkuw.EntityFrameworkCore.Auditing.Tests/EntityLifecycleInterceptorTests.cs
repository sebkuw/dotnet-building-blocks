using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using sebkuw.Domain.Abstractions.Auditing;
using sebkuw.Domain.Abstractions.Deactivation;
using sebkuw.Domain.Abstractions.Entities;
using sebkuw.Domain.Abstractions.History;
using sebkuw.Domain.Abstractions.Persistence;
using sebkuw.EntityFrameworkCore.Auditing.Abstractions;
using sebkuw.EntityFrameworkCore.Auditing.Exceptions;
using sebkuw.EntityFrameworkCore.Auditing.Extensions;
using sebkuw.EntityFrameworkCore.Auditing.Interceptors;

using Xunit;

namespace sebkuw.EntityFrameworkCore.Auditing.Tests;

public sealed class EntityLifecycleInterceptorTests
{
    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChangesAsync_rejects_hard_delete_of_protected_entity()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new HardDeleteProtectedEntity { Name = "Protected" };
        context.ProtectedEntities.Add(entity);
        await context.SaveChangesAsync();

        context.ProtectedEntities.Remove(entity);

        var exception = await Assert.ThrowsAsync<HardDeleteNotAllowedException>(
            () => context.SaveChangesAsync());
        Assert.Contains(nameof(HardDeleteProtectedEntity), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChangesAsync_rejects_append_only_entity_update()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new AppendOnlyEntity { Value = "Original" };
        context.AppendOnlyEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.Value = "Changed";

        var exception = await Assert.ThrowsAsync<AppendOnlyEntityModificationException>(
            () => context.SaveChangesAsync());
        Assert.Contains(nameof(AppendOnlyEntity), exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveChangesAsync_rejects_append_only_entity_delete_as_hard_delete()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new AppendOnlyEntity { Value = "History" };
        context.AppendOnlyEntities.Add(entity);
        await context.SaveChangesAsync();

        context.AppendOnlyEntities.Remove(entity);

        await Assert.ThrowsAsync<HardDeleteNotAllowedException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_persists_deactivation_and_update_audit_values()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new DeactivatableEntity { Name = "Active" };
        context.DeactivatableEntities.Add(entity);
        await context.SaveChangesAsync();
        Guid userId = Guid.NewGuid();

        entity.Deactivate(userId, FixedUtcNow);
        await context.SaveChangesAsync();

        Assert.False(entity.IsActive);
        Assert.Equal(userId, entity.DeactivatedBy);
        Assert.Equal(FixedUtcNow, entity.DeactivatedAt);
        Assert.Equal("tester", entity.UpdatedBy);
        Assert.Equal(FixedUtcNow, entity.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_allows_normal_entity_delete()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new PlainEntity { Name = "Disposable" };
        context.PlainEntities.Add(entity);
        await context.SaveChangesAsync();

        context.PlainEntities.Remove(entity);
        int changes = await context.SaveChangesAsync();

        Assert.Equal(1, changes);
        Assert.Empty(context.PlainEntities);
    }

    [Fact]
    public void SaveChanges_rejects_hard_delete_of_protected_entity()
    {
        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        var entity = new HardDeleteProtectedEntity { Name = "Protected" };
        context.ProtectedEntities.Add(entity);
        context.SaveChanges();

        context.ProtectedEntities.Remove(entity);

        Assert.Throws<HardDeleteNotAllowedException>(() => context.SaveChanges());
    }

    [Fact]
    public async Task SaveChangesAsync_propagates_cancellation()
    {
        await using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LifecycleDbContext>();
        context.PlainEntities.Add(new PlainEntity { Name = "Cancelled" });
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            context.SaveChangesAsync(cancellationSource.Token));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Lifecycle_exceptions_reject_missing_entity_type(string? entityType)
    {
        Assert.ThrowsAny<ArgumentException>(() => new HardDeleteNotAllowedException(entityType!));
        Assert.ThrowsAny<ArgumentException>(() => new AppendOnlyEntityModificationException(entityType!));
    }

    [Fact]
    public void Lifecycle_interceptor_is_defined_in_infrastructure_package()
    {
        Assert.Equal(
            typeof(AuditSaveChangesInterceptor).Assembly,
            typeof(EntityLifecycleInterceptor).Assembly);
        Assert.NotEqual(
            typeof(IHardDeleteProtected).Assembly,
            typeof(EntityLifecycleInterceptor).Assembly);
    }

    private static AsyncServiceScope CreateScope()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICurrentUserProvider>(new TestCurrentUserProvider());
        services.AddSingleton<IDateTimeProvider>(new TestDateTimeProvider());
        services.AddEfCoreAuditing();
        services.AddSingleton<IDateTimeProvider>(new TestDateTimeProvider());
        services.AddDbContext<LifecycleDbContext>((serviceProvider, options) =>
            options
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddSebkuwAuditingInterceptors(serviceProvider));

        return services.BuildServiceProvider(validateScopes: true).CreateAsyncScope();
    }

    private sealed class LifecycleDbContext(DbContextOptions<LifecycleDbContext> options)
        : DbContext(options)
    {
        public DbSet<HardDeleteProtectedEntity> ProtectedEntities => Set<HardDeleteProtectedEntity>();

        public DbSet<AppendOnlyEntity> AppendOnlyEntities => Set<AppendOnlyEntity>();

        public DbSet<DeactivatableEntity> DeactivatableEntities => Set<DeactivatableEntity>();

        public DbSet<PlainEntity> PlainEntities => Set<PlainEntity>();
    }

    private sealed class HardDeleteProtectedEntity : GuidEntity, IHardDeleteProtected
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class AppendOnlyEntity : CreatedAuditableEntity<Guid>, IAppendOnlyEntity
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class DeactivatableEntity : DeactivatableAuditableEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PlainEntity : GuidEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestCurrentUserProvider : ICurrentUserProvider
    {
        public string GetUserName() => "tester";
    }

    private sealed class TestDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow() => FixedUtcNow;
    }
}
