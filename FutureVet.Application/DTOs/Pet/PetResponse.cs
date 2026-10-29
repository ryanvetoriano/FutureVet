using FutureVet.Application.DTOs.Common;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.Pet;

public record PetResponse(
    Guid Id,
    string NomePet,
    EspeciePet Especie,
    string? Raca,
    int Idade,
    PortePet Tamanho,
    decimal Peso,
    Guid UsuarioId
) : ResourceResponse
{
    public static PetResponse FromDomain(
        Domain.Entities.Pet pet)
    {
        return new PetResponse(
            pet.Id,
            pet.NomePet,
            pet.Especie,
            pet.Raca,
            pet.Idade,
            pet.Tamanho,
            pet.Peso,
            pet.UsuarioId
        );
    }
}
