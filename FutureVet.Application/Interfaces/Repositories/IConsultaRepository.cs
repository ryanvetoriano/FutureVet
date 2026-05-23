using FutureVet.Domain.Entities;

namespace FutureVet.Application.Interfaces.Repositories;

public interface IConsultaRepository
{
    Task<Consulta?> GetByIdAsync(Guid id);
    Task<IEnumerable<Consulta>> GetAllAsync();
    Task<IEnumerable<Consulta>> GetByPetAsync(Guid petId);
    Task<IEnumerable<Consulta>> GetByDataAsync(DateTime data);
    Task<IEnumerable<Consulta>> GetByTipoAsync(string tipo);
    Task AddAsync(Consulta consulta);
    Task UpdateAsync(Consulta consulta);
    Task DeleteAsync(Consulta consulta);
}