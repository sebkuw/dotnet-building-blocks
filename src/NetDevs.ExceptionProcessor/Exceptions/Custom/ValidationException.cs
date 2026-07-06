using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an operation fails due to invalid input.
/// </summary>
public class ValidationException : BaseException
{
    public ValidationException(string details)
        : base("ValidationError", 400, "Invalid input", $"Validation failed: {details}")
    {
    }
}