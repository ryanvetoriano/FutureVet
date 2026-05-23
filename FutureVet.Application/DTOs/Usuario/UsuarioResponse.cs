using FutureVet.Domain.Entities;

namespace FutureVet.Application.DTOs.Usuario;

public record UsuarioResponse(
    Guid Id,
    string Nome,
    string Email,
    string Cpf,
    string Telefone
)
{
    public static UsuarioResponse FromDomain(
        Domain.Entities.Usuario usuario)
    {
        return new UsuarioResponse(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Cpf,
            usuario.Telefone
        );
    }
}