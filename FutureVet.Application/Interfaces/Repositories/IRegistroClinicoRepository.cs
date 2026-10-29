using FutureVet.Application.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Domain.Entities;

namespace FutureVet.Application.Interfaces.Repositories;

/// <summary>
/// Repositório do prontuário. A implementação concreta usa MongoDB; a Application não
/// conhece esse detalhe e trabalha apenas com as entidades de domínio.
/// </summary>
public interface IRegistroClinicoRepository
{
    Task<RegistroClinico?> GetByIdAsync(Guid id);
    Task<PagedResult<RegistroClinico>> SearchAsync(RegistroClinicoFiltro filtro);
    Task<IEnumerable<RegistroClinico>> GetByPetAsync(Guid petId);
    Task AddAsync(RegistroClinico registro);
    Task UpdateAsync(RegistroClinico registro);
    Task DeleteAsync(RegistroClinico registro);
}
