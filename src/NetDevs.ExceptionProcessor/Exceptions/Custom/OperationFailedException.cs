using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an operation fails for an unspecified reason.
/// </summary>
public class OperationFailedException : BaseException
{
    /// <summary>Initializes an operation-failed error.</summary>
    /// <param name="reason">A safe client-facing failure reason.</param>
    public OperationFailedException(string reason)
        : base("OperationFailed", 500, "Operation failure", $"The operation failed: {reason}")
    {
    }
}
