using FutureVet.Application.DTOs.Vacina;

namespace FutureVet.Application.Interfaces.Services;

public interface IVacinaService
{
    Task<VacinaResponse> CreateAsync(CreateVacinaRequest request);
    Task<VacinaResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<VacinaResponse>> GetAllAsync();
    Task<IEnumerable<VacinaResponse>> GetByPetAsync(Guid petId);
    Task<IEnumerable<VacinaResponse>> GetByProximaDoseAsync(DateTime dataLimite);
    Task UpdateAsync(Guid id, UpdateVacinaRequest request);
    Task DeleteAsync(Guid id);
}