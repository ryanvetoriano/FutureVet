using FutureVet.Application.DTOs.Common;

namespace FutureVet.API.Hateoas;

/// <summary>
/// Decide quais links acompanham um tipo de recurso. Cada recurso tem a sua subclasse;
/// os controllers apenas aplicam os links à resposta.
/// </summary>
/// <remarks>
/// Todos os controllers seguem a mesma convenção de nomes de action (<c>GetById</c>,
/// <c>Create</c>, <c>Update</c>, <c>Delete</c>), o que permite à base montar os links
/// comuns — <c>self</c>, <c>update</c>, <c>delete</c> e a navegação entre páginas.
/// </remarks>
public abstract class ResourceLinks<T> where T : ResourceResponse
{
    protected ResourceLinks(LinkBuilder links)
    {
        Links = links;
    }

    protected LinkBuilder Links { get; }

    /// <summary>Nome do controller do recurso, sem o sufixo <c>Controller</c>.</summary>
    protected abstract string Controller { get; }

    /// <summary>Links específicos de um recurso, incluindo os recursos relacionados.</summary>
    protected abstract IEnumerable<LinkResponse> GerarLinks(T recurso);

    public T Aplicar(T recurso)
        => (T)recurso.ComLinks(GerarLinks(recurso));

    public IReadOnlyList<T> Aplicar(IEnumerable<T> recursos)
        => recursos.Select(Aplicar).ToList();

    public PagedResponse<T> Aplicar(PagedResponse<T> pagina)
    {
        var ultima = Math.Max(pagina.TotalPaginas, 1);

        var links = new List<LinkResponse>
        {
            Links.ParaPagina(pagina.Pagina, "self"),
            Links.ParaPagina(1, "first")
        };

        if (pagina.TemPaginaAnterior)
            links.Add(Links.ParaPagina(Math.Min(pagina.Pagina - 1, ultima), "prev"));

        if (pagina.TemProximaPagina)
            links.Add(Links.ParaPagina(pagina.Pagina + 1, "next"));

        links.Add(Links.ParaPagina(ultima, "last"));
        links.Add(Links.ParaAcao("Create", Controller, null, "create", HttpMethods.Post));

        return pagina with
        {
            Itens = Aplicar(pagina.Itens),
            Links = links
        };
    }

    protected LinkResponse Self(Guid id)
        => Links.ParaAcao("GetById", Controller, new { id }, "self", HttpMethods.Get);

    protected LinkResponse Atualizar(Guid id)
        => Links.ParaAcao("Update", Controller, new { id }, "update", HttpMethods.Put);

    protected LinkResponse Remover(Guid id)
        => Links.ParaAcao("Delete", Controller, new { id }, "delete", HttpMethods.Delete);
}
