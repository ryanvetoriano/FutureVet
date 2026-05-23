using FutureVet.Domain.Entities;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.Interfaces.Repositories;

public interface IPetRepository
{
    Task<Pet?> GetByIdAsync(Guid id);
    Task<IEnumerable<Pet>> GetAllAsync();
    Task<IEnumerable<Pet>> GetByNomeAsync(string nome);
    Task<IEnumerable<Pet>> GetByEspecieAsync(EspeciePet especie);
    Task<IEnumerable<Pet>> GetByUsuarioAsync(Guid usuarioId);
    Task AddAsync(Pet pet);
    Task UpdateAsync(Pet pet);
    Task DeleteAsync(Pet pet);
}