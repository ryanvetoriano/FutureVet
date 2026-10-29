using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.RegistroClinico;

public record CreateRegistroClinicoRequest(
    Guid PetId,
    TipoRegistroClinico Tipo,
    string Titulo,
    string Descricao,
    string? Veterinario,
    DateTime DataRegistro,
    IReadOnlyList<string>? Sintomas,
    IReadOnlyList<MedicamentoRequest>? Medicamentos,
    IReadOnlyList<string>? Tags
)
{
    public Domain.Entities.RegistroClinico ToDomain()
    {
        return new Domain.Entities.RegistroClinico(
            PetId,
            Tipo,
            Titulo,
            Descricao,
            Veterinario,
            DataRegistro,
            Sintomas,
            Medicamentos?.Select(m => m.ToDomain()),
            Tags
        );
    }
}
