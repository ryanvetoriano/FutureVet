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

    public async Task<IEnumerable<Vacina>> GetAllAsync()
        => await _context.Vacinas.ToListAsync();

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