using FutureVet.Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FutureVet.Infrastructure.NoSql;

/// <summary>
/// Ponto de acesso às coleções do MongoDB, no papel que o <c>FutureVetContext</c> cumpre
/// para o Oracle. Registrado como singleton: o <see cref="IMongoClient"/> é thread-safe e
/// mantém o pool de conexões, então deve existir uma única instância por processo.
/// </summary>
public sealed class MongoDbContext
{
    public const string ColecaoRegistrosClinicos = "registros_clinicos";

    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient client, IOptions<MongoDbOptions> options)
    {
        MongoMapeamentos.Registrar();

        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public string DatabaseName => _database.DatabaseNamespace.DatabaseName;

    public IMongoCollection<RegistroClinico> RegistrosClinicos
        => _database.GetCollection<RegistroClinico>(ColecaoRegistrosClinicos);

    /// <summary>Comando <c>ping</c>: confirma que o servidor responde, sem tocar nos dados.</summary>
    public Task PingAsync(CancellationToken cancellationToken = default)
        => _database.RunCommandAsync<BsonDocument>(
            new BsonDocument("ping", 1),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Índices que sustentam as consultas da API. <c>CreateMany</c> é idempotente: índices
    /// já existentes com a mesma definição são mantidos.
    /// </summary>
    public Task CriarIndicesAsync(CancellationToken cancellationToken = default)
    {
        var indice = Builders<RegistroClinico>.IndexKeys;

        return RegistrosClinicos.Indexes.CreateManyAsync(
        [
            // Prontuário de um pet, do mais recente para o mais antigo: a consulta mais comum.
            new CreateIndexModel<RegistroClinico>(
                indice.Ascending(x => x.PetId).Descending(x => x.DataRegistro),
                new CreateIndexOptions { Name = "ix_pet_data" }),

            new CreateIndexModel<RegistroClinico>(
                indice.Ascending(x => x.Tipo),
                new CreateIndexOptions { Name = "ix_tipo" }),

            // Índice multikey: cada tag do array vira uma entrada do índice.
            new CreateIndexModel<RegistroClinico>(
                indice.Ascending(x => x.Tags),
                new CreateIndexOptions { Name = "ix_tags" })
        ],
        cancellationToken);
    }
}
