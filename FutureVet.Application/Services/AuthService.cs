using FutureVet.Application.DTOs.Auth;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Observability;

namespace FutureVet.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _repository;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(
        IUsuarioRepository repository,
        ITokenGenerator tokenGenerator)
    {
        _repository = repository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse?> AutenticarAsync(LoginRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "AuthService.AutenticarAsync", "Usuario");

        var usuario = await _repository.GetByEmailAsync(request.Email);

        // E-mail inexistente e senha errada produzem o mesmo resultado de propósito:
        // distinguir os casos revelaria quais e-mails estão cadastrados.
        if (usuario is null || !usuario.SenhaCorresponde(request.Senha))
        {
            activity?.SetTag("futurevet.auth.sucesso", false);
            return null;
        }

        var (token, expiraEm) = _tokenGenerator.Gerar(usuario);

        activity?.SetTag("futurevet.auth.sucesso", true);
        activity?.SetTag("futurevet.entity.id", usuario.Id);

        return new LoginResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            token,
            expiraEm);
    }
}
