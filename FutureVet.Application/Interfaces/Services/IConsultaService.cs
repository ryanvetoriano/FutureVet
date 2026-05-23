using FutureVet.Application.DTOs.Consulta;

namespace FutureVet.Application.Interfaces.Services;

public interface IConsultaService
{
    Task<ConsultaResponse> CreateAsync(CreateConsultaRequest request);
    Task<ConsultaResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<ConsultaResponse>> GetAllAsync();
    Task<IEnumerable<ConsultaResponse>> GetByPetAsync(Guid petId);
    Task<IEnumerable<ConsultaResponse>> GetByDataAsync(DateTime data);
    Task<IEnumerable<ConsultaResponse>> GetByTipoAsync(string tipo);
    Task UpdateAsync(Guid id, UpdateConsultaRequest request);
    Task DeleteAsync(Guid id);
}