using NetDevs.Domain.Abstractions.Auditing;
using NetDevs.Domain.Abstractions.Entities;
using Xunit;

namespace NetDevs.Domain.Abstractions.Tests;

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
}
