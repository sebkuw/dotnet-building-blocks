namespace sebkuw.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an entity that stores update audit information.
/// </summary>
public interface IUpdatedEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time of the last update.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}
