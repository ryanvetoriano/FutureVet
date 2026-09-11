using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Auth;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

[Collection(ApiCollection.Name)]
public class AuthEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public AuthEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task Login_CredenciaisValidas_Retorna200ComTokenJwt()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new LoginRequest(usuario.Email, ApiTestData.SenhaPadrao);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(ApiJson.Options);

        Assert.NotNull(login);
        Assert.Equal(usuario.Id, login.Id);
        Assert.Equal(usuario.Email, login.Email);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        // Um JWT tem tres segmentos separados por ponto: header.payload.assinatura.
        Assert.Equal(3, login.Token.Split('.').Length);
        Assert.True(login.ExpiraEm > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_SenhaIncorreta_Retorna401()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new LoginRequest(usuario.Email, "SenhaCompletamenteErrada999");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailNaoCadastrado_Retorna401()
    {
        // Arrange
        var request = new LoginRequest(ApiTestData.EmailDeTeste(), ApiTestData.SenhaPadrao);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmailNaoCadastradoOuSenhaErrada_RetornamAMesmaResposta()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var senhaErrada = await _client.PostAsJsonAsync(
            "/api/Auth/login", new LoginRequest(usuario.Email, "OutraSenha123"), ApiJson.Options);

        var emailInexistente = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest(ApiTestData.EmailDeTeste(), ApiTestData.SenhaPadrao),
            ApiJson.Options);

        // Assert
        // Respostas identicas de proposito: diferencia-las revelaria quais e-mails existem.
        Assert.Equal(senhaErrada.StatusCode, emailInexistente.StatusCode);
        Assert.Equal(
            await senhaErrada.Content.ReadAsStringAsync(),
            await emailInexistente.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_SenhaVazia_Retorna400()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new LoginRequest(usuario.Email, "   ");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_RespostaDeSucesso_NaoDevolveASenhaDoUsuario()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest(usuario.Email, ApiTestData.SenhaPadrao),
            ApiJson.Options);

        // Assert
        var corpo = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(ApiTestData.SenhaPadrao, corpo);
        Assert.DoesNotContain("senha", corpo, StringComparison.OrdinalIgnoreCase);
    }
}
