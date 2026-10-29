using FutureVet.Application.DTOs.Common;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.RegistroClinico;

public record RegistroClinicoResponse(
    Guid Id,
    Guid PetId,
    TipoRegistroClinico Tipo,
    string Titulo,
    string Descricao,
    string? Veterinario,
    DateTime DataRegistro,
    IReadOnlyList<string> Sintomas,
    IReadOnlyList<MedicamentoResponse> Medicamentos,
    IReadOnlyList<string> Tags,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
) : ResourceResponse
{
    public static RegistroClinicoResponse FromDomain(
        Domain.Entities.RegistroClinico registro)
    {
        return new RegistroClinicoResponse(
            registro.Id,
            registro.PetId,
            registro.Tipo,
            registro.Titulo,
            registro.Descricao,
            registro.Veterinario,
            registro.DataRegistro,
            registro.Sintomas,
            registro.Medicamentos.Select(MedicamentoResponse.FromDomain).ToList(),
            registro.Tags,
            registro.DataCriacao,
            registro.DataAtualizacao
        );
    }
}
