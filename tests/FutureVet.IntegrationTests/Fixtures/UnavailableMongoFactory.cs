using Microsoft.AspNetCore.Hosting;

namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Variante da factory com o MongoDB apontando para um endereço local onde nada escuta: a
/// conexão é recusada de verdade pelo sistema operacional, sem mock no caminho. O Oracle
/// continua disponível, o que permite verificar que a queda do MongoDB degrada a API sem
/// derrubar o que depende apenas do banco relacional.
/// </summary>
public sealed class UnavailableMongoFactory : CustomWebApplicationFactory
{
    protected override string MongoConnectionString => "mongodb://127.0.0.1:1";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Falha rápida: o driver desiste de encontrar um servidor em 2 s.
        builder.UseSetting("MongoDb:ServerSelectionTimeoutSeconds", "2");

        base.ConfigureWebHost(builder);
    }
}
