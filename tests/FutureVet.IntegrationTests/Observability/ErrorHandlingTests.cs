using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

/// <summary>
/// Validação do tratamento global de exceções: todas as falhas precisam sair como
/// ProblemDetails, com o status correto e sem vazar detalhes internos.
/// </summary>
[Collection(ApiCollection.Name)]
public class ErrorHandlingTests
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task TratamentoGlobal_RegraDeDominioViolada_Retorna400EmProblemDetails()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Fulano", "email-invalido", "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problema = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal("Requisição inválida", problema.GetProperty("title").GetString());
        Assert.Equal(400, problema.GetProperty("status").GetInt32());
        Assert.Equal("Email inválido.", problema.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TratamentoGlobal_RecursoInexistente_Retorna404EmProblemDetails()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdateUsuarioRequest("Nome Qualquer", "11999998888");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Usuario/{idInexistente}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problema = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal("Recurso não encontrado", problema.GetProperty("title").GetString());
        Assert.Equal(404, problema.GetProperty("status").GetInt32());
        Assert.Contains(idInexistente.ToString(), problema.GetProperty("detail").GetString()!);
    }

    [Fact]
    public async Task TratamentoGlobal_RespostaDeErro_IncluiTraceIdECorrelationId()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Fulano", "email-invalido", "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        var problema = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.True(problema.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));

        Assert.True(problema.TryGetProperty("correlationId", out var correlationId));
        Assert.False(string.IsNullOrWhiteSpace(correlationId.GetString()));
    }

    [Fact]
    public async Task TratamentoGlobal_QualquerErro_NaoRetornaStackTraceParaOCliente()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Fulano", "email-invalido", "SenhaSegura123", "12345678901", "11999998888");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        var corpo = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("stackTrace", corpo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("   at ", corpo);
        Assert.DoesNotContain("FutureVet.Application.Services", corpo);
    }

    [Fact]
    public async Task ModelBinding_JsonMalFormado_Retorna400SemQuebrarAApi()
    {
        // Arrange
        using var conteudo = new StringContent(
            "{ isso não é json válido", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/Usuario", conteudo);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Roteamento_GuidMalFormadoNaRota_Retorna404PelaRestricaoDeRota()
    {
        // Arrange
        const string idQueNaoEGuid = "isso-nao-e-um-guid";

        // Act
        var response = await _client.GetAsync($"/api/Usuario/{idQueNaoEGuid}");

        // Assert
        // A restrição :guid da rota não casa, então nenhum endpoint responde.
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Roteamento_RotaInexistente_Retorna404()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/RecursoQueNaoExiste");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
