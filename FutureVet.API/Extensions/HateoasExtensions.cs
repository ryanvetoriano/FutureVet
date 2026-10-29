using FutureVet.API.Hateoas;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;

namespace FutureVet.API.Extensions;

/// <summary>
/// Registro dos geradores de links HATEOAS. Os controllers dependem da abstração
/// <see cref="ResourceLinks{T}"/>, não das implementações.
/// </summary>
public static class HateoasExtensions
{
    public static IServiceCollection AddHateoas(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<LinkBuilder>();

        services.AddScoped<ResourceLinks<UsuarioResponse>, UsuarioLinks>();
        services.AddScoped<ResourceLinks<PetResponse>, PetLinks>();
        services.AddScoped<ResourceLinks<VacinaResponse>, VacinaLinks>();
        services.AddScoped<ResourceLinks<ConsultaResponse>, ConsultaLinks>();
        services.AddScoped<ResourceLinks<RegistroClinicoResponse>, RegistroClinicoLinks>();

        return services;
    }
}
