using FutureVet.API.Controllers;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Usuario;

namespace FutureVet.API.Hateoas;

public sealed class UsuarioLinks : ResourceLinks<UsuarioResponse>
{
    public UsuarioLinks(LinkBuilder links) : base(links) { }

    protected override string Controller => "Usuario";

    protected override IEnumerable<LinkResponse> GerarLinks(UsuarioResponse usuario)
    {
        yield return Self(usuario.Id);
        yield return Atualizar(usuario.Id);
        yield return Remover(usuario.Id);

        yield return Links.ParaAcao(
            nameof(PetController.GetByUsuario), "Pet",
            new { usuarioId = usuario.Id }, "pets", HttpMethods.Get);
    }
}
