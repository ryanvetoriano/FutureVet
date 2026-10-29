using FutureVet.API.Controllers;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.RegistroClinico;

namespace FutureVet.API.Hateoas;

public sealed class RegistroClinicoLinks : ResourceLinks<RegistroClinicoResponse>
{
    public RegistroClinicoLinks(LinkBuilder links) : base(links) { }

    protected override string Controller => "RegistroClinico";

    protected override IEnumerable<LinkResponse> GerarLinks(RegistroClinicoResponse registro)
    {
        yield return Self(registro.Id);
        yield return Atualizar(registro.Id);
        yield return Remover(registro.Id);

        yield return Links.ParaAcao(
            nameof(PetController.GetById), "Pet",
            new { id = registro.PetId }, "pet", HttpMethods.Get);

        yield return Links.ParaAcao(
            nameof(RegistroClinicoController.GetByPet), "RegistroClinico",
            new { petId = registro.PetId }, "prontuario", HttpMethods.Get);
    }
}
