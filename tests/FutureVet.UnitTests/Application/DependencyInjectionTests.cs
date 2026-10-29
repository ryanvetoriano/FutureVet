using FutureVet.Application;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FutureVet.UnitTests.Application;

public class DependencyInjectionTests
{
    [Theory]
    [InlineData(typeof(IUsuarioService), typeof(UsuarioService))]
    [InlineData(typeof(IPetService), typeof(PetService))]
    [InlineData(typeof(IVacinaService), typeof(VacinaService))]
    [InlineData(typeof(IConsultaService), typeof(ConsultaService))]
    [InlineData(typeof(IRegistroClinicoService), typeof(RegistroClinicoService))]
    [InlineData(typeof(IAuthService), typeof(AuthService))]
    public void AddApplication_ServicoDaCamada_RegistraImplementacaoComEscopoPorRequisicao(
        Type servico,
        Type implementacao)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplication();

        // Assert
        var registro = Assert.Single(services, d => d.ServiceType == servico);
        Assert.Equal(implementacao, registro.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, registro.Lifetime);
    }
}
