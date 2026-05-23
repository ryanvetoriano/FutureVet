using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.Pet;

public record CreatePetRequest(
    string NomePet,
    EspeciePet Especie,
    string? Raca,
    int Idade,
    PortePet Tamanho,
    decimal Peso,
    Guid UsuarioId
)
{
    public Domain.Entities.Pet ToDomain()
    {
        return new Domain.Entities.Pet(
            NomePet,
            Especie,
            Raca,
            Idade,
            Tamanho,
            Peso,
            UsuarioId
        );
    }
}