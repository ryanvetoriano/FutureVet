using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Observability;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;

    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "UsuarioService.CreateAsync", "Usuario");

        var usuario = request.ToDomain();
        await _repository.AddAsync(usuario);

        activity?.SetTag("futurevet.entity.id", usuario.Id);

        return UsuarioResponse.FromDomain(usuario);
    }

    public async Task<UsuarioResponse?> GetByIdAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "UsuarioService.GetByIdAsync", "Usuario", id);

        var usuario = await _repository.GetByIdAsync(id);

        activity?.SetTag("futurevet.found", usuario != null);

        return usuario == null ? null : UsuarioResponse.FromDomain(usuario);
    }

    public async Task<UsuarioResponse?> GetByEmailAsync(string email)
    {
        var usuario = await _repository.GetByEmailAsync(email);
        return usuario == null ? null : UsuarioResponse.FromDomain(usuario);
    }

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
    {
        var usuarios = await _repository.GetAllAsync();
        return usuarios.Select(UsuarioResponse.FromDomain);
    }

    public async Task<IEnumerable<UsuarioResponse>> GetByNomeAsync(string nome)
    {
        var usuarios = await _repository.GetByNomeAsync(nome);
        return usuarios.Select(UsuarioResponse.FromDomain);
    }

    public async Task UpdateAsync(Guid id, UpdateUsuarioRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "UsuarioService.UpdateAsync", "Usuario", id);

        var usuario = await _repository.GetByIdAsync(id);
        if (usuario == null)
            throw NotFoundException.For("Usuário", id);

        usuario.AtualizarNome(request.Nome);
        usuario.AtualizarTelefone(request.Telefone);
        await _repository.UpdateAsync(usuario);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "UsuarioService.DeleteAsync", "Usuario", id);

        var usuario = await _repository.GetByIdAsync(id);
        if (usuario == null) return;

        await _repository.DeleteAsync(usuario);
    }
}
