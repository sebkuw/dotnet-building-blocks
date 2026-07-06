using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when a requested file does not exist.
/// </summary>
public class FileNotExistException : BaseException
{
    public FileNotExistException(string filePath)
        : base("FileNotExist", 404, "File not found", $"The requested file '{filePath}' does not exist.")
    {
    }
}