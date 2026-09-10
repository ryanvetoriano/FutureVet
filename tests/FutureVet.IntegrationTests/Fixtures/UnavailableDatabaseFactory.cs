namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Variante da factory apontando para um endereço Oracle onde nada escuta: a conexão é
/// recusada de verdade pelo sistema operacional, sem nenhum mock no caminho.
/// </summary>
/// <remarks>
/// <para>Usada com <see cref="IClassFixture{TFixture}"/> em vez da collection fixture: esta
/// configuração de host é diferente da compartilhada e só interessa à classe que verifica o
/// comportamento de <c>/health</c> com o banco fora do ar.</para>
/// <para>O destino é <c>127.0.0.1:1521</c> — endereço local, portanto o teste não depende de
/// rede externa e a falha é imediata, sem esperar timeout.</para>
/// </remarks>
public sealed class UnavailableDatabaseFactory : CustomWebApplicationFactory
{
    protected override string ConnectionString =>
        "User Id=usuario_de_teste;Password=nao_utilizada;"
        + "Data Source=127.0.0.1:1521/BANCO_FORA_DO_AR;Connection Timeout=5";
}
