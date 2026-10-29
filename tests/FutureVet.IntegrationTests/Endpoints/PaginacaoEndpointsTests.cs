using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Domain.Enums;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Endpoints;

/// <summary>
/// Paginação, ordenação e filtros dos <c>GET</c> de listagem. Como o Oracle é compartilhado,
/// todo cenário filtra pelo dono ou pelo pet que ele mesmo criou, para que registros de
/// outras execuções não interfiram nas contagens.
/// </summary>
[Collection(ApiCollection.Name)]
public class PaginacaoEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public PaginacaoEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetAllPets_TresPetsComTamanhoPagina2_RetornaPrimeiraPaginaComMetadados()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        await _dados.CriarPetAsync(dono.Id, nome: "Charlie");
        await _dados.CriarPetAsync(dono.Id, nome: "Alfa");
        await _dados.CriarPetAsync(dono.Id, nome: "Bravo");

        // Act
        var response = await _client.GetAsync(
            $"/api/Pet?usuarioId={dono.Id}&ordenarPor=nome&tamanhoPagina=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<PetResponse>>(ApiJson.Options);
        Assert.NotNull(pagina);
        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(2, pagina.TamanhoPagina);
        Assert.Equal(3, pagina.TotalItens);
        Assert.Equal(2, pagina.TotalPaginas);
        Assert.Equal(["Alfa", "Bravo"], pagina.Itens.Select(p => p.NomePet));
    }

    [Fact]
    public async Task GetAllPets_SegundaPagina_RetornaOsItensRestantes()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        await _dados.CriarPetAsync(dono.Id, nome: "Charlie");
        await _dados.CriarPetAsync(dono.Id, nome: "Alfa");
        await _dados.CriarPetAsync(dono.Id, nome: "Bravo");

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&ordenarPor=nome&tamanhoPagina=2&pagina=2", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal("Charlie", Assert.Single(pagina.Itens).NomePet);
    }

    [Fact]
    public async Task GetAllPets_OrdenarPorIdadeDesc_RetornaDoMaisVelhoParaOMaisNovo()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        await _dados.CriarPetAsync(dono.Id, idade: 2);
        await _dados.CriarPetAsync(dono.Id, idade: 9);
        await _dados.CriarPetAsync(dono.Id, idade: 5);

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&ordenarPor=idade&direcao=desc", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal([9, 5, 2], pagina.Itens.Select(p => p.Idade));
    }

    [Fact]
    public async Task GetAllPets_FiltrosCombinados_RetornaApenasPetsQueAtendemATodos()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        var gatoAdulto = await _dados.CriarPetAsync(dono.Id, idade: 6, especie: EspeciePet.Gato);
        await _dados.CriarPetAsync(dono.Id, idade: 1, especie: EspeciePet.Gato);
        await _dados.CriarPetAsync(dono.Id, idade: 6, especie: EspeciePet.Cao);

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&especie={(int)EspeciePet.Gato}&idadeMinima=3", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(gatoAdulto.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAllPets_FiltroPorNomeEmMinusculas_IgnoraDiferencaDeCaixa()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        var pet = await _dados.CriarPetAsync(dono.Id, nome: "Thor Odinson");

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&nome=odin", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(pet.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAllUsuarios_FiltroPorNome_RetornaUsuarioCorrespondente()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync(prefixoNome: "Filtrado");

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<UsuarioResponse>>(
            $"/api/Usuario?nome={Uri.EscapeDataString(usuario.Nome.ToUpperInvariant())}", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(usuario.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAllConsultas_FiltroPorPeriodo_RetornaSomenteConsultasDoIntervaloEmOrdemCronologica()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        await _dados.CriarConsultaAsync(pet.Id, new DateTime(2026, 1, 20));
        var marcoFim = await _dados.CriarConsultaAsync(pet.Id, new DateTime(2026, 3, 31));
        var marcoInicio = await _dados.CriarConsultaAsync(pet.Id, new DateTime(2026, 3, 1));
        await _dados.CriarConsultaAsync(pet.Id, new DateTime(2026, 4, 1));

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<ConsultaResponse>>(
            $"/api/Consulta?petId={pet.Id}&dataInicio=2026-03-01&dataFim=2026-03-31", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal([marcoInicio.Id, marcoFim.Id], pagina.Itens.Select(c => c.Id));
    }

    [Fact]
    public async Task GetAllVacinas_FiltroPorPet_OrdenaPelaProximaDosePorPadrao()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var vacina = await _dados.CriarVacinaAsync(pet.Id);

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<VacinaResponse>>(
            $"/api/Vacina?petId={pet.Id}&proximaDoseAte=2030-12-31", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Equal(vacina.Id, Assert.Single(pagina.Itens).Id);
    }

    [Fact]
    public async Task GetAll_SemResultados_RetornaPaginaVaziaComZeroPaginas()
    {
        // Arrange
        var usuarioSemPets = await _dados.CriarUsuarioAsync();

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={usuarioSemPets.Id}", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);
        Assert.Empty(pagina.Itens);
        Assert.Equal(0, pagina.TotalItens);
        Assert.Equal(0, pagina.TotalPaginas);
    }

    [Theory]
    [InlineData("/api/Pet?tamanhoPagina=51")]
    [InlineData("/api/Pet?tamanhoPagina=0")]
    [InlineData("/api/Pet?pagina=0")]
    [InlineData("/api/Pet?ordenarPor=senha")]
    [InlineData("/api/Pet?direcao=aleatoria")]
    [InlineData("/api/Pet?especie=99")]
    [InlineData("/api/Pet?idadeMinima=10&idadeMaxima=2")]
    [InlineData("/api/Usuario?ordenarPor=cpf")]
    [InlineData("/api/Vacina?aplicacaoInicio=2026-12-01&aplicacaoFim=2026-01-01")]
    [InlineData("/api/Consulta?dataInicio=2026-12-01&dataFim=2026-01-01")]
    [InlineData("/api/RegistroClinico?tipo=42")]
    public async Task GetAll_ParametroDeListagemInvalido_Retorna400ComValidationProblemDetails(string url)
    {
        // Arrange & Act
        var response = await _client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // ValidationProblemDetails: os erros vêm agrupados por parâmetro em "errors".
        var corpo = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(400, corpo.GetProperty("status").GetInt32());
        Assert.NotEmpty(corpo.GetProperty("errors").EnumerateObject());
    }

    [Fact]
    public async Task GetAll_CampoDeOrdenacaoInvalido_InformaOsCamposAceitos()
    {
        // Arrange & Act
        var corpo = await (await _client.GetAsync("/api/Pet?ordenarPor=senha")).Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("nome, especie, idade, peso, dataCriacao", corpo);
    }
}
