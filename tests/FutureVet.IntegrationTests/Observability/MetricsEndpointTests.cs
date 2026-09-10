using System.Net;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

[Collection(ApiCollection.Name)]
public class MetricsEndpointTests
{
    private readonly HttpClient _client;

    public MetricsEndpointTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Metrics_ExporterConfigurado_Retorna200NoFormatoDoPrometheus()
    {
        // Arrange
        // Gera tráfego para que o exporter tenha métricas a publicar.
        await _client.GetAsync("/api/Usuario");

        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.ToString();
        Assert.NotNull(contentType);
        Assert.Contains("text/plain", contentType);
    }

    [Fact]
    public async Task Metrics_AposRequisicoesHttp_ExpoeDuracaoEContagemDeRequisicoes()
    {
        // Arrange
        await _client.GetAsync("/api/Pet");
        await _client.GetAsync("/api/Vacina");

        // Act
        var corpo = await _client.GetStringAsync("/metrics");

        // Assert
        // A instrumentação do ASP.NET Core publica o histograma de duração, do qual
        // derivam contagem de requisições, tempo de resposta e taxa de erro por status.
        Assert.Contains("http_server_request_duration_seconds", corpo);
        Assert.Contains("http_route=", corpo);
        Assert.Contains("http_response_status_code=", corpo);
    }

    [Fact]
    public async Task Metrics_AposRequisicaoComErro_RegistraOStatusDeFalha()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        await _client.GetAsync($"/api/Usuario/{idInexistente}");

        // Act
        var corpo = await _client.GetStringAsync("/metrics");

        // Assert
        Assert.Contains("http_response_status_code=\"404\"", corpo);
    }

    [Fact]
    public async Task Metrics_RuntimeInstrumentado_ExpoeMetricasDoProcesso()
    {
        // Arrange & Act
        var corpo = await _client.GetStringAsync("/metrics");

        // Assert
        Assert.Contains("dotnet_", corpo);
    }

    [Fact]
    public async Task Metrics_QualquerCenario_NaoExpoeConnectionStringNemCredenciais()
    {
        // Arrange & Act
        var corpo = await _client.GetStringAsync("/metrics");

        // Assert
        Assert.DoesNotContain("Password", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("User Id", corpo, StringComparison.OrdinalIgnoreCase);
    }
}
