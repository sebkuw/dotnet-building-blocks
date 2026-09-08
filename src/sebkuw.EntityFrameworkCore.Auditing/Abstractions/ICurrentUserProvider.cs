namespace sebkuw.EntityFrameworkCore.Auditing.Abstractions;

/// <summary>
/// Provides information about the current user.
/// </summary>
public interface ICurrentUserProvider
{
    /// <summary>
    /// Returns the current user name or identifier.
    /// </summary>
    /// <returns>The current user name or identifier.</returns>
    string GetUserName();
}
