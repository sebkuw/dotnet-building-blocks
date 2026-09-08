using sebkuw.ExceptionProcessor.Exceptions.Base;

namespace sebkuw.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an operation fails due to invalid input.
/// </summary>
public class ValidationException : BaseException
{
    /// <summary>Initializes a validation error.</summary>
    /// <param name="details">Safe client-facing validation details.</param>
    public ValidationException(string details)
        : base("ValidationError", 400, "Invalid input", $"Validation failed: {details}")
    {
    }
}
