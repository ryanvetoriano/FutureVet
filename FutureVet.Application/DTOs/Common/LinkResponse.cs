namespace FutureVet.Application.DTOs.Common;

/// <summary>
/// Link HATEOAS: indica ao cliente uma ação ou um recurso relacionado disponível a partir
/// da resposta atual, sem que ele precise montar URLs por conta própria.
/// </summary>
/// <param name="Href">URL absoluta do recurso ou da ação.</param>
/// <param name="Rel">Relação com o recurso atual (ex.: <c>self</c>, <c>update</c>, <c>next</c>).</param>
/// <param name="Method">Método HTTP a ser usado.</param>
public sealed record LinkResponse(string Href, string Rel, string Method);
