using Microsoft.Extensions.DependencyInjection;

using sebkuw.Cqrs.Abstractions;

namespace sebkuw.Cqrs;

/// <summary>
/// Adds sebkuw CQRS handlers to a dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers CQRS command and query handlers from selected assemblies.
    /// </summary>
    /// <param name="services">Dependency injection container.</param>
    /// <param name="configure">Registration configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Action<CqrsRegistrationOptions> configure
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        CqrsRegistrationOptions options = new();
        configure(options);

        var assemblies = options.Assemblies
            .Distinct()
            .ToArray();

        if (assemblies.Length == 0)
            throw new InvalidOperationException(
                "No assemblies were configured for CQRS registration.");

        services.Scan(scan => scan
            .FromAssemblies(assemblies)

            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()

            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()

            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers CQRS handlers from the assembly containing the specified marker type.
    /// </summary>
    /// <typeparam name="T">Marker type used to identify the target assembly.</typeparam>
    /// <param name="services">Dependency injection container.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddCqrsFromAssemblyContaining<T>(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddCqrs(options =>
            options.RegisterServicesFromAssemblyContaining<T>());
    }
}
