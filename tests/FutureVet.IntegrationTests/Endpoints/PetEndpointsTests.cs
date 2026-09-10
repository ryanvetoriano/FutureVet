using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Domain.Enums;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

[Collection(ApiCollection.Name)]
public class PetEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public PetEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetAll_ApiDisponivel_Retorna200ComListaDePets()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var response = await _client.GetAsync("/api/Pet");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pets = await response.Content
            .ReadFromJsonAsync<List<PetResponse>>(ApiJson.Options);

        Assert.NotNull(pets);
        Assert.Contains(pets, p => p.Id == pet.Id);
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComPetCriado()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 32.5m, usuario.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pet", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criado = await response.Content
            .ReadFromJsonAsync<PetResponse>(ApiJson.Options);

        Assert.NotNull(criado);
        Assert.Equal("Thor", criado.NomePet);
        Assert.Equal(EspeciePet.Cao, criado.Especie);
        Assert.Equal(32.5m, criado.Peso);
    }

    [Fact]
    public async Task Post_PesoZerado_Retorna400()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", 3, PortePet.Grande, 0m, usuario.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pet", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Peso inválido.", corpo);
    }

    [Fact]
    public async Task Post_IdadeNegativa_Retorna400()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var request = new CreatePetRequest(
            "Thor", EspeciePet.Cao, "Labrador", -1, PortePet.Grande, 30m, usuario.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pet", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_PetExistente_Retorna200ComOPet()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var response = await _client.GetAsync($"/api/Pet/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var encontrado = await response.Content
            .ReadFromJsonAsync<PetResponse>(ApiJson.Options);

        Assert.NotNull(encontrado);
        Assert.Equal(pet.Id, encontrado.Id);
    }

    [Fact]
    public async Task GetById_PetInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Pet/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByEspecie_EspecieForaDoEnum_Retorna400()
    {
        // Arrange
        const int especieInexistente = 99;

        // Act
        var response = await _client.GetAsync($"/api/Pet/especie/{especieInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByEspecie_EspecieValida_Retorna200()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var response = await _client.GetAsync($"/api/Pet/especie/{(int)EspeciePet.Cao}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pets = await response.Content
            .ReadFromJsonAsync<List<PetResponse>>(ApiJson.Options);

        Assert.NotNull(pets);
        Assert.Contains(pets, p => p.Id == pet.Id);
    }

    [Fact]
    public async Task GetByUsuario_UsuarioComPets_Retorna200ApenasComOsPetsDele()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();
        var pet = await _dados.CriarPetAsync(usuario.Id);
        var petDeOutroDono = await _dados.CriarPetAsync();

        // Act
        var response = await _client.GetAsync($"/api/Pet/usuario/{usuario.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pets = await response.Content
            .ReadFromJsonAsync<List<PetResponse>>(ApiJson.Options);

        Assert.NotNull(pets);
        Assert.Contains(pets, p => p.Id == pet.Id);
        Assert.DoesNotContain(pets, p => p.Id == petDeOutroDono.Id);
    }

    [Fact]
    public async Task Put_PetExistente_Retorna204EPersisteAlteracao()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new UpdatePetRequest("Thor Atualizado", "Golden", 5, PortePet.Medio, 28.0m);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Pet/{pet.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var atualizado = await _client.GetFromJsonAsync<PetResponse>(
            $"/api/Pet/{pet.Id}", ApiJson.Options);

        Assert.NotNull(atualizado);
        Assert.Equal("Thor Atualizado", atualizado.NomePet);
        Assert.Equal(5, atualizado.Idade);
        Assert.Equal(PortePet.Medio, atualizado.Tamanho);
    }

    [Fact]
    public async Task Put_PetInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdatePetRequest("Thor", "Labrador", 3, PortePet.Grande, 30m);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Pet/{idInexistente}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_PesoNegativo_Retorna400()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new UpdatePetRequest("Thor", "Labrador", 3, PortePet.Grande, -1m);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Pet/{pet.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_PetExistente_Retorna204ERemoveDoBanco()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/Pet/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var consultaPosterior = await _client.GetAsync($"/api/Pet/{pet.Id}");
        Assert.Equal(HttpStatusCode.NotFound, consultaPosterior.StatusCode);
    }

    [Fact]
    public async Task Delete_PetInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Pet/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
