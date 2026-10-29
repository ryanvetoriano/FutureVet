using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.RegistroClinico;

namespace FutureVet.Application.Interfaces.Services;

public interface IRegistroClinicoService
{
    Task<RegistroClinicoResponse> CreateAsync(CreateRegistroClinicoRequest request);
    Task<RegistroClinicoResponse?> GetByIdAsync(Guid id);
    Task<PagedResponse<RegistroClinicoResponse>> SearchAsync(RegistroClinicoFiltro filtro);
    Task<IEnumerable<RegistroClinicoResponse>> GetByPetAsync(Guid petId);
    Task UpdateAsync(Guid id, UpdateRegistroClinicoRequest request);
    Task DeleteAsync(Guid id);
}
