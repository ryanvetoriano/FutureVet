using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

[Collection(ApiCollection.Name)]
public class ConsultaEndpointsTests
{
    private static readonly DateTime DataConsulta = new(2026, 3, 15);

    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public ConsultaEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetAll_ApiDisponivel_Retorna200ComListaDeConsultas()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.GetAsync("/api/Consulta");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var consultas = await response.Content
            .ReadFromJsonAsync<List<ConsultaResponse>>(ApiJson.Options);

        Assert.NotNull(consultas);
        Assert.Contains(consultas, c => c.Id == consulta.Id);
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComConsultaCriada()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateConsultaRequest(
            "Emergência", DataConsulta, "08:15", "Clínica Central", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Consulta", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criada = await response.Content
            .ReadFromJsonAsync<ConsultaResponse>(ApiJson.Options);

        Assert.NotNull(criada);
        Assert.Equal("Emergência", criada.TipoConsulta);
        Assert.Equal("08:15", criada.Hora);
        Assert.Equal(pet.Id, criada.PetId);
    }

    [Fact]
    public async Task Post_TipoConsultaVazio_Retorna400()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateConsultaRequest(
            "   ", DataConsulta, "08:15", "Clínica Central", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Consulta", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Consulta inválida.", corpo);
    }

    [Fact]
    public async Task Post_HoraVazia_Retorna400()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateConsultaRequest(
            "Rotina", DataConsulta, "", "Clínica Central", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Consulta", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ConsultaExistente_Retorna200()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.GetAsync($"/api/Consulta/{consulta.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var encontrada = await response.Content
            .ReadFromJsonAsync<ConsultaResponse>(ApiJson.Options);

        Assert.NotNull(encontrada);
        Assert.Equal(consulta.Id, encontrada.Id);
    }

    [Fact]
    public async Task GetById_ConsultaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Consulta/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByPet_PetComConsultas_Retorna200ApenasComAsConsultasDoPet()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var consulta = await _dados.CriarConsultaAsync(pet.Id);
        var consultaDeOutroPet = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.GetAsync($"/api/Consulta/pet/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var consultas = await response.Content
            .ReadFromJsonAsync<List<ConsultaResponse>>(ApiJson.Options);

        Assert.NotNull(consultas);
        Assert.Contains(consultas, c => c.Id == consulta.Id);
        Assert.DoesNotContain(consultas, c => c.Id == consultaDeOutroPet.Id);
    }

    [Fact]
    public async Task GetByData_DataMalFormada_Retorna400()
    {
        // Arrange
        const string dataInvalida = "nao-e-data";

        // Act
        var response = await _client.GetAsync($"/api/Consulta/data/{dataInvalida}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByData_DataValida_Retorna200ComAsConsultasDaData()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/Consulta/data/{DataConsulta:yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var consultas = await response.Content
            .ReadFromJsonAsync<List<ConsultaResponse>>(ApiJson.Options);

        Assert.NotNull(consultas);
        Assert.Contains(consultas, c => c.Id == consulta.Id);
    }

    [Fact]
    public async Task GetByTipo_TipoComConsultas_Retorna200()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.GetAsync("/api/Consulta/tipo/Rotina");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var consultas = await response.Content
            .ReadFromJsonAsync<List<ConsultaResponse>>(ApiJson.Options);

        Assert.NotNull(consultas);
        Assert.Contains(consultas, c => c.Id == consulta.Id);
    }

    [Fact]
    public async Task Put_ConsultaExistente_Retorna204EPersisteAlteracao()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();
        var novaData = new DateTime(2026, 4, 20);
        var request = new UpdateConsultaRequest(novaData, "09:00", "Clínica Norte");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Consulta/{consulta.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var atualizada = await _client.GetFromJsonAsync<ConsultaResponse>(
            $"/api/Consulta/{consulta.Id}", ApiJson.Options);

        Assert.NotNull(atualizada);
        Assert.Equal(novaData, atualizada.Data);
        Assert.Equal("09:00", atualizada.Hora);
        Assert.Equal("Clínica Norte", atualizada.Local);
    }

    [Fact]
    public async Task Put_ConsultaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdateConsultaRequest(DataConsulta, "09:00", "Clínica Norte");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Consulta/{idInexistente}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_LocalVazio_Retorna400()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();
        var request = new UpdateConsultaRequest(DataConsulta, "09:00", "   ");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Consulta/{consulta.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ConsultaExistente_Retorna204ERemoveDoBanco()
    {
        // Arrange
        var consulta = await _dados.CriarConsultaAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/Consulta/{consulta.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var consultaPosterior = await _client.GetAsync($"/api/Consulta/{consulta.Id}");
        Assert.Equal(HttpStatusCode.NotFound, consultaPosterior.StatusCode);
    }

    [Fact]
    public async Task Delete_ConsultaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Consulta/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
