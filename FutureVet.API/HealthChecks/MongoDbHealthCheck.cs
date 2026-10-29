using System.Diagnostics;
using FutureVet.Infrastructure.NoSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FutureVet.API.HealthChecks;

/// <summary>
/// Verifica a conectividade real com o MongoDB através do comando <c>ping</c>, que exige
/// um servidor respondendo de fato. A descrição retornada nunca inclui a connection string.
/// </summary>
/// <remarks>
/// Registrado com <see cref="HealthStatus.Degraded"/> como status de falha: o MongoDB guarda
/// apenas o prontuário. Se ele cair, usuários, pets, vacinas e consultas (Oracle) continuam
/// funcionando, então a instância não deve sair do balanceamento por isso.
/// </remarks>
public sealed class MongoDbHealthCheck : IHealthCheck
{
    public const string Name = "mongodb";

    private readonly MongoDbContext _context;
    private readonly ILogger<MongoDbHealthCheck> _logger;

    public MongoDbHealthCheck(
        MongoDbContext context,
        ILogger<MongoDbHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _context.PingAsync(cancellationToken);
            stopwatch.Stop();

            return HealthCheckResult.Healthy(
                $"MongoDB respondeu ao ping (banco {_context.DatabaseName}).",
                data: BuildData(stopwatch));
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
                "Health check do MongoDB falhou para o banco {Database}.",
                _context.DatabaseName);

            // A exceção vai para o log; a resposta HTTP recebe apenas a mensagem genérica.
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                $"Falha ao conectar ao MongoDB (banco {_context.DatabaseName}).",
                data: BuildData(stopwatch));
        }
    }

    private IReadOnlyDictionary<string, object> BuildData(Stopwatch stopwatch)
        => new Dictionary<string, object>
        {
            ["database"] = _context.DatabaseName,
            ["elapsedMs"] = stopwatch.ElapsedMilliseconds
        };
}
