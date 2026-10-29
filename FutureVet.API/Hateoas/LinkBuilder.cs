using System.Globalization;
using FutureVet.Application.DTOs.Common;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FutureVet.API.Hateoas;

/// <summary>
/// Monta as URLs dos links HATEOAS a partir da tabela de rotas da aplicação, e não de
/// strings fixas: se uma rota mudar, os links acompanham.
/// </summary>
public sealed class LinkBuilder
{
    private const string ParametroPagina = "pagina";

    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LinkBuilder(
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext => _httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("Links HATEOAS só podem ser gerados durante uma requisição.");

    /// <summary>Link para uma action de controller, com URL absoluta.</summary>
    public LinkResponse ParaAcao(
        string acao,
        string controller,
        object? valores,
        string rel,
        string metodo)
    {
        var href = _linkGenerator.GetUriByAction(HttpContext, acao, controller, valores)
            ?? throw new InvalidOperationException(
                $"Nenhuma rota encontrada para {controller}.{acao}.");

        return new LinkResponse(href, rel, metodo);
    }

    /// <summary>
    /// Link para outra página da listagem atual. Filtros, ordenação e tamanho da página da
    /// requisição são preservados; apenas o número da página muda.
    /// </summary>
    public LinkResponse ParaPagina(int pagina, string rel)
    {
        var request = HttpContext.Request;

        var parametros = QueryHelpers.ParseQuery(request.QueryString.Value)
            .Where(p => !p.Key.Equals(ParametroPagina, StringComparison.OrdinalIgnoreCase))
            .Append(new KeyValuePair<string, StringValues>(
                ParametroPagina,
                pagina.ToString(CultureInfo.InvariantCulture)));

        var href = UriHelper.BuildAbsolute(
            request.Scheme,
            request.Host,
            request.PathBase,
            request.Path,
            QueryString.Create(parametros));

        return new LinkResponse(href, rel, HttpMethods.Get);
    }
}
