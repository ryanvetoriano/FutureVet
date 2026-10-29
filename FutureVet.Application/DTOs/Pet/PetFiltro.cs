using System.ComponentModel.DataAnnotations;
using FutureVet.Application.DTOs.Common;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.Pet;

/// <summary>
/// Filtros, paginação e ordenação de <c>GET /api/Pet</c>.
/// Ordenação: <c>nome</c> (padrão), <c>especie</c>, <c>idade</c>, <c>peso</c> ou <c>dataCriacao</c>.
/// </summary>
public sealed class PetFiltro : PaginacaoQuery
{
    private static readonly string[] Campos = ["nome", "especie", "idade", "peso", "dataCriacao"];

    /// <summary>Parte do nome do pet, sem diferenciar maiúsculas.</summary>
    public string? Nome { get; set; }

    /// <summary>Espécie: 1=Cão, 2=Gato, 3=Coelho, 4=Outro.</summary>
    public EspeciePet? Especie { get; set; }

    /// <summary>Porte: 1=Pequeno, 2=Médio, 3=Grande.</summary>
    public PortePet? Tamanho { get; set; }

    /// <summary>ID do dono do pet.</summary>
    public Guid? UsuarioId { get; set; }

    /// <summary>Idade mínima, inclusive.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "A idade mínima não pode ser negativa.")]
    public int? IdadeMinima { get; set; }

    /// <summary>Idade máxima, inclusive.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "A idade máxima não pode ser negativa.")]
    public int? IdadeMaxima { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => Campos;

    protected override IEnumerable<ValidationResult> ValidarFiltros()
        => new[]
        {
            ValidarEnum(Especie, nameof(Especie)),
            ValidarEnum(Tamanho, nameof(Tamanho)),
            ValidarIntervalo(IdadeMinima, IdadeMaxima, nameof(IdadeMinima), nameof(IdadeMaxima))
        }.OfType<ValidationResult>();
}
