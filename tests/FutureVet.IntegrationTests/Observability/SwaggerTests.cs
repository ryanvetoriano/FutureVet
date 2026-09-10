using System.Net;
using System.Text.Json;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

/// <summary>
/// Garante que a documentação Swagger construída nas sprints anteriores continua
/// intacta depois das alterações de observabilidade no <c>Program.cs</c>.
/// </summary>
[Collection(ApiCollection.Name)]
public class SwaggerTests
{
    private readonly HttpClient _client;

    public SwaggerTests(ApiFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Swagger_ApiDisponivel_ServeODocumentoOpenApi()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var documento = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        Assert.Equal("FutureVet API", documento.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task Swagger_DocumentoOpenApi_MantemTodosOsControllersDasSprintsAnteriores()
    {
        // Arrange & Act
        var documento = JsonDocument
            .Parse(await _client.GetStringAsync("/swagger/v1/swagger.json"))
            .RootElement;

        // Assert
        var rotas = documento.GetProperty("paths")
            .EnumerateObject()
            .Select(p => p.Name)
            .ToList();

        Assert.Contains("/api/Usuario", rotas);
        Assert.Contains("/api/Pet", rotas);
        Assert.Contains("/api/Vacina", rotas);
        Assert.Contains("/api/Consulta", rotas);
    }
}
