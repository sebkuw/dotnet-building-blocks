using sebkuw.EntityFrameworkCore.Auditing.Abstractions;

namespace sebkuw.EntityFrameworkCore.Auditing.Providers;

/// <summary>
/// Provides current UTC time using <see cref="TimeProvider.System"/>.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Returns the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time.</returns>
    public DateTimeOffset UtcNow()
    {
        return TimeProvider.System.GetUtcNow();
    }
}
