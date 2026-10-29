using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FutureVet.Application;

/// <summary>
/// Registro dos serviços da camada de Application.
/// </summary>
/// <remarks>
/// Cada camada declara as próprias dependências: o <c>Program.cs</c> apenas compõe as
/// camadas e não precisa conhecer as implementações concretas de cada uma.
/// </remarks>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IVacinaService, VacinaService>();
        services.AddScoped<IConsultaService, ConsultaService>();
        services.AddScoped<IRegistroClinicoService, RegistroClinicoService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
