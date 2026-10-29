using FutureVet.Application.Common;

namespace FutureVet.Application.DTOs.Common;

/// <summary>
/// Página de uma listagem, com os metadados de navegação e os links HATEOAS
/// (<c>self</c>, <c>first</c>, <c>prev</c>, <c>next</c>, <c>last</c>).
/// </summary>
/// <param name="Itens">Itens da página atual.</param>
/// <param name="Pagina">Número da página atual, a partir de 1.</param>
/// <param name="TamanhoPagina">Quantidade máxima de itens por página.</param>
/// <param name="TotalItens">Total de registros que atendem aos filtros.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens
) : ResourceResponse
{
    public int TotalPaginas => TotalItens == 0
        ? 0
        : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);

    public bool TemPaginaAnterior => Pagina > 1;

    public bool TemProximaPagina => Pagina < TotalPaginas;

    public static PagedResponse<T> De<TEntidade>(
        PagedResult<TEntidade> resultado,
        PaginacaoQuery query,
        Func<TEntidade, T> mapear)
        => new(
            resultado.Itens.Select(mapear).ToList(),
            query.Pagina,
            query.TamanhoPagina,
            resultado.TotalItens);
}
