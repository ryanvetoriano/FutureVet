namespace FutureVet.Application.Common;

/// <summary>
/// Página de entidades devolvida pelos repositórios: os itens da página solicitada e o total
/// de registros que atendem aos filtros, necessário para calcular o número de páginas.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Itens, int TotalItens);
