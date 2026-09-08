using sebkuw.ExceptionProcessor.Exceptions.Base;

namespace sebkuw.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when access to a resource is unauthorized.
/// </summary>
public class UnauthorizedAccessException : BaseException
{
    /// <summary>Initializes an access-denied error.</summary>
    public UnauthorizedAccessException()
        : base("UnauthorizedAccess", 403, "Access denied", "You do not have permission to perform this operation.")
    {
    }
}
