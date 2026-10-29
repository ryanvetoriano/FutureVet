using FutureVet.API.Controllers;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Vacina;

namespace FutureVet.API.Hateoas;

public sealed class VacinaLinks : ResourceLinks<VacinaResponse>
{
    public VacinaLinks(LinkBuilder links) : base(links) { }

    protected override string Controller => "Vacina";

    protected override IEnumerable<LinkResponse> GerarLinks(VacinaResponse vacina)
    {
        yield return Self(vacina.Id);
        yield return Atualizar(vacina.Id);
        yield return Remover(vacina.Id);

        yield return Links.ParaAcao(
            nameof(PetController.GetById), "Pet",
            new { id = vacina.PetId }, "pet", HttpMethods.Get);
    }
}
