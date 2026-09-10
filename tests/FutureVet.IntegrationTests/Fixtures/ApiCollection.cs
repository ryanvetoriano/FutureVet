namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Fixture compartilhada por todas as classes de teste de integração da coleção.
/// Subir o host da API e abrir a conexão com o Oracle é caro; a
/// <see cref="ICollectionFixture{TFixture}"/> garante que isso aconteça uma única vez por
/// execução, e não a cada classe.
/// </summary>
/// <remarks>
/// A fixture também é responsável pelo ciclo de vida dos dados: valida a conectividade antes
/// de a suíte começar e remove do Oracle todos os registros de teste ao final.
/// </remarks>
public sealed class ApiFixture : IAsyncLifetime
{
    // Atribuídos em InitializeAsync, que o xUnit executa antes de qualquer teste da coleção.
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;
    private ApiTestData _dados = null!;

    public CustomWebApplicationFactory Factory => _factory;

    /// <summary>HttpClient compartilhado, apontando para a API em memória.</summary>
    public HttpClient Client => _client;

    /// <summary>Fábrica de cenários que também rastreia o que precisa ser limpo.</summary>
    public ApiTestData Dados => _dados;

    public async Task InitializeAsync()
    {
        _factory = new CustomWebApplicationFactory();

        // Falha aqui produz uma mensagem única e clara, em vez de dezenas de erros de conexão.
        await _factory.VerificarConectividadeAsync();

        _client = _factory.CriarClient();
        _dados = new ApiTestData(_client);

        // Varre sobras de uma execução anterior que tenha sido interrompida.
        await _dados.LimparAsync(_factory);
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _dados.LimparAsync(_factory);
        }
        finally
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}

/// <summary>
/// Nome da coleção que compartilha a <see cref="ApiFixture"/>. Classes decoradas com
/// <c>[Collection(ApiCollection.Name)]</c> rodam em sequência e reaproveitam o mesmo host.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "API de integração";
}
