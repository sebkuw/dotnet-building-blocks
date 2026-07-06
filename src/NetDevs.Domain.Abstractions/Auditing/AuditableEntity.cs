using NetDevs.Domain.Abstractions.Entities;

namespace NetDevs.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an auditable entity.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class AuditableEntity<TId> : Entity<TId>, ICreatedEntity, IUpdatedEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time of the last update.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
