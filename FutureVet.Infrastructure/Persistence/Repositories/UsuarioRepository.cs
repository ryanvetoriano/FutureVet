using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Usuario;
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

    public async Task<PagedResult<Usuario>> SearchAsync(UsuarioFiltro filtro)
    {
        var query = _context.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim().ToUpper();
            query = query.Where(x => x.Nome.ToUpper().Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Email))
        {
            var email = filtro.Email.Trim().ToUpper();
            query = query.Where(x => x.Email.ToUpper().Contains(email));
        }

        var descendente = filtro.OrdemDescendente();

        var ordenada = filtro.CampoOrdenacao() switch
        {
            "email" => query.OrdenarPor(x => x.Email, descendente),
            "datacriacao" => query.OrdenarPor(x => x.DataCriacao, descendente),
            _ => query.OrdenarPor(x => x.Nome, descendente)
        };

        return await ordenada.ThenBy(x => x.Id).PaginarAsync(filtro);
    }

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
