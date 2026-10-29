using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence.Repositories;

public class ConsultaRepository : IConsultaRepository
{
    private readonly FutureVetContext _context;

    public ConsultaRepository(FutureVetContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Consulta consulta)
    {
        await _context.Consultas.AddAsync(consulta);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Consulta consulta)
    {
        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<Consulta>> SearchAsync(ConsultaFiltro filtro)
    {
        var query = _context.Consultas.AsNoTracking();

        if (filtro.PetId.HasValue)
            query = query.Where(x => x.PetId == filtro.PetId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
        {
            var tipo = filtro.Tipo.Trim().ToUpper();
            query = query.Where(x => x.TipoConsulta.ToUpper().Contains(tipo));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Local))
        {
            var local = filtro.Local.Trim().ToUpper();
            query = query.Where(x => x.Local.ToUpper().Contains(local));
        }

        // Filtros de data comparam dias inteiros: "até 10/01" inclui o dia 10 todo.
        if (filtro.DataInicio.HasValue)
        {
            var inicio = filtro.DataInicio.Value.Date;
            query = query.Where(x => x.Data >= inicio);
        }

        if (filtro.DataFim.HasValue)
        {
            var limite = filtro.DataFim.Value.Date.AddDays(1);
            query = query.Where(x => x.Data < limite);
        }

        var descendente = filtro.OrdemDescendente();

        var ordenada = filtro.CampoOrdenacao() switch
        {
            "tipo" => query.OrdenarPor(x => x.TipoConsulta, descendente),
            "local" => query.OrdenarPor(x => x.Local, descendente),
            _ => query.OrdenarPor(x => x.Data, descendente)
        };

        return await ordenada.ThenBy(x => x.Id).PaginarAsync(filtro);
    }

    public async Task<IEnumerable<Consulta>> GetByPetAsync(Guid petId)
        => await _context.Consultas.Where(x => x.PetId == petId).ToListAsync();

    public async Task<IEnumerable<Consulta>> GetByDataAsync(DateTime data)
        => await _context.Consultas
            .Where(x => x.Data.Date == data.Date)
            .ToListAsync();

    public async Task<IEnumerable<Consulta>> GetByTipoAsync(string tipo)
        => await _context.Consultas
            .Where(x => x.TipoConsulta.Contains(tipo))
            .ToListAsync();

    public async Task<Consulta?> GetByIdAsync(Guid id)
        => await _context.Consultas.FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateAsync(Consulta consulta)
    {
        _context.Consultas.Update(consulta);
        await _context.SaveChangesAsync();
    }
}
