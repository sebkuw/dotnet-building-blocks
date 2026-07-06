namespace NetDevs.Domain.Abstractions.Entities;

/// <summary>
/// Represents a base entity.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class Entity<TId> : IEntity<TId>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public TId Id { get; set; } = default!;
}