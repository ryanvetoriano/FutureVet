using FutureVet.Domain.Exceptions;

namespace FutureVet.Domain.ValueObjects;

/// <summary>
/// Medicamento prescrito em um registro clínico. Não tem identidade própria: só existe
/// como parte do <see cref="Entities.RegistroClinico"/> que o contém.
/// </summary>
public sealed class Medicamento
{
    public string Nome { get; private set; } = null!;

    public string Dosagem { get; private set; } = null!;

    public string? Frequencia { get; private set; }

    public int? DuracaoDias { get; private set; }


    private Medicamento() { }


    public Medicamento(
        string nome,
        string dosagem,
        string? frequencia = null,
        int? duracaoDias = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do medicamento inválido.");

        if (string.IsNullOrWhiteSpace(dosagem))
            throw new DomainException("Dosagem do medicamento inválida.");

        if (duracaoDias is <= 0)
            throw new DomainException("Duração do tratamento inválida.");

        Nome = nome.Trim();
        Dosagem = dosagem.Trim();
        Frequencia = string.IsNullOrWhiteSpace(frequencia) ? null : frequencia.Trim();
        DuracaoDias = duracaoDias;
    }
}
