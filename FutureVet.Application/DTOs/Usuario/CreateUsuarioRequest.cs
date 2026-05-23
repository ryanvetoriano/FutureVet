using FutureVet.Domain.Entities;

namespace FutureVet.Application.DTOs.Usuario;

public record CreateUsuarioRequest(
    string Nome,
    string Email,
    string Senha,
    string Cpf,
    string Telefone
)
{
    public Domain.Entities.Usuario ToDomain()
    {
        return new Domain.Entities.Usuario(
            Nome,
            Email,
            Senha,
            Cpf,
            Telefone
        );
    }
}