using System;

namespace NetDevs.ExceptionProcessor.Exceptions.Base;

/// <summary>
/// Abstract base class for custom exceptions in ExceptionProcessor.
/// Allows projects using the library to define their own custom exceptions.
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// Unique error code to identify the exception type.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Suggested HTTP status code for the exception.
    /// </summary>
    public int HttpCode { get; }

    /// <summary>
    /// Main error message, providing a short description of the issue.
    /// </summary>
    public string MainText { get; }

    /// <summary>
    /// Detailed error description with context-specific information.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the BaseException class.
    /// </summary>
    /// <param name="code">Unique error code.</param>
    /// <param name="httpCode">Suggested HTTP status code.</param>
    /// <param name="mainText">Short description of the issue.</param>
    /// <param name="description">Detailed explanation of the error.</param>
    public BaseException(string code, int httpCode, string mainText, string description)
        : base(mainText)
    {
        Code = code;
        HttpCode = httpCode;
        MainText = mainText;
        Description = description;
    }
}