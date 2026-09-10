using Microsoft.AspNetCore.Hosting;

namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Factory que declara um serviço HTTP externo em <c>HealthChecks:ExternalServices</c>,
/// exercitando o registro por configuração do <c>ExternalServiceHealthCheck</c>.
/// </summary>
/// <remarks>
/// O endereço aponta para <c>127.0.0.1</c> na porta 1, onde nada escuta: a conexão é
/// recusada de imediato pelo sistema operacional. Nenhum serviço externo real — de
/// produção ou de terceiros — é contatado durante os testes.
/// </remarks>
public class ExternalServiceIndisponivelFactory : CustomWebApplicationFactory
{
    protected virtual bool ServicoOpcional => false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting entra na configuração do host antes de o Program.cs lê-la,
        // que é o momento em que os health checks são registrados.
        builder.UseSetting("HealthChecks:ExternalServices:0:Name", "servico-externo");
        builder.UseSetting("HealthChecks:ExternalServices:0:Url", "http://127.0.0.1:1/health");
        builder.UseSetting("HealthChecks:ExternalServices:0:TimeoutSeconds", "2");
        builder.UseSetting("HealthChecks:ExternalServices:0:Optional", ServicoOpcional.ToString());

        base.ConfigureWebHost(builder);
    }
}

/// <summary>
/// Mesma configuração, porém com o serviço marcado como opcional: a indisponibilidade
/// deve degradar a API em vez de derrubá-la.
/// </summary>
public sealed class ExternalServiceOpcionalFactory : ExternalServiceIndisponivelFactory
{
    protected override bool ServicoOpcional => true;
}
