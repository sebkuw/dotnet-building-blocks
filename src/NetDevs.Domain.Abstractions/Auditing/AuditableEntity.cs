namespace NetDevs.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an auditable entity.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class AuditableEntity<TId> : CreatedAuditableEntity<TId>, IUpdatedEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time of the last update.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
