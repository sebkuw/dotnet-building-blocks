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
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
