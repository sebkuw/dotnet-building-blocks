namespace NetDevs.EntityFrameworkCore.Auditing.Abstractions;

/// <summary>
/// Provides current UTC date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Returns the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time.</returns>
    DateTimeOffset UtcNow();
}
