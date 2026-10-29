using System.ComponentModel.DataAnnotations;
using FutureVet.Application.DTOs.Common;

namespace FutureVet.Application.DTOs.Consulta;

/// <summary>
/// Filtros, paginação e ordenação de <c>GET /api/Consulta</c>.
/// Ordenação: <c>data</c> (padrão), <c>tipo</c> ou <c>local</c>.
/// </summary>
public sealed class ConsultaFiltro : PaginacaoQuery
{
    private static readonly string[] Campos = ["data", "tipo", "local"];

    /// <summary>ID do pet atendido.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Parte do tipo da consulta (ex.: Rotina), sem diferenciar maiúsculas.</summary>
    public string? Tipo { get; set; }

    /// <summary>Parte do local da consulta, sem diferenciar maiúsculas.</summary>
    public string? Local { get; set; }

    /// <summary>Consultas a partir desta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? DataInicio { get; set; }

    /// <summary>Consultas até esta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? DataFim { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => Campos;

    protected override IEnumerable<ValidationResult> ValidarFiltros()
        => new[]
        {
            ValidarIntervalo(DataInicio, DataFim, nameof(DataInicio), nameof(DataFim))
        }.OfType<ValidationResult>();
}
