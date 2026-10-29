using FutureVet.API.Controllers;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Consulta;

namespace FutureVet.API.Hateoas;

public sealed class ConsultaLinks : ResourceLinks<ConsultaResponse>
{
    public ConsultaLinks(LinkBuilder links) : base(links) { }

    protected override string Controller => "Consulta";

    protected override IEnumerable<LinkResponse> GerarLinks(ConsultaResponse consulta)
    {
        yield return Self(consulta.Id);
        yield return Atualizar(consulta.Id);
        yield return Remover(consulta.Id);

        yield return Links.ParaAcao(
            nameof(PetController.GetById), "Pet",
            new { id = consulta.PetId }, "pet", HttpMethods.Get);
    }
}
