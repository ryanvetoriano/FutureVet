using System.Net;
using System.Net.Http.Json;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.WebUtilities;

namespace FutureVet.IntegrationTests.Endpoints;

/// <summary>
/// HATEOAS: as respostas de consulta trazem links para as ações possíveis e para os recursos
/// relacionados, e esses links funcionam de verdade quando seguidos.
/// </summary>
[Collection(ApiCollection.Name)]
public class HateoasEndpointsTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public HateoasEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task GetById_Pet_RetornaLinksDeAcoesEDeRecursosRelacionados()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var resposta = await _client.GetFromJsonAsync<PetResponse>($"/api/Pet/{pet.Id}", ApiJson.Options);

        // Assert
        Assert.NotNull(resposta);

        var links = resposta.Links.ToDictionary(l => l.Rel);
        Assert.Equal(
            ["self", "update", "delete", "usuario", "vacinas", "consultas", "registros-clinicos"],
            resposta.Links.Select(l => l.Rel));

        Assert.Equal("GET", links["self"].Method);
        Assert.Equal("PUT", links["update"].Method);
        Assert.Equal("DELETE", links["delete"].Method);

        Assert.Equal($"/api/Pet/{pet.Id}", new Uri(links["self"].Href).AbsolutePath);
        Assert.Equal($"/api/Usuario/{pet.UsuarioId}", new Uri(links["usuario"].Href).AbsolutePath);
        Assert.Equal($"/api/Vacina/pet/{pet.Id}", new Uri(links["vacinas"].Href).AbsolutePath);
        Assert.Equal($"/api/RegistroClinico/pet/{pet.Id}", new Uri(links["registros-clinicos"].Href).AbsolutePath);
    }

    [Fact]
    public async Task GetById_LinksDoRecurso_SaoNavegaveis()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();
        var vacina = await _dados.CriarVacinaAsync(pet.Id);
        var resposta = await _client.GetFromJsonAsync<PetResponse>($"/api/Pet/{pet.Id}", ApiJson.Options);

        // Act
        var linkVacinas = resposta!.Links.Single(l => l.Rel == "vacinas").Href;
        var linkDono = resposta.Links.Single(l => l.Rel == "usuario").Href;

        var vacinas = await _client.GetFromJsonAsync<List<VacinaResponse>>(linkVacinas, ApiJson.Options);
        var dono = await _client.GetFromJsonAsync<UsuarioResponse>(linkDono, ApiJson.Options);

        // Assert
        Assert.NotNull(vacinas);
        Assert.Contains(vacinas, v => v.Id == vacina.Id);
        Assert.Equal(pet.UsuarioId, dono!.Id);
    }

    [Fact]
    public async Task Post_RecursoCriado_RespostaJaTrazOsLinks()
    {
        // Arrange & Act
        var pet = await _dados.CriarPetAsync();

        // Assert
        Assert.Contains(pet.Links, l => l.Rel == "self" && l.Href.EndsWith($"/api/Pet/{pet.Id}"));
    }

    [Fact]
    public async Task GetAll_PrimeiraDeDuasPaginas_TrazNextELastMasNaoPrev()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        for (var i = 0; i < 3; i++)
            await _dados.CriarPetAsync(dono.Id);

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&tamanhoPagina=2", ApiJson.Options);

        // Assert
        Assert.NotNull(pagina);

        var rels = pagina.Links.Select(l => l.Rel).ToList();
        Assert.Contains("self", rels);
        Assert.Contains("first", rels);
        Assert.Contains("next", rels);
        Assert.Contains("last", rels);
        Assert.Contains("create", rels);
        Assert.DoesNotContain("prev", rels);

        Assert.All(pagina.Itens, p => Assert.Contains(p.Links, l => l.Rel == "self"));
    }

    [Fact]
    public async Task GetAll_LinkNext_PreservaFiltrosOrdenacaoETamanhoDaPagina()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        for (var i = 0; i < 3; i++)
            await _dados.CriarPetAsync(dono.Id);

        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&ordenarPor=idade&direcao=desc&tamanhoPagina=2", ApiJson.Options);

        // Act
        var next = new Uri(pagina!.Links.Single(l => l.Rel == "next").Href);
        var query = QueryHelpers.ParseQuery(next.Query);

        // Assert
        Assert.Equal("2", query["pagina"].ToString());
        Assert.Equal("2", query["tamanhoPagina"].ToString());
        Assert.Equal(dono.Id.ToString(), query["usuarioId"].ToString());
        Assert.Equal("idade", query["ordenarPor"].ToString());
        Assert.Equal("desc", query["direcao"].ToString());
    }

    [Fact]
    public async Task GetAll_SeguindoOLinkNext_ChegaNaUltimaPaginaComPrev()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        for (var i = 0; i < 3; i++)
            await _dados.CriarPetAsync(dono.Id);

        var primeira = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={dono.Id}&tamanhoPagina=2", ApiJson.Options);

        // Act
        var segunda = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            primeira!.Links.Single(l => l.Rel == "next").Href, ApiJson.Options);

        // Assert
        Assert.NotNull(segunda);
        Assert.Equal(2, segunda.Pagina);
        Assert.Single(segunda.Itens);
        Assert.Contains(segunda.Links, l => l.Rel == "prev");
        Assert.DoesNotContain(segunda.Links, l => l.Rel == "next");
    }

    [Fact]
    public async Task GetByUsuario_ListaDePets_CadaItemTrazOsProprios()
    {
        // Arrange
        var dono = await _dados.CriarUsuarioAsync();
        var pet = await _dados.CriarPetAsync(dono.Id);

        // Act
        var pets = await _client.GetFromJsonAsync<List<PetResponse>>(
            $"/api/Pet/usuario/{dono.Id}", ApiJson.Options);

        // Assert
        var item = Assert.Single(pets!);
        Assert.Contains(item.Links, l => l.Rel == "self" && l.Href.EndsWith($"/api/Pet/{pet.Id}"));
    }

    [Fact]
    public async Task GetById_Usuario_LinkPetsApontaParaOsPetsDoUsuario()
    {
        // Arrange
        var usuario = await _dados.CriarUsuarioAsync();

        // Act
        var resposta = await _client.GetFromJsonAsync<UsuarioResponse>(
            $"/api/Usuario/{usuario.Id}", ApiJson.Options);

        // Assert
        var linkPets = resposta!.Links.Single(l => l.Rel == "pets");
        Assert.Equal($"/api/Pet/usuario/{usuario.Id}", new Uri(linkPets.Href).AbsolutePath);
    }

    [Fact]
    public async Task GetById_RespostaJson_ExpoeLinksComHrefRelEMethod()
    {
        // Arrange
        var pet = await _dados.CriarPetAsync();

        // Act
        var json = await _client.GetStringAsync($"/api/Pet/{pet.Id}");

        // Assert
        Assert.Contains("\"links\":[", json);
        Assert.Contains("\"href\":", json);
        Assert.Contains("\"rel\":\"self\"", json);
        Assert.Contains("\"method\":\"GET\"", json);
    }

    [Fact]
    public async Task GetAll_PaginaVazia_LinkLastApontaParaAPrimeiraPagina()
    {
        // Arrange
        var semPets = await _dados.CriarUsuarioAsync();

        // Act
        var pagina = await _client.GetFromJsonAsync<PagedResponse<PetResponse>>(
            $"/api/Pet?usuarioId={semPets.Id}", ApiJson.Options);

        // Assert
        var last = new Uri(pagina!.Links.Single(l => l.Rel == "last").Href);
        Assert.Equal("1", QueryHelpers.ParseQuery(last.Query)["pagina"].ToString());
    }

    [Fact]
    public async Task GetById_RecursoInexistente_Retorna404SemLinks()
    {
        // Arrange & Act
        var response = await _client.GetAsync($"/api/Pet/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
