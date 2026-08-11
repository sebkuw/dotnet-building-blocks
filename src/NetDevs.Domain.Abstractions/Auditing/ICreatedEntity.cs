namespace NetDevs.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an entity that stores creation audit information.
/// </summary>
public interface ICreatedEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC date and time when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
}
