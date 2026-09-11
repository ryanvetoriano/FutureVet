using FutureVet.Application.DTOs.Auth;

namespace FutureVet.Application.Interfaces.Services;

public interface IAuthService
{
    /// <summary>
    /// Valida as credenciais e emite um token de acesso.
    /// Retorna <c>null</c> quando e-mail ou senha não conferem — deliberadamente sem
    /// distinguir os dois casos, para não revelar quais e-mails estão cadastrados.
    /// </summary>
    Task<LoginResponse?> AutenticarAsync(LoginRequest request);
}
