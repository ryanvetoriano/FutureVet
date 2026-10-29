using System.ComponentModel.DataAnnotations;

namespace FutureVet.Application.DTOs.Common;

/// <summary>
/// Parâmetros de paginação e ordenação comuns a todas as listagens. Cada recurso herda esta
/// classe, acrescenta os próprios filtros e declara os campos pelos quais aceita ser ordenado.
/// </summary>
/// <remarks>
/// A validação roda no model binding: com <c>[ApiController]</c>, um parâmetro inválido
/// devolve <c>400</c> com <c>ValidationProblemDetails</c> antes de chegar ao Application Service.
/// </remarks>
public abstract class PaginacaoQuery : IValidatableObject
{
    public const int TamanhoPaginaPadrao = 10;
    public const int TamanhoPaginaMaximo = 50;

    // Limite que mantém (Pagina - 1) * TamanhoPagina dentro de um int.
    public const int PaginaMaxima = 1_000_000;

    /// <summary>Número da página, a partir de 1.</summary>
    [Range(1, PaginaMaxima, ErrorMessage = "A página deve estar entre 1 e 1000000.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de itens por página, de 1 a 50.</summary>
    [Range(1, TamanhoPaginaMaximo, ErrorMessage = "O tamanho da página deve estar entre 1 e 50.")]
    public int TamanhoPagina { get; set; } = TamanhoPaginaPadrao;

    /// <summary>Campo de ordenação. Os valores aceitos variam por recurso.</summary>
    public string? OrdenarPor { get; set; }

    /// <summary>Direção da ordenação: <c>asc</c> ou <c>desc</c>.</summary>
    public string? Direcao { get; set; }

    /// <summary>Campos aceitos em <see cref="OrdenarPor"/>, sem diferenciar maiúsculas.</summary>
    protected abstract IReadOnlyCollection<string> CamposOrdenacao { get; }

    /// <summary>Direção usada quando o cliente não informa <see cref="Direcao"/>.</summary>
    protected virtual bool DescendentePorPadrao => false;

    /// <summary>Quantidade de registros a pular para chegar à página solicitada.</summary>
    public int Deslocamento() => (Pagina - 1) * TamanhoPagina;

    /// <summary>
    /// Campo de ordenação normalizado em minúsculas, ou <c>null</c> quando o cliente não
    /// informou nenhum — caso em que cada repositório aplica a ordenação padrão do recurso.
    /// </summary>
    public string? CampoOrdenacao()
        => string.IsNullOrWhiteSpace(OrdenarPor)
            ? null
            : OrdenarPor.Trim().ToLowerInvariant();

    public bool OrdemDescendente()
        => string.IsNullOrWhiteSpace(Direcao)
            ? DescendentePorPadrao
            : Direcao.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var erros = new List<ValidationResult>();

        var campo = CampoOrdenacao();
        if (campo != null &&
            !CamposOrdenacao.Contains(campo, StringComparer.OrdinalIgnoreCase))
        {
            erros.Add(new ValidationResult(
                $"Campo de ordenação inválido. Valores aceitos: {string.Join(", ", CamposOrdenacao)}.",
                [nameof(OrdenarPor)]));
        }

        if (!string.IsNullOrWhiteSpace(Direcao) &&
            !Direcao.Trim().Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !Direcao.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase))
        {
            erros.Add(new ValidationResult(
                "Direção de ordenação inválida. Valores aceitos: asc, desc.",
                [nameof(Direcao)]));
        }

        erros.AddRange(ValidarFiltros());

        return erros;
    }

    /// <summary>Validações dos filtros específicos de cada recurso.</summary>
    protected virtual IEnumerable<ValidationResult> ValidarFiltros() => [];

    protected static ValidationResult? ValidarIntervalo<TValor>(
        TValor? inicio,
        TValor? fim,
        string campoInicio,
        string campoFim)
        where TValor : struct, IComparable<TValor>
        => inicio.HasValue && fim.HasValue && inicio.Value.CompareTo(fim.Value) > 0
            ? new ValidationResult(
                $"{campoInicio} não pode ser posterior a {campoFim}.",
                [campoInicio, campoFim])
            : null;

    protected static ValidationResult? ValidarEnum<TEnum>(TEnum? valor, string campo)
        where TEnum : struct, Enum
        => valor.HasValue && !Enum.IsDefined(valor.Value)
            ? new ValidationResult($"Valor inválido para {campo}.", [campo])
            : null;
}
