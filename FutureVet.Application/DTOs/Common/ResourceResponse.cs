namespace FutureVet.Application.DTOs.Common;

/// <summary>
/// Base das respostas que carregam links HATEOAS.
/// </summary>
/// <remarks>
/// A Application apenas define o formato da representação. Quais links existem — e as URLs
/// de cada um — são decididos pela camada de API, a única que conhece as rotas HTTP.
/// </remarks>
public abstract record ResourceResponse
{
    /// <summary>Ações e recursos relacionados disponíveis a partir deste recurso.</summary>
    public IReadOnlyList<LinkResponse> Links { get; init; } = [];

    /// <summary>Cópia desta resposta com os links informados.</summary>
    public ResourceResponse ComLinks(IEnumerable<LinkResponse> links)
        => this with { Links = [.. links] };
}
