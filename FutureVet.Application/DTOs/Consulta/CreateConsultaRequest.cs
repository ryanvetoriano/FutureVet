using FutureVet.Domain.Entities;

namespace FutureVet.Application.DTOs.Consulta;

public record CreateConsultaRequest(
    string TipoConsulta,
    DateTime Data,
    String Hora,
    string Local,
    Guid PetId
)
{
    public Domain.Entities.Consulta ToDomain()
    {
        return new Domain.Entities.Consulta(
            TipoConsulta,
            Data,
            Hora,
            Local,
            PetId
        );
    }
}