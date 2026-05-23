using FutureVet.Application.DTOs.Usuario;

namespace FutureVet.Application.Interfaces.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request);
    Task<UsuarioResponse?> GetByIdAsync(Guid id);
    Task<UsuarioResponse?> GetByEmailAsync(string email);
    Task<IEnumerable<UsuarioResponse>> GetAllAsync();
    Task<IEnumerable<UsuarioResponse>> GetByNomeAsync(string nome);
    Task UpdateAsync(Guid id, UpdateUsuarioRequest request);
    Task DeleteAsync(Guid id);
}