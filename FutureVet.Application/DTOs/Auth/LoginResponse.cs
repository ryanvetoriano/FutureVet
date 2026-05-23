namespace FutureVet.Application.DTOs.Auth;

public record LoginResponse(
    Guid Id,
    string Nome,
    string Email
);