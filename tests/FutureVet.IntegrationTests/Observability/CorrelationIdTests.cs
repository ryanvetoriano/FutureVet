using System.Net.Http.Json;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

[Collection(ApiCollection.Name)]
public class CorrelationIdTests
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly HttpClient _client;

    public CorrelationIdTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task CorrelationId_ClienteNaoEnviaHeader_ApiGeraUmIdentificadorNaResposta()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/Usuario");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.TryGetValues(HeaderName, out var valores));

        var correlationId = valores!.Single();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task CorrelationId_ClienteEnviaHeader_ApiReaproveitaOMesmoValor()
    {
        // Arrange
        const string correlationIdDoCliente = "pedido-12345-do-cliente";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/Usuario");
        request.Headers.Add(HeaderName, correlationIdDoCliente);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(correlationIdDoCliente, response.Headers.GetValues(HeaderName).Single());
    }

    [Fact]
    public async Task CorrelationId_HeaderVazio_ApiGeraUmIdentificadorProprio()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/Usuario");
        request.Headers.TryAddWithoutValidation(HeaderName, string.Empty);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var correlationId = response.Headers.GetValues(HeaderName).Single();
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task CorrelationId_RequisicoesDistintasSemHeader_RecebemIdentificadoresDiferentes()
    {
        // Arrange & Act
        var primeira = await _client.GetAsync("/api/Usuario");
        var segunda = await _client.GetAsync("/api/Usuario");

        // Assert
        Assert.NotEqual(
            primeira.Headers.GetValues(HeaderName).Single(),
            segunda.Headers.GetValues(HeaderName).Single());
    }

    [Fact]
    public async Task CorrelationId_RespostaDeErro_TambemCarregaOIdentificador()
    {
        // Arrange
        const string correlationIdDoCliente = "requisicao-que-falha";
        var payloadInvalido = new CreateUsuarioRequest(
            "Fulano", "email-sem-arroba", "SenhaSegura123", "12345678901", "11999998888");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/Usuario")
        {
            Content = JsonContent.Create(payloadInvalido, options: ApiJson.Options)
        };
        request.Headers.Add(HeaderName, correlationIdDoCliente);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(correlationIdDoCliente, response.Headers.GetValues(HeaderName).Single());

        // O ProblemDetails também expõe o identificador, permitindo ligar
        // o erro visto pelo cliente aos logs do servidor.
        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains(correlationIdDoCliente, corpo);
    }

    [Fact]
    public async Task CorrelationId_HealthCheck_TambemRecebeOIdentificador()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.TryGetValues(HeaderName, out _));
    }
}
