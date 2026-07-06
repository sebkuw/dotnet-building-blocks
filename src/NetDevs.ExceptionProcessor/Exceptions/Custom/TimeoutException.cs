using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an operation exceeds the allowed time.
/// </summary>
public class TimeoutException : BaseException
{
    public TimeoutException()
        : base("TimeoutError", 408, "Operation timed out", "The operation exceeded the allowed execution time.")
    {
    }
}