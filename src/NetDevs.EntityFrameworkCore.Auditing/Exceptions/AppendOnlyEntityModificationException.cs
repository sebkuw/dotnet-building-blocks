namespace NetDevs.EntityFrameworkCore.Auditing.Exceptions;

/// <summary>
/// Represents an attempt to modify an existing append-only entity.
/// </summary>
public sealed class AppendOnlyEntityModificationException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppendOnlyEntityModificationException"/> class.
    /// </summary>
    /// <param name="entityType">The append-only entity type name.</param>
    public AppendOnlyEntityModificationException(string entityType)
        : base(CreateMessage(entityType))
    {
    }

    private static string CreateMessage(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        return $"Entity '{entityType}' is append-only and cannot be modified.";
    }
}
