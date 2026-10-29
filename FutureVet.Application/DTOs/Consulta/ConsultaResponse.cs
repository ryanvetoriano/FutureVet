using FutureVet.Application.DTOs.Common;

namespace FutureVet.Application.DTOs.Consulta;

public record ConsultaResponse(
    Guid Id,
    string TipoConsulta,
    DateTime Data,
    string Hora,
    string Local,
    Guid PetId
) : ResourceResponse
{
    public static ConsultaResponse FromDomain(
        Domain.Entities.Consulta consulta)
    {
        return new ConsultaResponse(
            consulta.Id,
            consulta.TipoConsulta,
            consulta.Data,
            consulta.Hora,
            consulta.Local,
            consulta.PetId
        );
    }
}
