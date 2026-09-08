using sebkuw.Domain.Abstractions.Persistence;

namespace sebkuw.Domain.Abstractions.Deactivation;

/// <summary>
/// Represents an entity that can be deactivated instead of physically deleted.
/// </summary>
public interface IDeactivatable : IHardDeleteProtected
{
    /// <summary>
    /// Gets a value indicating whether the entity is currently active.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets the identifier of the user who performed the current deactivation.
    /// </summary>
    Guid? DeactivatedBy { get; }

    /// <summary>
    /// Gets the UTC date and time when the entity was deactivated.
    /// </summary>
    DateTimeOffset? DeactivatedAt { get; }
}
