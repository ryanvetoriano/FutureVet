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

    public async Task<IEnumerable<Consulta>> GetAllAsync()
        => await _context.Consultas.ToListAsync();

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