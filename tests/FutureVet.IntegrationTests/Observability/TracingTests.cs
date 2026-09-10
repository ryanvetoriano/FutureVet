using System.Collections.Concurrent;
using System.Diagnostics;
using FutureVet.Application.Observability;
using FutureVet.IntegrationTests.Fixtures;

namespace FutureVet.IntegrationTests.Observability;

/// <summary>
/// Verifica o tracing distribuído observando as <see cref="Activity"/> realmente
/// produzidas durante uma requisição HTTP. Como a API roda no mesmo processo do teste,
/// um <see cref="ActivityListener"/> consegue capturá-las sem nenhum exporter externo.
/// </summary>
[Collection(ApiCollection.Name)]
public class TracingTests
{
    private readonly HttpClient _client;
    private readonly ApiTestData _dados;

    public TracingTests(ApiFixture fixture)
    {
        _client = fixture.Client;
        _dados = fixture.Dados;
    }

    [Fact]
    public async Task Tracing_RequisicaoQueChegaAoApplicationService_GeraSpanDaCamadaDeApplication()
    {
        // Arrange
        var capturadas = new ConcurrentBag<Activity>();
        using var listener = CriarListener(ApplicationDiagnostics.ActivitySourceName, capturadas);

        // Act
        var usuario = await _dados.CriarUsuarioAsync();

        // Assert
        var span = capturadas.SingleOrDefault(a => a.OperationName == "UsuarioService.CreateAsync");

        Assert.NotNull(span);
        Assert.Equal("Usuario", span.GetTagItem("futurevet.entity"));
        Assert.Equal(usuario.Id, span.GetTagItem("futurevet.entity.id"));
    }

    [Fact]
    public async Task Tracing_ConsultaPorIdInexistente_MarcaOSpanComoNaoEncontrado()
    {
        // Arrange
        var capturadas = new ConcurrentBag<Activity>();
        using var listener = CriarListener(ApplicationDiagnostics.ActivitySourceName, capturadas);
        var idInexistente = Guid.NewGuid();

        // Act
        await _client.GetAsync($"/api/Usuario/{idInexistente}");

        // Assert
        var span = capturadas.SingleOrDefault(a => a.OperationName == "UsuarioService.GetByIdAsync");

        Assert.NotNull(span);
        Assert.Equal(idInexistente, span.GetTagItem("futurevet.entity.id"));
        Assert.Equal(false, span.GetTagItem("futurevet.found"));
    }

    [Fact]
    public async Task Tracing_RequisicaoHttp_ProduzSpanDaCamadaDeApplicationDentroDoTraceDaRequisicao()
    {
        // Arrange
        var capturadas = new ConcurrentBag<Activity>();
        using var listener = CriarListener(ApplicationDiagnostics.ActivitySourceName, capturadas);

        // Act
        await _client.GetAsync($"/api/Pet/{Guid.NewGuid()}");

        // Assert
        var span = capturadas.SingleOrDefault(a => a.OperationName == "PetService.GetByIdAsync");

        Assert.NotNull(span);
        // O span da Application é filho do span criado pela instrumentação do ASP.NET Core,
        // ou seja, ambos pertencem ao mesmo trace da requisição.
        Assert.NotNull(span.ParentId);
        Assert.NotEqual(default, span.TraceId);
    }

    private static ActivityListener CriarListener(
        string sourceName,
        ConcurrentBag<Activity> destino)
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == sourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = destino.Add
        };

        ActivitySource.AddActivityListener(listener);

        return listener;
    }
}
