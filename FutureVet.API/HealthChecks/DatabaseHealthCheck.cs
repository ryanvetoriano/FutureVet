using System.Diagnostics;
using FutureVet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FutureVet.API.HealthChecks;

/// <summary>
/// Verifica a conectividade real com o banco de dados configurado (Oracle em produção)
/// através de <c>Database.CanConnectAsync()</c>, que abre uma conexão de fato.
/// A descrição retornada nunca inclui a connection string.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    public const string Name = "database";

    private readonly FutureVetContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        FutureVetContext context,
        ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var provider = _context.Database.ProviderName ?? "desconhecido";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            if (!canConnect)
            {
                _logger.LogWarning(
                    "Health check do banco falhou: provider {Provider} não respondeu à conexão.",
                    provider);

                return HealthCheckResult.Unhealthy(
                    $"Não foi possível conectar ao banco de dados ({provider}).",
                    data: BuildData(provider, stopwatch));
            }

            return HealthCheckResult.Healthy(
                $"Conexão com o banco de dados estabelecida ({provider}).",
                data: BuildData(provider, stopwatch));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Timeout do health check: propagado para o pipeline decidir.
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Health check do banco lançou exceção para o provider {Provider}.",
                provider);

            // A exceção vai para o log; a resposta HTTP recebe apenas a mensagem genérica.
            return HealthCheckResult.Unhealthy(
                $"Falha ao verificar o banco de dados ({provider}).",
                data: BuildData(provider, stopwatch));
        }
    }

    private static IReadOnlyDictionary<string, object> BuildData(
        string provider,
        Stopwatch stopwatch)
        => new Dictionary<string, object>
        {
            ["provider"] = provider,
            ["elapsedMs"] = stopwatch.ElapsedMilliseconds
        };
}
