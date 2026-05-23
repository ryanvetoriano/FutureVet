using FutureVet.Application.DTOs.Pet;
using FutureVet.Domain.Enums;

namespace FutureVet.Application.Interfaces.Services;

public interface IPetService
{
    Task<PetResponse> CreateAsync(CreatePetRequest request);
    Task<IEnumerable<PetResponse>> GetAllAsync();
    Task<PetResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<PetResponse>> GetByNomeAsync(string nome);
    Task<IEnumerable<PetResponse>> GetByEspecieAsync(EspeciePet especie);
    Task<IEnumerable<PetResponse>> GetByUsuarioAsync(Guid usuarioId);
    Task UpdateAsync(Guid id, UpdatePetRequest request);
    Task DeleteAsync(Guid id);
}