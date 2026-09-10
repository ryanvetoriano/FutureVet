using System.Net;
using System.Text.Json;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

/// <summary>
/// Health check de dependências HTTP externas. Cada classe usa
/// <see cref="IClassFixture{TFixture}"/> porque precisa de um host com configuração
/// própria, diferente da compartilhada pela collection fixture.
/// </summary>
public class ExternalServiceHealthCheckTests
    : IClassFixture<ExternalServiceIndisponivelFactory>
{
    private readonly HttpClient _client;

    public ExternalServiceHealthCheckTests(ExternalServiceIndisponivelFactory factory)
    {
        _client = factory.CriarClient();
    }

    [Fact]
    public async Task HealthReady_ServicoExternoConfigurado_IncluiOCheckNoRelatorio()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var nomes = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.Contains("servico-externo", nomes);
    }

    [Fact]
    public async Task HealthReady_ServicoExternoObrigatorioForaDoAr_Retorna503()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var check = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Single(c => c.GetProperty("name").GetString() == "servico-externo");

        Assert.Equal("Unhealthy", check.GetProperty("status").GetString());
    }

    [Fact]
    public async Task HealthLive_ServicoExternoForaDoAr_ContinuaHealthy()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        // Liveness não depende de terceiros: o processo continua saudável.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        var nomes = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        Assert.DoesNotContain("servico-externo", nomes);
    }

    [Fact]
    public async Task HealthReady_ServicoExternoForaDoAr_NaoVazaAUrlNemDetalhesInternos()
    {
        // Arrange & Act
        var corpo = await (await _client.GetAsync("/health/ready")).Content.ReadAsStringAsync();

        // Assert
        Assert.DoesNotContain("127.0.0.1", corpo);
        Assert.DoesNotContain("HttpRequestException", corpo);
    }
}

public class ExternalServiceOpcionalHealthCheckTests
    : IClassFixture<ExternalServiceOpcionalFactory>
{
    private readonly HttpClient _client;

    public ExternalServiceOpcionalHealthCheckTests(ExternalServiceOpcionalFactory factory)
    {
        _client = factory.CriarClient();
    }

    [Fact]
    public async Task HealthReady_ServicoExternoOpcionalForaDoAr_Retorna200ComStatusDegraded()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        // Degraded não é falha: a API segue apta a receber tráfego.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var relatorio = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal("Degraded", relatorio.GetProperty("status").GetString());

        var check = relatorio.GetProperty("checks")
            .EnumerateArray()
            .Single(c => c.GetProperty("name").GetString() == "servico-externo");

        Assert.Equal("Degraded", check.GetProperty("status").GetString());
    }
}
