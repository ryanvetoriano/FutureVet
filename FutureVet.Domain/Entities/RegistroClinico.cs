using FutureVet.Domain.Common;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;
using FutureVet.Domain.ValueObjects;

namespace FutureVet.Domain.Entities;

/// <summary>
/// Entrada do prontuário de um pet: observações, exames, diagnósticos, prescrições e
/// procedimentos.
/// </summary>
/// <remarks>
/// Persistido no <b>MongoDB</b>, e não no Oracle: cada registro tem estrutura variável
/// (listas de sintomas, medicamentos e tags de tamanho livre) e é sempre lido inteiro, como
/// um documento. O vínculo com o pet, que vive no banco relacional, é feito por
/// <see cref="PetId"/> e validado pela camada de Application.
/// </remarks>
public class RegistroClinico : BaseEntity
{
    public Guid PetId { get; private set; }

    public TipoRegistroClinico Tipo { get; private set; }

    public string Titulo { get; private set; } = null!;

    public string Descricao { get; private set; } = null!;

    public string? Veterinario { get; private set; }

    public DateTime DataRegistro { get; private set; }

    public IReadOnlyList<string> Sintomas { get; private set; } = [];

    public IReadOnlyList<Medicamento> Medicamentos { get; private set; } = [];

    public IReadOnlyList<string> Tags { get; private set; } = [];


    private RegistroClinico() { }


    public RegistroClinico(
        Guid petId,
        TipoRegistroClinico tipo,
        string titulo,
        string descricao,
        string? veterinario,
        DateTime dataRegistro,
        IEnumerable<string>? sintomas = null,
        IEnumerable<Medicamento>? medicamentos = null,
        IEnumerable<string>? tags = null)
    {
        if (petId == Guid.Empty)
            throw new DomainException("Pet inválido.");

        PetId = petId;

        Atualizar(
            tipo,
            titulo,
            descricao,
            veterinario,
            dataRegistro,
            sintomas,
            medicamentos,
            tags);
    }


    /// <summary>
    /// Substitui o conteúdo do registro. Todas as regras são verificadas antes de qualquer
    /// alteração, de modo que um registro inválido nunca fica parcialmente atualizado.
    /// </summary>
    public void Atualizar(
        TipoRegistroClinico tipo,
        string titulo,
        string descricao,
        string? veterinario,
        DateTime dataRegistro,
        IEnumerable<string>? sintomas,
        IEnumerable<Medicamento>? medicamentos,
        IEnumerable<string>? tags)
    {
        if (!Enum.IsDefined(tipo))
            throw new DomainException("Tipo de registro clínico inválido.");

        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Título inválido.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição inválida.");

        var data = NormalizarParaUtc(dataRegistro);
        if (data > DateTime.UtcNow.AddDays(1))
            throw new DomainException("A data do registro não pode estar no futuro.");

        var listaMedicamentos = medicamentos?.ToList() ?? [];
        if (tipo == TipoRegistroClinico.Prescricao && listaMedicamentos.Count == 0)
            throw new DomainException("Uma prescrição deve informar ao menos um medicamento.");

        Tipo = tipo;
        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        Veterinario = string.IsNullOrWhiteSpace(veterinario) ? null : veterinario.Trim();
        DataRegistro = data;
        Sintomas = Limpar(sintomas);
        Medicamentos = listaMedicamentos;
        // Tags são normalizadas em minúsculas para que o filtro por tag seja exato.
        Tags = Limpar(tags?.Select(t => t.ToLowerInvariant()));

        AtualizarData();
    }


    /// <summary>
    /// Datas sem fuso (<see cref="DateTimeKind.Unspecified"/>, como chegam do JSON sem "Z")
    /// são tratadas como UTC, para que o valor gravado seja exatamente o informado.
    /// </summary>
    public static DateTime NormalizarParaUtc(DateTime data)
        => data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };

    private static List<string> Limpar(IEnumerable<string>? valores)
        => valores?
               .Where(v => !string.IsNullOrWhiteSpace(v))
               .Select(v => v.Trim())
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .ToList()
           ?? [];
}
