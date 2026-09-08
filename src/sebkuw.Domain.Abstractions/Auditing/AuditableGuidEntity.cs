namespace sebkuw.Domain.Abstractions.Auditing;

/// <summary>
/// Represents an auditable entity with <see cref="Guid"/> identifier.
/// </summary>
public abstract class AuditableGuidEntity : AuditableEntity<Guid>
{
}
