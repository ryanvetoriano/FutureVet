using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.RegistroClinico;

/// <summary>
/// Substitui o conteúdo do registro. O pet não pode ser trocado: um registro clínico
/// pertence ao prontuário do pet em que foi criado.
/// </summary>
public record UpdateRegistroClinicoRequest(
    TipoRegistroClinico Tipo,
    string Titulo,
    string Descricao,
    string? Veterinario,
    DateTime DataRegistro,
    IReadOnlyList<string>? Sintomas,
    IReadOnlyList<MedicamentoRequest>? Medicamentos,
    IReadOnlyList<string>? Tags
);
