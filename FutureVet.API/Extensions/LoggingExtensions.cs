using FutureVet.API.Middleware;
using Serilog;
using Serilog.Events;

namespace FutureVet.API.Extensions;

/// <summary>
/// Configuração do logging estruturado com Serilog.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Substitui o provider de log padrão pelo Serilog, lido de
    /// <c>appsettings.json</c> (seção <c>Serilog</c>). Isso permite ajustar
    /// níveis e sinks por ambiente sem recompilar.
    /// </summary>
    /// <remarks>
    /// <c>preserveStaticLogger: true</c> mantém o logger do host independente do
    /// <c>Serilog.Log</c> estático. Sem isso, o logger de bootstrap seria "congelado"
    /// na construção do host e um segundo host no mesmo processo — exatamente o que os
    /// testes de integração fazem ao criar mais de uma <c>WebApplicationFactory</c> —
    /// falharia com <i>The logger is already frozen</i>. Os componentes da aplicação
    /// usam <c>ILogger&lt;T&gt;</c> vindo da injeção de dependência e recebem, esses sim,
    /// o logger completo configurado aqui.
    /// </remarks>
    public static IHostBuilder AddSerilogLogging(this ConfigureHostBuilder host)
    {
        host.UseSerilog(
            (context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "FutureVet.API")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName),
            preserveStaticLogger: true);

        return host;
    }

    /// <summary>
    /// Log de uma linha por requisição HTTP com método, rota, status, duração e
    /// correlation ID. Headers de autenticação e query strings não são registrados.
    /// </summary>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        // Por padrão o middleware do Serilog escreve no Serilog.Log estático, que aqui é
        // apenas o logger de bootstrap (console). Apontá-lo para o logger do contêiner de
        // DI faz o log de requisições também chegar ao arquivo e receber os enrichers.
        var loggerDaAplicacao = app.ApplicationServices.GetService<Serilog.ILogger>();

        app.UseSerilogRequestLogging(options =>
        {
            options.Logger = loggerDaAplicacao;

            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} respondeu {StatusCode} em {Elapsed:0.0000} ms";

            options.GetLevel = DefinirNivel;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Enriquecimento deliberadamente restrito: nada de Authorization,
                // cookies, corpo da requisição ou query string, que podem carregar
                // tokens e dados pessoais.
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);

                if (httpContext.Items.TryGetValue(
                        CorrelationIdMiddleware.HeaderName, out var correlationId))
                {
                    diagnosticContext.Set("CorrelationId", correlationId);
                }

                var endpoint = httpContext.GetEndpoint();
                if (endpoint is not null)
                    diagnosticContext.Set("Endpoint", endpoint.DisplayName);
            };
        });

        return app;
    }

    private static LogEventLevel DefinirNivel(
        HttpContext httpContext,
        double elapsedMs,
        Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        // Health checks e /metrics são consultados por sondas com alta frequência:
        // mantê-los em Information poluiria o log sem agregar informação.
        if (EhEndpointDeInfraestrutura(httpContext.Request.Path))
            return LogEventLevel.Debug;

        return LogEventLevel.Information;
    }

    private static bool EhEndpointDeInfraestrutura(PathString path)
        => path.StartsWithSegments("/health") || path.StartsWithSegments("/metrics");
}
