using FutureVet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Sobe a API em memória para os testes de integração.
/// </summary>
/// <remarks>
/// <para><b>Banco de dados.</b> Os testes rodam contra <b>Oracle</b>, o mesmo SGBD usado em
/// produção e com o mesmo provider (<c>Oracle.EntityFrameworkCore</c>). Nenhum banco
/// substituto é utilizado: os testes exercitam os tipos, as constraints e o SQL do Oracle
/// de verdade. A connection string vem de User Secrets ou de variável de ambiente
/// (ver <see cref="TestOracleSettings"/>) e nunca é versionada.</para>
/// <para>O schema <b>não</b> é criado nem apagado pelos testes — ele já existe, aplicado
/// pelas migrations. Cada teste cria os próprios registros e a
/// <see cref="ApiFixture"/> os remove ao final da execução.</para>
/// <para><b>Serviços externos.</b> O ambiente <c>Testing</c> carrega
/// <c>appsettings.Testing.json</c>, que deixa <c>HealthChecks:ExternalServices</c> vazio e
/// <c>OpenTelemetry:OtlpEndpoint</c> em branco. Nenhuma telemetria sai da máquina.</para>
/// </remarks>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Connection string usada pelo host. Sobrescrita por
    /// <see cref="UnavailableDatabaseFactory"/> para simular o banco fora do ar.
    /// </summary>
    protected virtual string ConnectionString => TestOracleSettings.ObterConnectionString();

    /// <summary>
    /// Chave de assinatura exclusiva desta execução, gerada em memória. Os testes nunca
    /// dependem — nem tomam conhecimento — da chave real de desenvolvimento ou de produção.
    /// </summary>
    private static readonly string ChaveDeTeste =
        Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // User Secrets só são carregados no ambiente Development; no ambiente Testing a
        // chave precisa vir daqui, senão a validação do JwtOptions barra a inicialização.
        builder.UseSetting("Jwt:SigningKey", ChaveDeTeste);

        builder.ConfigureServices(services =>
        {
            // O Program.cs registra o DbContext com a connection string do appsettings.
            // Aqui o registro é trocado pelo do banco de teste — mesmo provider Oracle,
            // credenciais vindas de fora do repositório.
            RemoverRegistroDoDbContext(services);

            services.AddDbContext<FutureVetContext>(options =>
                options.UseOracle(ConnectionString));
        });
    }

    /// <summary>
    /// Confirma que o banco de teste está acessível antes de a suíte começar, para que uma
    /// credencial ausente ou um schema não aplicado apareçam como uma mensagem clara em vez
    /// de dezenas de falhas de conexão.
    /// </summary>
    public async Task VerificarConectividadeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FutureVetContext>();

        if (!await context.Database.CanConnectAsync())
        {
            throw new InvalidOperationException(
                "Não foi possível conectar ao Oracle de teste. Verifique a connection string "
                + "e a disponibilidade do servidor.");
        }
    }

    /// <summary>
    /// Cliente HTTP configurado para os testes. Redirecionamentos automáticos ficam
    /// desligados para que o teste enxergue o status realmente devolvido pela API.
    /// </summary>
    public HttpClient CriarClient()
        => CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    /// <summary>
    /// Abre um escopo com o <see cref="FutureVetContext"/> apontando para o banco de teste.
    /// Usado apenas pela limpeza de dados — os testes em si passam pelos endpoints HTTP.
    /// </summary>
    public IServiceScope CriarEscopo() => Services.CreateScope();

    /// <summary>
    /// Remove o registro do DbContext feito no <c>Program.cs</c> para que o registro de teste
    /// ocupe seu lugar. Sem isso, os dois coexistiriam e o EF Core resolveria o errado.
    /// </summary>
    private static void RemoverRegistroDoDbContext(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<FutureVetContext>>();
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll<FutureVetContext>();
        services.RemoveAll<IDbContextOptionsConfiguration<FutureVetContext>>();
    }
}
