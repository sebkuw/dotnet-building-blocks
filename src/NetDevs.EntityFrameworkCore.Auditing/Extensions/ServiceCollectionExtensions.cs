using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetDevs.EntityFrameworkCore.Auditing.Abstractions;
using NetDevs.EntityFrameworkCore.Auditing.Interceptors;
using NetDevs.EntityFrameworkCore.Auditing.Providers;

namespace NetDevs.EntityFrameworkCore.Auditing.Extensions;

/// <summary>
/// Provides dependency injection extensions for auditing services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers EF Core auditing services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddEfCoreAuditing(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddSingleton<EntityLifecycleInterceptor>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }

    /// <summary>
    /// Attaches all NetDevs auditing and lifecycle interceptors to an EF Core context.
    /// </summary>
    /// <param name="optionsBuilder">The context options builder.</param>
    /// <param name="serviceProvider">The service provider used to resolve the interceptors.</param>
    /// <returns>The same options builder instance.</returns>
    public static DbContextOptionsBuilder AddNetDevsAuditingInterceptors(
        this DbContextOptionsBuilder optionsBuilder,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        return optionsBuilder.AddInterceptors(
            serviceProvider.GetRequiredService<EntityLifecycleInterceptor>(),
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    }
}
