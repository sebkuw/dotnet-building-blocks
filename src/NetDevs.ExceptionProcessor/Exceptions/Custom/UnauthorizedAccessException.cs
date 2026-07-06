using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when access to a resource is unauthorized.
/// </summary>
public class UnauthorizedAccessException : BaseException
{
    public UnauthorizedAccessException()
        : base("UnauthorizedAccess", 403, "Access denied", "You do not have permission to perform this operation.")
    {
    }
}