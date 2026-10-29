using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence.Repositories;

public class VacinaRepository : IVacinaRepository
{
    private readonly FutureVetContext _context;

    public VacinaRepository(FutureVetContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Vacina vacina)
    {
        await _context.Vacinas.AddAsync(vacina);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vacina vacina)
    {
        _context.Vacinas.Remove(vacina);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Vacina>> SearchAsync(VacinaFiltro filtro)
    {
        var query = _context.Vacinas.AsNoTracking();

        if (filtro.PetId.HasValue)
            query = query.Where(x => x.PetId == filtro.PetId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim().ToUpper();
            query = query.Where(x => x.NomeVacina.ToUpper().Contains(nome));
        }

        // Filtros de data comparam dias inteiros: "até 10/01" inclui o dia 10 todo.
        if (filtro.AplicacaoInicio.HasValue)
        {
            var inicio = filtro.AplicacaoInicio.Value.Date;
            query = query.Where(x => x.DataAplicacao >= inicio);
        }

        if (filtro.AplicacaoFim.HasValue)
        {
            var limite = filtro.AplicacaoFim.Value.Date.AddDays(1);
            query = query.Where(x => x.DataAplicacao < limite);
        }

        if (filtro.ProximaDoseAte.HasValue)
        {
            var limite = filtro.ProximaDoseAte.Value.Date.AddDays(1);
            query = query.Where(x => x.ProximaDose < limite);
        }

        var descendente = filtro.OrdemDescendente();

        var ordenada = filtro.CampoOrdenacao() switch
        {
            "nome" => query.OrdenarPor(x => x.NomeVacina, descendente),
            "dataaplicacao" => query.OrdenarPor(x => x.DataAplicacao, descendente),
            _ => query.OrdenarPor(x => x.ProximaDose, descendente)
        };

        return await ordenada.ThenBy(x => x.Id).PaginarAsync(filtro);
    }

    public async Task<IEnumerable<Vacina>> GetByPetAsync(Guid petId)
        => await _context.Vacinas.Where(x => x.PetId == petId).ToListAsync();

    public async Task<IEnumerable<Vacina>> GetByProximaDoseAsync(DateTime dataLimite)
        => await _context.Vacinas
            .Where(x => x.ProximaDose.Date <= dataLimite.Date)
            .OrderBy(x => x.ProximaDose)
            .ToListAsync();

    public async Task<Vacina?> GetByIdAsync(Guid id)
        => await _context.Vacinas.FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateAsync(Vacina vacina)
    {
        _context.Vacinas.Update(vacina);
        await _context.SaveChangesAsync();
    }
}
