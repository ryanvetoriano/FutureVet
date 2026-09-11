using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FutureVet.API.HealthChecks;

/// <summary>
/// Configuração de um serviço HTTP externo a ser monitorado.
/// Preenchida a partir da seção <c>HealthChecks:ExternalServices</c> do appsettings.
/// </summary>
public sealed class ExternalServiceOptions
{
    public const string SectionName = "HealthChecks:ExternalServices";

    /// <summary>Nome exibido no relatório de health check.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL de verificação. Não deve conter credenciais nem tokens.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Tempo máximo de espera. Impede que a API fique bloqueada em <c>/health/ready</c>.</summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Quando <c>true</c>, a indisponibilidade resulta em <c>Degraded</c> em vez de
    /// <c>Unhealthy</c> — útil para integrações não críticas.
    /// </summary>
    public bool Optional { get; set; }
}

/// <summary>
/// Health check genérico para dependências HTTP externas, usando
/// <see cref="IHttpClientFactory"/> e timeout próprio por serviço.
/// </summary>
/// <remarks>
/// A FutureVet hoje não consome nenhuma API de terceiros — a seção
/// <c>HealthChecks:ExternalServices</c> vem vazia e nenhuma instância é registrada.
/// O check existe para que qualquer integração futura seja monitorada apenas
/// adicionando uma entrada de configuração, sem alteração de código.
/// </remarks>
public sealed class ExternalServiceHealthCheck : IHealthCheck
{
    public const string HttpClientName = "health-checks";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalServiceOptions _options;
    private readonly ILogger<ExternalServiceHealthCheck> _logger;

    public ExternalServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        ExternalServiceOptions options,
        ILogger<ExternalServiceHealthCheck> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var falha = _options.Optional
            ? HealthStatus.Degraded
            : HealthStatus.Unhealthy;

        using var timeoutCts = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);

            using var response = await client.GetAsync(
                _options.Url,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    $"Serviço {_options.Name} respondeu {(int)response.StatusCode}.");
            }

            _logger.LogWarning(
                "Serviço externo {Servico} respondeu status {StatusCode}.",
                _options.Name,
                (int)response.StatusCode);

            return new HealthCheckResult(
                falha,
                $"Serviço {_options.Name} respondeu {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Serviço externo {Servico} excedeu o timeout de {Timeout}s.",
                _options.Name,
                _options.TimeoutSeconds);

            return new HealthCheckResult(
                falha,
                $"Serviço {_options.Name} excedeu o timeout de {_options.TimeoutSeconds}s.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Falha de rede ao consultar o serviço externo {Servico}.",
                _options.Name);

            return new HealthCheckResult(
                falha,
                $"Serviço {_options.Name} inacessível.");
        }
    }
}
