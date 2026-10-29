using System.ComponentModel.DataAnnotations;
using FutureVet.Application.DTOs.Common;

namespace FutureVet.Application.DTOs.Vacina;

/// <summary>
/// Filtros, paginação e ordenação de <c>GET /api/Vacina</c>.
/// Ordenação: <c>proximaDose</c> (padrão), <c>nome</c> ou <c>dataAplicacao</c>.
/// </summary>
public sealed class VacinaFiltro : PaginacaoQuery
{
    private static readonly string[] Campos = ["proximaDose", "nome", "dataAplicacao"];

    /// <summary>ID do pet vacinado.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Parte do nome da vacina, sem diferenciar maiúsculas.</summary>
    public string? Nome { get; set; }

    /// <summary>Aplicadas a partir desta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? AplicacaoInicio { get; set; }

    /// <summary>Aplicadas até esta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? AplicacaoFim { get; set; }

    /// <summary>Com próxima dose até esta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? ProximaDoseAte { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => Campos;

    protected override IEnumerable<ValidationResult> ValidarFiltros()
        => new[]
        {
            ValidarIntervalo(AplicacaoInicio, AplicacaoFim, nameof(AplicacaoInicio), nameof(AplicacaoFim))
        }.OfType<ValidationResult>();
}
