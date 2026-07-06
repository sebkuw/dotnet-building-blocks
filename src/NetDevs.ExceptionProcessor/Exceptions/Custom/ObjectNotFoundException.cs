using NetDevs.ExceptionProcessor.Exceptions.Base;

namespace NetDevs.ExceptionProcessor.Exceptions.Custom;

/// <summary>
/// Exception thrown when an expected object is not found.
/// </summary>
public class ObjectNotFoundException : BaseException
{
    public ObjectNotFoundException(string objectName, object id)
        : base("ObjectNotFound", 404, "Object not found", $"The requested {objectName} with ID {id} was not found.")
    {
    }
}