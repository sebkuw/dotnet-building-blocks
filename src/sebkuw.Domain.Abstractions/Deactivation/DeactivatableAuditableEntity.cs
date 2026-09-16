using sebkuw.Domain.Abstractions.Auditing;

namespace sebkuw.Domain.Abstractions.Deactivation;

/// <summary>
/// Represents an auditable entity that supports explicit deactivation.
/// </summary>
/// <typeparam name="TId">The type of entity identifier.</typeparam>
public abstract class DeactivatableAuditableEntity<TId> : AuditableEntity<TId>, IDeactivatable
{
    /// <inheritdoc />
    public bool IsActive { get; private set; } = true;

    /// <inheritdoc />
    public Guid? DeactivatedBy { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? DeactivatedAt { get; private set; }

    /// <summary>
    /// Deactivates the entity and records the current deactivation metadata.
    /// </summary>
    /// <param name="userId">The identifier of the user performing the operation.</param>
    /// <param name="deactivatedAt">The UTC date and time of the deactivation.</param>
    public void Deactivate(Guid userId, DateTimeOffset deactivatedAt)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);

        if (deactivatedAt.Offset != TimeSpan.Zero)
            throw new ArgumentException("The deactivation date and time must be expressed in UTC.", nameof(deactivatedAt));

        if (!IsActive)
            return;

        IsActive = false;
        DeactivatedBy = userId;
        DeactivatedAt = deactivatedAt;
    }

    /// <summary>
    /// Reactivates the entity and clears the current deactivation metadata.
    /// </summary>
    public void Reactivate()
    {
        if (IsActive)
            return;

        IsActive = true;
        DeactivatedBy = null;
        DeactivatedAt = null;
    }
}
