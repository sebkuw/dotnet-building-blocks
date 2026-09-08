using sebkuw.ExceptionProcessor.Exceptions.Base;

namespace sebkuw.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an expected object is not found.
/// </summary>
public class ObjectNotFoundException : BaseException
{
    /// <summary>Initializes an object-not-found error.</summary>
    /// <param name="objectName">The safe client-facing object name.</param>
    /// <param name="id">The missing object's identifier.</param>
    public ObjectNotFoundException(string objectName, object id)
        : base("ObjectNotFound", 404, "Object not found", $"The requested {objectName} with ID {id} was not found.")
    {
    }
}
