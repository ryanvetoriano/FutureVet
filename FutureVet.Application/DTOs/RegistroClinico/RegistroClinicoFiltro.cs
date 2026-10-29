using System.ComponentModel.DataAnnotations;
using FutureVet.Application.DTOs.Common;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.DTOs.RegistroClinico;

/// <summary>
/// Filtros, paginação e ordenação de <c>GET /api/RegistroClinico</c>.
/// Ordenação: <c>dataRegistro</c> (padrão, mais recentes primeiro), <c>titulo</c> ou <c>tipo</c>.
/// </summary>
public sealed class RegistroClinicoFiltro : PaginacaoQuery
{
    private static readonly string[] Campos = ["dataRegistro", "titulo", "tipo"];

    /// <summary>ID do pet.</summary>
    public Guid? PetId { get; set; }

    /// <summary>Tipo: 1=Observação, 2=Exame, 3=Diagnóstico, 4=Prescrição, 5=Procedimento.</summary>
    public TipoRegistroClinico? Tipo { get; set; }

    /// <summary>Texto procurado no título e na descrição, sem diferenciar maiúsculas.</summary>
    public string? Texto { get; set; }

    /// <summary>Registros marcados com esta tag.</summary>
    public string? Tag { get; set; }

    /// <summary>Registros a partir desta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? DataInicio { get; set; }

    /// <summary>Registros até esta data (yyyy-MM-dd), inclusive.</summary>
    public DateTime? DataFim { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => Campos;

    // O histórico clínico é lido do mais recente para o mais antigo.
    protected override bool DescendentePorPadrao => true;

    protected override IEnumerable<ValidationResult> ValidarFiltros()
        => new[]
        {
            ValidarEnum(Tipo, nameof(Tipo)),
            ValidarIntervalo(DataInicio, DataFim, nameof(DataInicio), nameof(DataFim))
        }.OfType<ValidationResult>();
}
