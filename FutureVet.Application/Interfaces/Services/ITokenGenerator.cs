using FutureVet.Domain.Entities;

namespace FutureVet.Application.Interfaces.Services;

/// <summary>
/// Emissão do token de acesso. A abstração mantém a camada de Application livre de
/// dependências de JWT; a implementação concreta vive na camada de API.
/// </summary>
public interface ITokenGenerator
{
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario);
}
