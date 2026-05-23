using FutureVet.Domain.Entities;

namespace FutureVet.Application.DTOs.Vacina;

public record CreateVacinaRequest(
    string NomeVacina,
    DateTime DataAplicacao,
    DateTime ProximaDose,
    string LocalAplicacao,
    Guid PetId
)
{
    public Domain.Entities.Vacina ToDomain()
    {
        return new Domain.Entities.Vacina(
            NomeVacina,
            DataAplicacao,
            ProximaDose,
            LocalAplicacao,
            PetId
        );
    }
}