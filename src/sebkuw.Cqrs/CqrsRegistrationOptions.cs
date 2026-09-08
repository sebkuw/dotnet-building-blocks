using System.Reflection;

namespace sebkuw.Cqrs;

/// <summary>
/// Selects assemblies that contain CQRS handler implementations.
/// </summary>
public sealed class CqrsRegistrationOptions
{
    internal List<Assembly> Assemblies { get; } = [];

    /// <summary>Adds an assembly to the handler scan.</summary>
    /// <param name="assembly">The assembly that contains handlers.</param>
    /// <returns>The same options instance.</returns>
    public CqrsRegistrationOptions RegisterServicesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Assemblies.Add(assembly);
        return this;
    }

    /// <summary>Adds the assembly that contains the marker type.</summary>
    /// <typeparam name="T">A type from the assembly that contains handlers.</typeparam>
    /// <returns>The same options instance.</returns>
    public CqrsRegistrationOptions RegisterServicesFromAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }
}
