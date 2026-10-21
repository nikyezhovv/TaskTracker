using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TaskTracker.Shared.Observability;

public static class HealthExtensions
{
    public static IServiceCollection AddHealth(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IEndpointRouteBuilder MapHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        endpoints.MapHealthChecks("/health/ready");

        return endpoints;
    }
}
