using System.Text.RegularExpressions;
using FutureVet.Application.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FutureVet.Infrastructure.NoSql.Repositories;

/// <summary>
/// Repositório do prontuário sobre a coleção <c>registros_clinicos</c> do MongoDB.
/// Cada registro é um documento completo — sintomas, medicamentos e tags embutidos —,
/// lido e gravado em uma única operação, sem joins.
/// </summary>
public class RegistroClinicoRepository : IRegistroClinicoRepository
{
    private static readonly FilterDefinitionBuilder<RegistroClinico> Filtro =
        Builders<RegistroClinico>.Filter;

    private static readonly SortDefinitionBuilder<RegistroClinico> Ordem =
        Builders<RegistroClinico>.Sort;

    private readonly IMongoCollection<RegistroClinico> _colecao;

    public RegistroClinicoRepository(MongoDbContext context)
    {
        _colecao = context.RegistrosClinicos;
    }

    public async Task<RegistroClinico?> GetByIdAsync(Guid id)
        => await _colecao.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<RegistroClinico>> GetByPetAsync(Guid petId)
        => await _colecao
            .Find(x => x.PetId == petId)
            .SortByDescending(x => x.DataRegistro)
            .ToListAsync();

    public async Task<PagedResult<RegistroClinico>> SearchAsync(RegistroClinicoFiltro filtro)
    {
        var filtroMongo = MontarFiltro(filtro);
        var descendente = filtro.OrdemDescendente();

        var ordem = filtro.CampoOrdenacao() switch
        {
            "titulo" => Ordenar(x => x.Titulo, descendente),
            "tipo" => Ordenar(x => x.Tipo, descendente),
            _ => Ordenar(x => x.DataRegistro, descendente)
        };

        var total = await _colecao.CountDocumentsAsync(filtroMongo);

        var itens = await _colecao
            .Find(filtroMongo)
            .Sort(Ordem.Combine(ordem, Ordem.Ascending(x => x.Id)))
            .Skip(filtro.Deslocamento())
            .Limit(filtro.TamanhoPagina)
            .ToListAsync();

        return new PagedResult<RegistroClinico>(itens, (int)total);
    }

    public async Task AddAsync(RegistroClinico registro)
        => await _colecao.InsertOneAsync(registro);

    public async Task UpdateAsync(RegistroClinico registro)
        => await _colecao.ReplaceOneAsync(x => x.Id == registro.Id, registro);

    public async Task DeleteAsync(RegistroClinico registro)
        => await _colecao.DeleteOneAsync(x => x.Id == registro.Id);

    private static FilterDefinition<RegistroClinico> MontarFiltro(RegistroClinicoFiltro filtro)
    {
        var condicoes = new List<FilterDefinition<RegistroClinico>>();

        if (filtro.PetId.HasValue)
            condicoes.Add(Filtro.Eq(x => x.PetId, filtro.PetId.Value));

        if (filtro.Tipo.HasValue)
            condicoes.Add(Filtro.Eq(x => x.Tipo, filtro.Tipo.Value));

        // As tags são gravadas em minúsculas; AnyEq procura o valor dentro do array.
        if (!string.IsNullOrWhiteSpace(filtro.Tag))
            condicoes.Add(Filtro.AnyEq(x => x.Tags, filtro.Tag.Trim().ToLowerInvariant()));

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            // O texto do cliente é escapado: vira busca literal, nunca uma expressão regular.
            var termo = new BsonRegularExpression(Regex.Escape(filtro.Texto.Trim()), "i");
            condicoes.Add(Filtro.Or(
                Filtro.Regex(x => x.Titulo, termo),
                Filtro.Regex(x => x.Descricao, termo)));
        }

        if (filtro.DataInicio.HasValue)
        {
            var inicio = RegistroClinico.NormalizarParaUtc(filtro.DataInicio.Value.Date);
            condicoes.Add(Filtro.Gte(x => x.DataRegistro, inicio));
        }

        if (filtro.DataFim.HasValue)
        {
            var limite = RegistroClinico.NormalizarParaUtc(filtro.DataFim.Value.Date.AddDays(1));
            condicoes.Add(Filtro.Lt(x => x.DataRegistro, limite));
        }

        return condicoes.Count == 0 ? Filtro.Empty : Filtro.And(condicoes);
    }

    private static SortDefinition<RegistroClinico> Ordenar(
        System.Linq.Expressions.Expression<Func<RegistroClinico, object>> campo,
        bool descendente)
        => descendente ? Ordem.Descending(campo) : Ordem.Ascending(campo);
}
