using sebkuw.ExceptionProcessor.Exceptions.Base;

namespace sebkuw.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when a requested file does not exist.
/// </summary>
public class FileNotExistException : BaseException
{
    /// <summary>Initializes a missing-file error.</summary>
    /// <param name="filePath">The safe client-facing file identifier or path.</param>
    public FileNotExistException(string filePath)
        : base("FileNotExist", 404, "File not found", $"The requested file '{filePath}' does not exist.")
    {
    }
}
