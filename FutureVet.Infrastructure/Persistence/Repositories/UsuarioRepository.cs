using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly FutureVetContext _context;

    public UsuarioRepository(FutureVetContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
        => await _context.Usuarios.Include(x => x.Pets).ToListAsync();

    public async Task<Usuario?> GetByEmailAsync(string email)
        => await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);

    public async Task<IEnumerable<Usuario>> GetByNomeAsync(string nome)
        => await _context.Usuarios
            .Where(x => x.Nome.Contains(nome))
            .ToListAsync();

    public async Task<Usuario?> GetByIdAsync(Guid id)
        => await _context.Usuarios.Include(x => x.Pets).FirstOrDefaultAsync(x => x.Id == id);

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }
}