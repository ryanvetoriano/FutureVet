namespace FutureVet.Infrastructure.NoSql;

/// <summary>
/// Configuração do MongoDB (seção <c>MongoDb</c> do appsettings).
/// </summary>
/// <remarks>
/// A connection string fica em <c>ConnectionStrings:MongoConnection</c>, ao lado da do Oracle,
/// para que credenciais de um cluster remoto (ex.: MongoDB Atlas) sigam o mesmo caminho:
/// User Secrets ou variável de ambiente, nunca o repositório.
/// </remarks>
public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";
    public const string ConnectionStringName = "MongoConnection";

    public string DatabaseName { get; set; } = "futurevet";

    /// <summary>
    /// Tempo máximo para encontrar um servidor disponível. Mantido curto para que uma queda do
    /// MongoDB apareça rápido no health check, em vez de prender requisições por 30 s (o padrão
    /// do driver). Aceita de 1 a 60.
    /// </summary>
    public int ServerSelectionTimeoutSeconds { get; set; } = 5;
}
