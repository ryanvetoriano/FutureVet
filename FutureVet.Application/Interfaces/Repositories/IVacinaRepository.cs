using FutureVet.Application.Common;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Domain.Entities;

namespace FutureVet.Application.Interfaces.Repositories;

public interface IVacinaRepository
{
    Task<Vacina?> GetByIdAsync(Guid id);
    Task<PagedResult<Vacina>> SearchAsync(VacinaFiltro filtro);
    Task<IEnumerable<Vacina>> GetByPetAsync(Guid petId);
    Task<IEnumerable<Vacina>> GetByProximaDoseAsync(DateTime dataLimite);
    Task AddAsync(Vacina vacina);
    Task UpdateAsync(Vacina vacina);
    Task DeleteAsync(Vacina vacina);
}