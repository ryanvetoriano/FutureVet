namespace FutureVet.Application.DTOs.Auth;

/// <summary>Credenciais enviadas para autenticação.</summary>
public record LoginRequest(
    string Email,
    string Senha
);
