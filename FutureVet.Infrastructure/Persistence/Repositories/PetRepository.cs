using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Pet;
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

    public async Task<PagedResult<Pet>> SearchAsync(PetFiltro filtro)
    {
        var query = _context.Pets.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            var nome = filtro.Nome.Trim().ToUpper();
            query = query.Where(x => x.NomePet.ToUpper().Contains(nome));
        }

        if (filtro.Especie.HasValue)
            query = query.Where(x => x.Especie == filtro.Especie.Value);

        if (filtro.Tamanho.HasValue)
            query = query.Where(x => x.Tamanho == filtro.Tamanho.Value);

        if (filtro.UsuarioId.HasValue)
            query = query.Where(x => x.UsuarioId == filtro.UsuarioId.Value);

        if (filtro.IdadeMinima.HasValue)
            query = query.Where(x => x.Idade >= filtro.IdadeMinima.Value);

        if (filtro.IdadeMaxima.HasValue)
            query = query.Where(x => x.Idade <= filtro.IdadeMaxima.Value);

        var descendente = filtro.OrdemDescendente();

        var ordenada = filtro.CampoOrdenacao() switch
        {
            "especie" => query.OrdenarPor(x => x.Especie, descendente),
            "idade" => query.OrdenarPor(x => x.Idade, descendente),
            "peso" => query.OrdenarPor(x => x.Peso, descendente),
            "datacriacao" => query.OrdenarPor(x => x.DataCriacao, descendente),
            _ => query.OrdenarPor(x => x.NomePet, descendente)
        };

        // Desempate pelo ID: sem ele, registros com o mesmo valor de ordenação poderiam
        // trocar de página entre uma requisição e outra.
        return await ordenada.ThenBy(x => x.Id).PaginarAsync(filtro);
    }

    // COUNT em vez de Any: o Any vira "CASE WHEN EXISTS ... THEN TRUE ELSE FALSE", e
    // literais booleanos em SQL só existem a partir do Oracle 23 (ORA-00904 no 19c da FIAP).
    public async Task<bool> ExistsAsync(Guid id)
        => await _context.Pets.CountAsync(x => x.Id == id) > 0;

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
