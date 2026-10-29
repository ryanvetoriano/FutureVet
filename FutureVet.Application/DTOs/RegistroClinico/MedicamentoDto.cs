using FutureVet.Domain.ValueObjects;

namespace FutureVet.Application.DTOs.RegistroClinico;

public record MedicamentoRequest(
    string Nome,
    string Dosagem,
    string? Frequencia,
    int? DuracaoDias
)
{
    public Medicamento ToDomain()
        => new(Nome, Dosagem, Frequencia, DuracaoDias);
}

public record MedicamentoResponse(
    string Nome,
    string Dosagem,
    string? Frequencia,
    int? DuracaoDias
)
{
    public static MedicamentoResponse FromDomain(Medicamento medicamento)
        => new(
            medicamento.Nome,
            medicamento.Dosagem,
            medicamento.Frequencia,
            medicamento.DuracaoDias);
}
