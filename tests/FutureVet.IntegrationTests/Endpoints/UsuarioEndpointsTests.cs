using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

[Collection(ApiCollection.Name)]
public class UsuarioEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public UsuarioEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetAll_ApiDisponivel_Retorna200ComListaDeUsuarios()
    {
        // Arrange
        var usuarioCriado = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.GetAsync("/api/Usuario");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var usuarios = await response.Content
            .ReadFromJsonAsync<List<UsuarioResponse>>(ApiJson.Options);

        Assert.NotNull(usuarios);
        Assert.Contains(usuarios, u => u.Id == usuarioCriado.Id);
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComLocationHeader()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "Maria Souza",
            ApiTestData.EmailDeTeste(),
            "SenhaSegura123",
            ApiTestData.GerarCpf(),
            "11955554444");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var criado = await response.Content
            .ReadFromJsonAsync<UsuarioResponse>(ApiJson.Options);

        Assert.NotNull(criado);

        // Criado fora dos helpers: registra para que a fixture o remova do Oracle.
        _dados.Registrar(criado.Id);

        Assert.NotEqual(Guid.Empty, criado.Id);
        Assert.Equal(request.Nome, criado.Nome);
        Assert.Equal(request.Email, criado.Email);
    }

    [Fact]
    public async Task Post_EmailSemArroba_Retorna400ComProblemDetails()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "João Inválido",
            "email-sem-arroba",
            "SenhaSegura123",
            ApiTestData.GerarCpf(),
            "11955554444");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email inválido.", corpo);
        // O tratamento global não pode vazar detalhes internos para o cliente.
        Assert.DoesNotContain("StackTrace", corpo);
        Assert.DoesNotContain("FutureVet.Domain.Exceptions", corpo);
    }

    [Fact]
    public async Task Post_SenhaCurta_Retorna400()
    {
        // Arrange
        var request = new CreateUsuarioRequest(
            "João Silva",
            ApiTestData.EmailDeTeste(),
            "curta",
            ApiTestData.GerarCpf(),
            "11955554444");

        // Act
        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_UsuarioExistente_Retorna200ComOUsuario()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.GetAsync($"/api/Usuario/{usuario.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var encontrado = await response.Content
            .ReadFromJsonAsync<UsuarioResponse>(ApiJson.Options);

        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado.Id);
        Assert.Equal(usuario.Email, encontrado.Email);
    }

    [Fact]
    public async Task GetById_UsuarioInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Usuario/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmail_EmailMalFormado_Retorna400()
    {
        // Arrange
        const string emailInvalido = "sem-arroba";

        // Act
        var response = await _client.GetAsync($"/api/Usuario/email/{emailInvalido}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmail_EmailCadastrado_Retorna200()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.GetAsync($"/api/Usuario/email/{usuario.Email}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var encontrado = await response.Content
            .ReadFromJsonAsync<UsuarioResponse>(ApiJson.Options);

        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado.Id);
    }

    [Fact]
    public async Task GetByNome_NomeCadastrado_Retorna200ComOsUsuariosCorrespondentes()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.GetAsync($"/api/Usuario/nome/{Uri.EscapeDataString(usuario.Nome)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var usuarios = await response.Content
            .ReadFromJsonAsync<List<UsuarioResponse>>(ApiJson.Options);

        Assert.NotNull(usuarios);
        Assert.Contains(usuarios, u => u.Id == usuario.Id);
    }

    [Fact]
    public async Task GetByNome_NomeSemCorrespondencia_Retorna200ComListaVazia()
    {
        // Arrange
        var nomeInexistente = $"NaoExiste{Guid.NewGuid():N}";

        // Act
        var response = await _client.GetAsync($"/api/Usuario/nome/{nomeInexistente}");

        // Assert
        // A busca parcial nao encontrar nada e um resultado valido, nao um erro.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var usuarios = await response.Content
            .ReadFromJsonAsync<List<UsuarioResponse>>(ApiJson.Options);

        Assert.NotNull(usuarios);
        Assert.Empty(usuarios);
    }

    [Fact]
    public async Task Put_UsuarioExistente_Retorna204EPersisteAlteracao()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new UpdateUsuarioRequest("Nome Atualizado", "11911112222");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Usuario/{usuario.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var atualizado = await _client.GetFromJsonAsync<UsuarioResponse>(
            $"/api/Usuario/{usuario.Id}", ApiJson.Options);

        Assert.NotNull(atualizado);
        Assert.Equal("Nome Atualizado", atualizado.Nome);
        Assert.Equal("11911112222", atualizado.Telefone);
    }

    [Fact]
    public async Task Put_UsuarioInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdateUsuarioRequest("Nome Qualquer", "11911112222");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Usuario/{idInexistente}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_NomeVazio_Retorna400()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new UpdateUsuarioRequest("   ", "11911112222");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Usuario/{usuario.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UsuarioExistente_Retorna204ERemoveDoBanco()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/Usuario/{usuario.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var consultaPosterior = await _client.GetAsync($"/api/Usuario/{usuario.Id}");
        Assert.Equal(HttpStatusCode.NotFound, consultaPosterior.StatusCode);
    }

    [Fact]
    public async Task Delete_UsuarioInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Usuario/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
