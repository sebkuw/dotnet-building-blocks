using sebkuw.ExceptionProcessor.Exceptions.Base;

namespace sebkuw.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when a database-related error occurs.
/// </summary>
public class DatabaseException : BaseException
{
    /// <summary>Initializes a database error.</summary>
    /// <param name="errorDetails">Safe details describing the database failure.</param>
    public DatabaseException(string errorDetails)
        : base("DatabaseError", 500, "Database failure", $"A database error occurred: {errorDetails}")
    {
    }
}
