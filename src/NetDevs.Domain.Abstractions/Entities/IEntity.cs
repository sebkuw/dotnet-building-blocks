namespace NetDevs.Domain.Abstractions.Entities;

/// <summary>
/// Represents a base entity contract.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public interface IEntity<TId>
{
    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    TId Id { get; set; }
}
