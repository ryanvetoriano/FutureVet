using System.Net;
using System.Text.Json;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

[Collection(ApiCollection.Name)]
public class HealthCheckEndpointsTests
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task HealthCheck_ApiEBancoDisponiveis_RetornaHealthy()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = await LerRelatorioAsync(response);
        Assert.Equal("Healthy", relatorio.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthCheck_ApiDisponivel_RetornaJsonEstruturadoComOsChecks()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        var relatorio = await LerRelatorioAsync(response);

        Assert.True(relatorio.TryGetProperty("totalDuration", out _));

        var checks = relatorio.GetProperty("checks").EnumerateArray().ToList();
        Assert.NotEmpty(checks);

        foreach (var check in checks)
        {
            Assert.False(string.IsNullOrWhiteSpace(check.GetProperty("name").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(check.GetProperty("status").GetString()));
            Assert.True(check.TryGetProperty("duration", out _));
        }
    }

    [Fact]
    public async Task HealthCheck_BancoConfigurado_IncluiOCheckDeBanco()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        var relatorio = await LerRelatorioAsync(response);

        var checkDeBanco = relatorio.GetProperty("checks")
            .EnumerateArray()
            .SingleOrDefault(c => c.GetProperty("name").GetString() == "database");

        Assert.Equal(JsonValueKind.Object, checkDeBanco.ValueKind);
        Assert.Equal("Healthy", checkDeBanco.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthCheck_QualquerCenario_NaoExpoeConnectionStringNemCredenciais()
    {
        // Arrange & Act
        var corpo = await _client.GetStringAsync("/health");

        // Assert
        Assert.DoesNotContain("Password", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("User Id", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DataSource", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthLive_ProcessoAtivo_RetornaHealthySemVerificarDependencias()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = await LerRelatorioAsync(response);
        Assert.Equal("Healthy", relatorio.GetProperty("status").GetString());

        var nomes = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        // Liveness responde apenas sobre o processo: o banco fica de fora de propósito.
        Assert.Contains("api", nomes);
        Assert.DoesNotContain("database", nomes);
    }

    [Fact]
    public async Task HealthReady_DependenciasDisponiveis_RetornaHealthyIncluindoOBanco()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = await LerRelatorioAsync(response);
        Assert.Equal("Healthy", relatorio.GetProperty("status").GetString());

        var nomes = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("database", nomes);
    }

    private static async Task<JsonElement> LerRelatorioAsync(HttpResponseMessage response)
    {
        var conteudo = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(conteudo).RootElement;
    }
}

/// <summary>
/// Cenário de banco indisponível. Usa <see cref="IClassFixture{TFixture}"/> porque exige
/// um host configurado de forma diferente do compartilhado pela collection fixture.
/// </summary>
public class HealthCheckComBancoIndisponivelTests
    : IClassFixture<UnavailableDatabaseFactory>
{
    private readonly HttpClient _client;

    public HealthCheckComBancoIndisponivelTests(UnavailableDatabaseFactory factory)
    {
        _client = factory.CriarClient();
    }

    [Fact]
    public async Task HealthCheck_BancoIndisponivel_Retorna503ComStatusUnhealthy()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Unhealthy", relatorio.GetProperty("status").GetString());

        var checkDeBanco = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Single(c => c.GetProperty("name").GetString() == "database");

        Assert.Equal("Unhealthy", checkDeBanco.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthLive_BancoIndisponivel_ContinuaHealthyPorqueOProcessoEstaAtivo()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        // Liveness não pode derrubar o pod por causa de uma dependência externa:
        // isso é responsabilidade do readiness.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Healthy", relatorio.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthReady_BancoIndisponivel_Retorna503PorqueNaoEstaProntaParaTrafego()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_BancoIndisponivel_NaoVazaDetalhesDaExcecao()
    {
        // Arrange & Act
        var corpo = await (await _client.GetAsync("/health")).Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("SqliteException", corpo);
        Assert.DoesNotContain("at Microsoft.Data.Sqlite", corpo);
        Assert.DoesNotContain("futurevet-diretorio-inexistente", corpo);
    }
}
