using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

[Collection(ApiCollection.Name)]
public class VacinaEndpointsTests
{
    private static readonly DateTime Aplicacao = new(2026, 1, 10);

    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public VacinaEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetAll_ApiDisponivel_Retorna200ComListaDeVacinas()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();

        // Act
        var response = await _client.GetAsync("/api/Vacina");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vacinas = await response.Content
            .ReadFromJsonAsync<List<VacinaResponse>>(ApiJson.Options);

        Assert.NotNull(vacinas);
        Assert.Contains(vacinas, v => v.Id == vacina.Id);
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComVacinaCriada()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateVacinaRequest(
            "Antirrábica", Aplicacao, Aplicacao.AddYears(1), "Clínica Central", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vacina", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criada = await response.Content
            .ReadFromJsonAsync<VacinaResponse>(ApiJson.Options);

        Assert.NotNull(criada);
        Assert.Equal("Antirrábica", criada.NomeVacina);
        Assert.Equal(pet.Id, criada.PetId);
    }

    [Fact]
    public async Task Post_ProximaDoseAnteriorAAplicacao_Retorna400()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateVacinaRequest(
            "V10", Aplicacao, Aplicacao.AddDays(-5), "Clínica Central", pet.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/api/Vacina", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Próxima dose inválida.", corpo);
    }

    [Fact]
    public async Task GetById_VacinaExistente_Retorna200()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();

        // Act
        var response = await _client.GetAsync($"/api/Vacina/{vacina.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var encontrada = await response.Content
            .ReadFromJsonAsync<VacinaResponse>(ApiJson.Options);

        Assert.NotNull(encontrada);
        Assert.Equal(vacina.Id, encontrada.Id);
    }

    [Fact]
    public async Task GetById_VacinaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Vacina/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByPet_PetComVacinas_Retorna200ApenasComAsVacinasDoPet()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var vacina = await _dados.CriarVacinaAsync(pet.Id);
        var vacinaDeOutroPet = await _dados.CriarVacinaAsync();

        // Act
        var response = await _client.GetAsync($"/api/Vacina/pet/{pet.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vacinas = await response.Content
            .ReadFromJsonAsync<List<VacinaResponse>>(ApiJson.Options);

        Assert.NotNull(vacinas);
        Assert.Contains(vacinas, v => v.Id == vacina.Id);
        Assert.DoesNotContain(vacinas, v => v.Id == vacinaDeOutroPet.Id);
    }

    [Fact]
    public async Task GetByProximaDose_DataMalFormada_Retorna400()
    {
        // Arrange
        const string dataInvalida = "nao-e-data";

        // Act
        var response = await _client.GetAsync($"/api/Vacina/proxima-dose/{dataInvalida}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByProximaDose_DataValida_Retorna200ComVacinasVencendoAteALimite()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();
        var dataLimite = Aplicacao.AddYears(2).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/Vacina/proxima-dose/{dataLimite}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var vacinas = await response.Content
            .ReadFromJsonAsync<List<VacinaResponse>>(ApiJson.Options);

        Assert.NotNull(vacinas);
        Assert.Contains(vacinas, v => v.Id == vacina.Id);
    }

    [Fact]
    public async Task Put_VacinaExistente_Retorna204EPersisteAlteracao()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();
        var novaDose = Aplicacao.AddMonths(18);
        var request = new UpdateVacinaRequest(novaDose, "Clínica Norte");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Vacina/{vacina.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var atualizada = await _client.GetFromJsonAsync<VacinaResponse>(
            $"/api/Vacina/{vacina.Id}", ApiJson.Options);

        Assert.NotNull(atualizada);
        Assert.Equal(novaDose, atualizada.ProximaDose);
        Assert.Equal("Clínica Norte", atualizada.LocalAplicacao);
    }

    [Fact]
    public async Task Put_VacinaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var request = new UpdateVacinaRequest(Aplicacao.AddYears(1), "Clínica Norte");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Vacina/{idInexistente}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ProximaDoseAnteriorAAplicacao_Retorna400()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();
        var request = new UpdateVacinaRequest(Aplicacao.AddDays(-1), "Clínica Norte");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/Vacina/{vacina.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_VacinaExistente_Retorna204ERemoveDoBanco()
    {
        // Arrange
        var vacina = await _dados.CriarVacinaAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/Vacina/{vacina.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var consultaPosterior = await _client.GetAsync($"/api/Vacina/{vacina.Id}");
        Assert.Equal(HttpStatusCode.NotFound, consultaPosterior.StatusCode);
    }

    [Fact]
    public async Task Delete_VacinaInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Vacina/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
