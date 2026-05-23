using FutureVet.Domain.Entities;

namespace FutureVet.Application.DTOs.Vacina;

public record VacinaResponse(
    Guid Id,
    string NomeVacina,
    DateTime DataAplicacao,
    DateTime ProximaDose,
    string LocalAplicacao,
    Guid PetId
)
{
    public static VacinaResponse FromDomain(
        Domain.Entities.Vacina vacina)
    {
        return new VacinaResponse(
            vacina.Id,
            vacina.NomeVacina,
            vacina.DataAplicacao,
            vacina.ProximaDose,
            vacina.LocalAplicacao,
            vacina.PetId
        );
    }
}