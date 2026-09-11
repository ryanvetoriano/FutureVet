using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Domain.Enums;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

/// <summary>
/// Autorizacao nos endpoints protegidos. As operacoes de escrita (POST, PUT e DELETE)
/// exigem um JWT valido; as de leitura permanecem publicas.
/// </summary>
/// <remarks>
/// Estes testes usam um <see cref="HttpClient"/> proprio, <b>sem</b> o token que a
/// <see cref="ApiFixture"/> configura no client compartilhado.
/// </remarks>
[Collection(ApiCollection.Name)]
public class AutorizacaoEndpointsTests
{
    private readonly HttpClient _semToken;
    private readonly ApiTestData _dados;

    public AutorizacaoEndpointsTests(ApiFixture fixture)
    {
        _semToken = fixture.Factory.CriarClient();
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task Post_SemAutenticacao_Retorna401()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 32.5m, usuario.Id);

        // Act
        var response = await _semToken.PostAsJsonAsync("/api/Pet", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostUsuario_SemAutenticacao_Retorna201PorqueCadastroEPublico()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Novo Cadastro", ApiTestData.EmailDeTeste(), ApiTestData.SenhaPadrao,
            ApiTestData.GerarCpf(), "11999998888");

        // Act
        var response = await _semToken.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        // Cadastro e o ponto de entrada: sem ele nao haveria como obter um token.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criado = await response.Content.ReadFromJsonAsync<UsuarioResponse>(ApiJson.Options);
        Assert.NotNull(criado);
        _dados.Registrar(criado.Id);
    }

    [Fact]
    public async Task Put_SemAutenticacao_Retorna401()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new UpdateUsuarioRequest("Nome Novo", "11911112222");

        // Act
        var response = await _semToken.PutAsJsonAsync(
            $"/api/Usuario/{usuario.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SemAutenticacao_Retorna401()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _semToken.DeleteAsync($"/api/Usuario/{usuario.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComTokenInvalido_Retorna401()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new CreatePetRequest(
            "Token Falso", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 10m, usuario.Id);

        using var comTokenFalso = new HttpRequestMessage(HttpMethod.Post, "/api/Pet")
        {
            Content = JsonContent.Create(request, options: ApiJson.Options)
        };
        comTokenFalso.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJmYWxzbyJ9.assinatura_invalida");

        // Act
        var response = await _semToken.SendAsync(comTokenFalso);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ComAutenticacaoValida_Retorna201()
    {
        // Arrange
        var login = await _dados.AutenticarAsync();
        var usuario = await _dados.CriarUsuarioAsync();

        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 32.5m, usuario.Id);

        using var autenticado = new HttpRequestMessage(HttpMethod.Post, "/api/Pet")
        {
            Content = JsonContent.Create(request, options: ApiJson.Options)
        };
        autenticado.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);

        // Act
        var response = await _semToken.SendAsync(autenticado);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criado = await response.Content.ReadFromJsonAsync<PetResponse>(ApiJson.Options);
        Assert.NotNull(criado);
        Assert.Equal("Thor", criado.NomePet);
    }

    [Fact]
    public async Task Get_SemAutenticacao_Retorna200PorqueLeituraEPublica()
    {
        // Arrange & Act
        var response = await _semToken.GetAsync("/api/Usuario");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthChecksEMetricas_SemAutenticacao_ContinuamAcessiveis()
    {
        // Arrange & Act
        var health = await _semToken.GetAsync("/health");
        var metrics = await _semToken.GetAsync("/metrics");

        // Assert
        // Sondas de orquestrador e o scrape do Prometheus nao enviam token.
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal(HttpStatusCode.OK, metrics.StatusCode);
    }
}
