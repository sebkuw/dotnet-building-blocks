using sebkuw.Domain.Abstractions.Auditing;
using sebkuw.Domain.Abstractions.Deactivation;
using sebkuw.Domain.Abstractions.Entities;
using sebkuw.Domain.Abstractions.History;
using sebkuw.Domain.Abstractions.Persistence;

using Xunit;

namespace sebkuw.Domain.Abstractions.Tests;

public sealed class DomainAbstractionsTests
{
    [Fact]
    public void Entity_exposes_mutable_identifier_through_contract()
    {
        Guid id = Guid.NewGuid();
        IEntity<Guid> entity = new TestGuidEntity
        {
            Id = id
        };

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void AuditableEntity_implements_creation_and_update_contracts()
    {
        TestAuditableGuidEntity entity = new();

        Assert.IsType<ICreatedEntity>(entity, exactMatch: false);
        Assert.IsType<IUpdatedEntity>(entity, exactMatch: false);
        Assert.IsType<IEntity<Guid>>(entity, exactMatch: false);
        Assert.IsAssignableFrom<CreatedAuditableEntity<Guid>>(entity);
    }

    [Fact]
    public void CreatedAuditableEntity_exposes_only_creation_audit_contract()
    {
        var entity = new TestCreatedAuditableEntity
        {
            Id = 42,
            CreatedBy = "creator",
            CreatedAt = new DateTimeOffset(2026, 9, 8, 8, 0, 0, TimeSpan.Zero)
        };

        Assert.IsAssignableFrom<ICreatedEntity>(entity);
        Assert.IsNotAssignableFrom<IUpdatedEntity>(entity);
        Assert.Equal(42, entity.Id);
        Assert.Equal("creator", entity.CreatedBy);
    }

    [Fact]
    public void Lifecycle_contracts_protect_hard_delete()
    {
        Assert.Contains(typeof(IHardDeleteProtected), typeof(IDeactivatable).GetInterfaces());
        Assert.Contains(typeof(IHardDeleteProtected), typeof(IAppendOnlyEntity).GetInterfaces());
    }

    [Fact]
    public void Deactivatable_entity_is_active_by_default()
    {
        var entity = new TestDeactivatableEntity();

        Assert.True(entity.IsActive);
        Assert.Null(entity.DeactivatedBy);
        Assert.Null(entity.DeactivatedAt);
    }

    [Fact]
    public void Deactivate_records_current_deactivation_and_is_idempotent()
    {
        var entity = new TestDeactivatableEntity();
        Guid firstUserId = Guid.NewGuid();
        Guid secondUserId = Guid.NewGuid();
        var firstDeactivation = new DateTimeOffset(2026, 9, 8, 8, 0, 0, TimeSpan.Zero);

        entity.Deactivate(firstUserId, firstDeactivation);
        entity.Deactivate(secondUserId, firstDeactivation.AddHours(1));

        Assert.False(entity.IsActive);
        Assert.Equal(firstUserId, entity.DeactivatedBy);
        Assert.Equal(firstDeactivation, entity.DeactivatedAt);
    }

    [Fact]
    public void Reactivate_restores_active_state_and_clears_deactivation_metadata()
    {
        var entity = new TestDeactivatableEntity();
        entity.Deactivate(
            Guid.NewGuid(),
            new DateTimeOffset(2026, 9, 8, 8, 0, 0, TimeSpan.Zero));

        entity.Reactivate();
        entity.Reactivate();

        Assert.True(entity.IsActive);
        Assert.Null(entity.DeactivatedBy);
        Assert.Null(entity.DeactivatedAt);
    }

    [Fact]
    public void Deactivate_rejects_empty_user_identifier()
    {
        var entity = new TestDeactivatableEntity();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            entity.Deactivate(Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Deactivate_rejects_non_utc_timestamp()
    {
        var entity = new TestDeactivatableEntity();
        var localTimestamp = new DateTimeOffset(2026, 9, 8, 10, 0, 0, TimeSpan.FromHours(2));

        Assert.Throws<ArgumentException>(() =>
            entity.Deactivate(Guid.NewGuid(), localTimestamp));
    }

    [Fact]
    public void Domain_abstractions_do_not_reference_entity_framework_core()
    {
        string[] references = typeof(IEntity<>).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name =>
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }

    [Fact]
    public void AuditableEntity_stores_creation_and_update_metadata()
    {
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;

        TestAuditableGuidEntity entity = new()
        {
            Id = Guid.NewGuid(),
            CreatedBy = "creator",
            CreatedAt = createdAt,
            UpdatedBy = "updater",
            UpdatedAt = updatedAt
        };

        Assert.Equal("creator", entity.CreatedBy);
        Assert.Equal(createdAt, entity.CreatedAt);
        Assert.Equal("updater", entity.UpdatedBy);
        Assert.Equal(updatedAt, entity.UpdatedAt);
    }

    private sealed class TestGuidEntity : GuidEntity;

    private sealed class TestAuditableGuidEntity : AuditableGuidEntity;

    private sealed class TestCreatedAuditableEntity : CreatedAuditableEntity<int>;

    private sealed class TestDeactivatableEntity : DeactivatableAuditableEntity<Guid>;
}
