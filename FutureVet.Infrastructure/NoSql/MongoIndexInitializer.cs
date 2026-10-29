using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FutureVet.Infrastructure.NoSql;

/// <summary>
/// Cria os índices do MongoDB em segundo plano, logo após a inicialização.
/// </summary>
/// <remarks>
/// Roda fora do caminho de inicialização de propósito: se o MongoDB estiver fora do ar, a API
/// sobe mesmo assim e continua servindo tudo o que depende apenas do Oracle. A falha é
/// registrada em log e aparece como <c>Degraded</c> em <c>/health/ready</c>.
/// </remarks>
public sealed class MongoIndexInitializer : BackgroundService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoIndexInitializer> _logger;

    public MongoIndexInitializer(
        MongoDbContext context,
        ILogger<MongoIndexInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _context.CriarIndicesAsync(stoppingToken);

            _logger.LogInformation(
                "Índices do MongoDB verificados no banco {Database}.",
                _context.DatabaseName);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Aplicação encerrando antes de concluir: nada a registrar.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Não foi possível criar os índices do MongoDB. As consultas continuam " +
                "funcionando, porém sem índice, até a próxima inicialização.");
        }
    }
}
