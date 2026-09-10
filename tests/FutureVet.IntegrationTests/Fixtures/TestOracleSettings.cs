using Microsoft.Extensions.Configuration;

namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Resolve a connection string do Oracle usada pelos testes de integração.
/// </summary>
/// <remarks>
/// A credencial <b>nunca</b> é versionada. Ela é lida, nesta ordem de precedência:
/// <list type="number">
///   <item>variável de ambiente <c>ConnectionStrings__OracleConnection</c>;</item>
///   <item>User Secrets deste projeto de teste (<c>dotnet user-secrets</c>);</item>
///   <item><c>appsettings.Testing.json</c>, que no repositório contém apenas um placeholder vazio.</item>
/// </list>
/// </remarks>
public static class TestOracleSettings
{
    public const string ConnectionStringName = "OracleConnection";

    private static readonly Lazy<IConfigurationRoot> Configuration = new(() =>
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddUserSecrets(typeof(TestOracleSettings).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build());

    /// <summary>
    /// Connection string do Oracle de teste. Lança com uma mensagem acionável quando não
    /// está configurada — falhar explicitamente é preferível a silenciar os testes.
    /// </summary>
    public static string ObterConnectionString()
    {
        var connectionString = Configuration.Value.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                """
                Os testes de integração rodam contra um banco Oracle real e a connection string
                não foi configurada.

                Configure-a com User Secrets (recomendado, fica fora do repositório):

                    dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL" --project tests/FutureVet.IntegrationTests

                Ou exporte a variável de ambiente:

                    ConnectionStrings__OracleConnection=...

                O schema precisa estar criado (dotnet ef database update).
                """);
        }

        return connectionString;
    }

    /// <summary>
    /// Indica se a connection string foi configurada, sem lançar.
    /// </summary>
    public static bool EstaConfigurada()
        => !string.IsNullOrWhiteSpace(
            Configuration.Value.GetConnectionString(ConnectionStringName));
}
