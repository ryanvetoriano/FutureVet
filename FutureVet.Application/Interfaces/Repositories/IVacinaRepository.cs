using FutureVet.Domain.Entities;

namespace FutureVet.Application.Interfaces.Repositories;

public interface IVacinaRepository
{
    Task<Vacina?> GetByIdAsync(Guid id);
    Task<IEnumerable<Vacina>> GetAllAsync();
    Task<IEnumerable<Vacina>> GetByPetAsync(Guid petId);
    Task<IEnumerable<Vacina>> GetByProximaDoseAsync(DateTime dataLimite);
    Task AddAsync(Vacina vacina);
    Task UpdateAsync(Vacina vacina);
    Task DeleteAsync(Vacina vacina);
}