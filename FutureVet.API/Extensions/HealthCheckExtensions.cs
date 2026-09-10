using FutureVet.API.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FutureVet.API.Extensions;

/// <summary>
/// Registro e exposição dos health checks da aplicação.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>Health checks que respondem em <c>/health/live</c>.</summary>
    public const string LiveTag = "live";

    /// <summary>Health checks que respondem em <c>/health/ready</c>.</summary>
    public const string ReadyTag = "ready";

    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var externalServices = configuration
            .GetSection(ExternalServiceOptions.SectionName)
            .Get<List<ExternalServiceOptions>>() ?? [];

        // Só criamos o HttpClient quando existe de fato algum serviço externo a monitorar.
        if (externalServices.Count > 0)
        {
            services.AddHttpClient(ExternalServiceHealthCheck.HttpClientName);
        }

        var builder = services.AddHealthChecks();

        // API: confirma que o processo está vivo e servindo requisições.
        builder.AddCheck(
            "api",
            () => HealthCheckResult.Healthy("API em execução."),
            tags: [LiveTag, ReadyTag]);

        // Banco de dados: dependência externa, entra apenas em /health/ready.
        builder.AddCheck<DatabaseHealthCheck>(
            DatabaseHealthCheck.Name,
            failureStatus: HealthStatus.Unhealthy,
            tags: [ReadyTag, "db"],
            timeout: TimeSpan.FromSeconds(10));

        foreach (var externo in externalServices)
        {
            if (string.IsNullOrWhiteSpace(externo.Name) ||
                string.IsNullOrWhiteSpace(externo.Url))
            {
                continue;
            }

            builder.Add(new HealthCheckRegistration(
                externo.Name,
                sp => new ExternalServiceHealthCheck(
                    sp.GetRequiredService<IHttpClientFactory>(),
                    externo,
                    sp.GetRequiredService<ILogger<ExternalServiceHealthCheck>>()),
                failureStatus: externo.Optional ? HealthStatus.Degraded : HealthStatus.Unhealthy,
                tags: [ReadyTag, "external"],
                // Rede de segurança acima do timeout interno do próprio check.
                timeout: TimeSpan.FromSeconds(externo.TimeoutSeconds + 2)));
        }

        return services;
    }

    /// <summary>
    /// Mapeia <c>/health</c> (visão completa), <c>/health/live</c> (liveness, sem
    /// dependências externas) e <c>/health/ready</c> (readiness, com dependências).
    /// </summary>
    public static IEndpointRouteBuilder MapApplicationHealthChecks(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new()
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        endpoints.MapHealthChecks("/health/live", new()
        {
            Predicate = registration => registration.Tags.Contains(LiveTag),
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        endpoints.MapHealthChecks("/health/ready", new()
        {
            Predicate = registration => registration.Tags.Contains(ReadyTag),
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        return endpoints;
    }
}
