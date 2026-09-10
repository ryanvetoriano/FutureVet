using FutureVet.Application.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FutureVet.API.Extensions;

/// <summary>
/// Configuração do OpenTelemetry: tracing distribuído e métricas.
/// </summary>
public static class OpenTelemetryExtensions
{
    public const string ServiceName = "FutureVet.API";

    public static IServiceCollection AddOpenTelemetryConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: typeof(OpenTelemetryExtensions).Assembly
                        .GetName().Version?.ToString() ?? "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;

                        // Sondas de health e o scrape de métricas gerariam um trace
                        // a cada poucos segundos, sem valor de diagnóstico.
                        options.Filter = httpContext =>
                            !EhEndpointDeInfraestrutura(httpContext.Request.Path);
                    })
                    .AddHttpClientInstrumentation(options =>
                        options.RecordException = true)
                    // Cada consulta ao banco vira um span filho, com o texto do
                    // comando SQL. Mantemos a configuração padrão de propósito: ela
                    // NÃO captura os valores dos parâmetros, que carregam dados
                    // pessoais (e-mail, CPF, telefone).
                    .AddEntityFrameworkCoreInstrumentation()
                    // Spans da camada de Application: o elo entre o Controller e o EF Core.
                    .AddSource(ApplicationDiagnostics.ActivitySourceName);

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
                else if (environment.IsDevelopment())
                    tracing.AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    // http.server.request.duration (contagem, duração e taxa de erro
                    // por status) e http.server.active_requests.
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // GC, threads e exceções do processo.
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
            });

        return services;
    }

    /// <summary>
    /// Expõe as métricas em <c>/metrics</c> no formato de exposição do Prometheus.
    /// </summary>
    public static IApplicationBuilder UseMetricsEndpoint(this WebApplication app)
    {
        app.UseOpenTelemetryPrometheusScrapingEndpoint("/metrics");
        return app;
    }

    private static bool EhEndpointDeInfraestrutura(PathString path)
        => path.StartsWithSegments("/health") || path.StartsWithSegments("/metrics");
}
