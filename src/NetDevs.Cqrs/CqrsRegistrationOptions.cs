using System.Reflection;

namespace NetDevs.Cqrs;

public sealed class CqrsRegistrationOptions
{
    internal List<Assembly> Assemblies { get; } = [];

    public CqrsRegistrationOptions RegisterServicesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Assemblies.Add(assembly);
        return this;
    }

    public CqrsRegistrationOptions RegisterServicesFromAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }
}