using NetDevs.Domain.Abstractions.Entities;

namespace NetDevs.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an entity containing creation audit information.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class CreatedAuditableEntity<TId> : Entity<TId>, ICreatedEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
