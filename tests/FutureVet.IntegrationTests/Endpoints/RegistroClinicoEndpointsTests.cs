using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Domain.Enums;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

/// <summary>
/// Prontuário (MongoDB). O pet de cada cenário é criado no Oracle pelos endpoints reais e o
/// registro vai para o mongod efêmero da factory — o fluxo exercita as duas bases juntas.
/// </summary>
[Collection(ApiCollection.Name)]
public class RegistroClinicoEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;
    private readonly CustomWebApplicationFactory _factory;

    public RegistroClinicoEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
        _factory = fixture.Factory;
    }

    [Fact]
    public async Task Post_DadosValidos_Retorna201ComDocumentoCompleto()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateRegistroClinicoRequest(
            pet.Id,
            TipoRegistroClinico.Prescricao,
            "Otite externa",
            "Prurido e secreção no ouvido esquerdo.",
            "Dr. Carlos Lima",
            new DateTime(2026, 5, 20, 14, 30, 0),
            ["Coceira", "Secreção"],
            [new MedicamentoRequest("Otomax", "5 gotas", "12/12h", 10)],
            ["Ouvido", "ALERGIA"]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/RegistroClinico", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var criado = await response.Content.ReadFromJsonAsync<RegistroClinicoResponse>(ApiJson.Options);
        Assert.NotNull(criado);
        Assert.Equal(pet.Id, criado.PetId);
        Assert.Equal(TipoRegistroClinico.Prescricao, criado.Tipo);
        Assert.Equal(["Coceira", "Secreção"], criado.Sintomas);
        Assert.Equal(["ouvido", "alergia"], criado.Tags);

        var medicamento = Assert.Single(criado.Medicamentos);
        Assert.Equal("Otomax", medicamento.Nome);
        Assert.Equal(10, medicamento.DuracaoDias);

        Assert.Equal($"/api/RegistroClinico/{criado.Id}", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task GetById_RegistroExistente_Retorna200ComOMesmoConteudoGravado()
    {
        // Arrange
        var registro = await _dados.CriarRegistroClinicoAsync(
            tipo: TipoRegistroClinico.Prescricao,
            dataRegistro: new DateTime(2026, 4, 2, 9, 15, 0));

        // Act
        var response = await _client.GetAsync($"/api/RegistroClinico/{registro.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var lido = await response.Content.ReadFromJsonAsync<RegistroClinicoResponse>(ApiJson.Options);
        Assert.NotNull(lido);
        Assert.Equal(registro.Id, lido.Id);
        Assert.Equal(registro.Titulo, lido.Titulo);
        Assert.Equal(new DateTime(2026, 4, 2, 9, 15, 0), lido.DataRegistro);
        Assert.Equal("Amoxicilina", Assert.Single(lido.Medicamentos).Nome);
        Assert.Equal(["apatia"], lido.Sintomas);
    }

    [Fact]
    public async Task GetById_RegistroInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/RegistroClinico/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_PetInexistente_Retorna404SemGravarNoMongo()
    {
        // Arrange
        var petInexistente = Guid.NewGuid();
        var request = new CreateRegistroClinicoRequest(
            petInexistente, TipoRegistroClinico.Observacao, "Check-up", "Sem alterações.",
            null, new DateTime(2026, 3, 1), null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/RegistroClinico", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var registros = await _client.GetFromJsonAsync<List<RegistroClinicoResponse>>(
            $"/api/RegistroClinico/pet/{petInexistente}", ApiJson.Options);
        Assert.NotNull(registros);
        Assert.Empty(registros);
    }

    [Fact]
    public async Task Post_PrescricaoSemMedicamento_Retorna400ComMensagemDoDominio()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var request = new CreateRegistroClinicoRequest(
            pet.Id, TipoRegistroClinico.Prescricao, "Receita", "Tratamento de otite.",
            null, new DateTime(2026, 3, 1), null, [], null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/RegistroClinico", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Uma prescrição deve informar ao menos um medicamento.", corpo);
    }

    [Fact]
    public async Task Post_SemAutenticacao_Retorna401()
    {
        // Arrange
        using var clientAnonimo = _factory.CriarClient();
        var request = new CreateRegistroClinicoRequest(
            Guid.NewGuid(), TipoRegistroClinico.Observacao, "Check-up", "Sem alterações.",
            null, new DateTime(2026, 3, 1), null, null, null);

        // Act
        var response = await clientAnonimo.PostAsJsonAsync("/api/RegistroClinico", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetByPet_PetComVariosRegistros_RetornaDoMaisRecenteParaOMaisAntigo()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var antigo = await _dados.CriarRegistroClinicoAsync(pet.Id, dataRegistro: new DateTime(2026, 1, 10));
        var recente = await _dados.CriarRegistroClinicoAsync(pet.Id, dataRegistro: new DateTime(2026, 6, 10));
        var outroPet = await _dados.CriarRegistroClinicoAsync();

        // Act
        var registros = await _client.GetFromJsonAsync<List<RegistroClinicoResponse>>(
            $"/api/RegistroClinico/pet/{pet.Id}", ApiJson.Options);

        // Assert
        Assert.NotNull(registros);
        Assert.Equal([recente.Id, antigo.Id], registros.Select(r => r.Id));
        Assert.DoesNotContain(registros, r => r.Id == outroPet.Id);
    }

    [Fact]
    public async Task GetAll_FiltroPorTag_RetornaApenasRegistrosComATag()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var comTag = await _dados.CriarRegistroClinicoAsync(pet.Id, tags: ["dermatologia"]);
        await _dados.CriarRegistroClinicoAsync(pet.Id, tags: ["rotina"]);

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<RegistroClinicoResponse>>(
            $"/api/RegistroClinico?petId={pet.Id}&tag=Dermatologia", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(1, pagina.TotalItens);
        Assert.Equal(comTag.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAll_FiltroPorTexto_BuscaNoTituloSemDiferenciarMaiusculas()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var alvo = await _dados.CriarRegistroClinicoAsync(pet.Id, titulo: "Hemograma completo");
        await _dados.CriarRegistroClinicoAsync(pet.Id, titulo: "Vermifugação");

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<RegistroClinicoResponse>>(
            $"/api/RegistroClinico?petId={pet.Id}&texto=HEMOGRAMA", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(alvo.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAll_TextoComCaracteresDeRegex_BuscaLiteralSemErro()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        await _dados.CriarRegistroClinicoAsync(pet.Id, titulo: "Exame (urina)");

        // Act
        var response = await _client.GetAsync(
            $"/api/RegistroClinico?petId={pet.Id}&texto={Uri.EscapeDataString("(urina")}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<RegistroClinicoResponse>>(ApiJson.Options);
        Assert.NotNull(pagina);
        Assert.Single(pagina.Itens);
    }

    [Fact]
    public async Task GetAll_TresRegistrosComTamanhoPagina2_PaginaNoMongo()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        for (var dia = 1; dia <= 3; dia++)
            await _dados.CriarRegistroClinicoAsync(pet.Id, dataRegistro: new DateTime(2026, 2, dia));

        // Act
        var primeira = await _client.GetFromJsonAsync<PagedResponse<RegistroClinicoResponse>>(
            $"/api/RegistroClinico?petId={pet.Id}&tamanhoPagina=2", ApiJson.Options);
        var segunda = await _client.GetFromJsonAsync<PagedResponse<RegistroClinicoResponse>>(
            $"/api/RegistroClinico?petId={pet.Id}&tamanhoPagina=2&pagina=2", ApiJson.Options);

        // Assert
        Assert.NotNull(primeira);
        Assert.NotNull(segunda);
        Assert.Equal(3, primeira.TotalItens);
        Assert.Equal(2, primeira.TotalPaginas);
        Assert.Equal(2, primeira.Itens.Count);
        Assert.Single(segunda.Itens);

        // Ordem padrão: mais recente primeiro.
        Assert.Equal(new DateTime(2026, 2, 3), primeira.Itens[0].DataRegistro);
        Assert.Equal(new DateTime(2026, 2, 1), segunda.Itens[0].DataRegistro);
    }

    [Fact]
    public async Task Put_RegistroExistente_Retorna204EPersisteAlteracao()
    {
        // Arrange
        var registro = await _dados.CriarRegistroClinicoAsync();
        var request = new UpdateRegistroClinicoRequest(
            TipoRegistroClinico.Diagnostico,
            "Dermatite alérgica",
            "Confirmada após teste de contato.",
            "Dra. Ana Souza",
            new DateTime(2026, 3, 20),
            ["Coceira"],
            null,
            ["dermatologia"]);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/RegistroClinico/{registro.Id}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var atualizado = await _client.GetFromJsonAsync<RegistroClinicoResponse>(
            $"/api/RegistroClinico/{registro.Id}", ApiJson.Options);

        Assert.NotNull(atualizado);
        Assert.Equal(TipoRegistroClinico.Diagnostico, atualizado.Tipo);
        Assert.Equal("Dermatite alérgica", atualizado.Titulo);
        Assert.Equal(["dermatologia"], atualizado.Tags);
        Assert.Equal(registro.PetId, atualizado.PetId);
    }

    [Fact]
    public async Task Put_RegistroInexistente_Retorna404()
    {
        // Arrange
        var request = new UpdateRegistroClinicoRequest(
            TipoRegistroClinico.Observacao, "Título", "Descrição", null,
            new DateTime(2026, 3, 20), null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/RegistroClinico/{Guid.NewGuid()}", request, ApiJson.Options);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RegistroExistente_Retorna204ERemoveDoMongo()
    {
        // Arrange
        var registro = await _dados.CriarRegistroClinicoAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/RegistroClinico/{registro.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var consultaPosterior = await _client.GetAsync($"/api/RegistroClinico/{registro.Id}");
        Assert.Equal(HttpStatusCode.NotFound, consultaPosterior.StatusCode);
    }

    [Fact]
    public async Task Delete_RegistroInexistente_Retorna404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/RegistroClinico/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
