using FutureVet.Application.DTOs.Common;

namespace FutureVet.Application.DTOs.Vacina;

public record VacinaResponse(
    Guid Id,
    string NomeVacina,
    DateTime DataAplicacao,
    DateTime ProximaDose,
    string LocalAplicacao,
    Guid PetId
) : ResourceResponse
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
