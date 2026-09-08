namespace NetDevs.EntityFrameworkCore.Auditing.Exceptions;

/// <summary>
/// Represents an attempt to physically delete an entity protected from hard deletion.
/// </summary>
public sealed class HardDeleteNotAllowedException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HardDeleteNotAllowedException"/> class.
    /// </summary>
    /// <param name="entityType">The protected entity type name.</param>
    public HardDeleteNotAllowedException(string entityType)
        : base(CreateMessage(entityType))
    {
    }

    private static string CreateMessage(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        return $"Physical deletion of entity '{entityType}' is not allowed. " +
            "Use the appropriate domain lifecycle operation instead.";
    }
}
