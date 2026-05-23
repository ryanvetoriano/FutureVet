using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence.Repositories;

public class PetRepository : IPetRepository
{
    private readonly FutureVetContext _context;

    public PetRepository(FutureVetContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Pet pet)
    {
        await _context.Pets.AddAsync(pet);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Pet pet)
    {
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Pet>> GetAllAsync()
        => await _context.Pets.Include(x => x.Vacinas).Include(x => x.Consultas).ToListAsync();

    public async Task<IEnumerable<Pet>> GetByEspecieAsync(EspeciePet especie)
        => await _context.Pets.Where(x => x.Especie == especie).ToListAsync();

    public async Task<IEnumerable<Pet>> GetByNomeAsync(string nome)
        => await _context.Pets.Where(x => x.NomePet.Contains(nome)).ToListAsync();

    public async Task<IEnumerable<Pet>> GetByUsuarioAsync(Guid usuarioId)
        => await _context.Pets
            .Where(x => x.UsuarioId == usuarioId)
            .Include(x => x.Vacinas)
            .Include(x => x.Consultas)
            .ToListAsync();

    public async Task<Pet?> GetByIdAsync(Guid id)
        => await _context.Pets
            .Include(x => x.Vacinas)
            .Include(x => x.Consultas)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateAsync(Pet pet)
    {
        _context.Pets.Update(pet);
        await _context.SaveChangesAsync();
    }
}