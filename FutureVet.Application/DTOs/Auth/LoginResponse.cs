namespace FutureVet.Application.DTOs.Auth;

/// <summary>
/// Resultado de uma autenticação bem-sucedida.
/// Não inclui a senha nem qualquer outro dado sensível do usuário.
/// </summary>
public record LoginResponse(
    Guid Id,
    string Nome,
    string Email,
    string Token,
    DateTime ExpiraEm
);
