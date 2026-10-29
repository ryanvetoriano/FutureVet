using System.Linq.Expressions;
using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence;

/// <summary>
/// Paginação e ordenação traduzidas para SQL pelo EF Core. No Oracle, <c>Skip</c>/<c>Take</c>
/// viram <c>OFFSET ... FETCH NEXT ...</c>: apenas a página pedida sai do banco.
/// </summary>
internal static class QueryableExtensions
{
    public static IOrderedQueryable<T> OrdenarPor<T, TChave>(
        this IQueryable<T> query,
        Expression<Func<T, TChave>> chave,
        bool descendente)
        => descendente
            ? query.OrderByDescending(chave)
            : query.OrderBy(chave);

    public static async Task<PagedResult<T>> PaginarAsync<T>(
        this IQueryable<T> query,
        PaginacaoQuery paginacao)
    {
        var total = await query.CountAsync();

        var itens = await query
            .Skip(paginacao.Deslocamento())
            .Take(paginacao.TamanhoPagina)
            .ToListAsync();

        return new PagedResult<T>(itens, total);
    }
}
