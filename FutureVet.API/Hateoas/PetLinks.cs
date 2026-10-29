using FutureVet.API.Controllers;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Pet;

namespace FutureVet.API.Hateoas;

public sealed class PetLinks : ResourceLinks<PetResponse>
{
    public PetLinks(LinkBuilder links) : base(links) { }

    protected override string Controller => "Pet";

    protected override IEnumerable<LinkResponse> GerarLinks(PetResponse pet)
    {
        yield return Self(pet.Id);
        yield return Atualizar(pet.Id);
        yield return Remover(pet.Id);

        yield return Links.ParaAcao(
            nameof(UsuarioController.GetById), "Usuario",
            new { id = pet.UsuarioId }, "usuario", HttpMethods.Get);

        yield return Links.ParaAcao(
            nameof(VacinaController.GetByPet), "Vacina",
            new { petId = pet.Id }, "vacinas", HttpMethods.Get);

        yield return Links.ParaAcao(
            nameof(ConsultaController.GetByPet), "Consulta",
            new { petId = pet.Id }, "consultas", HttpMethods.Get);

        yield return Links.ParaAcao(
            nameof(RegistroClinicoController.GetByPet), "RegistroClinico",
            new { petId = pet.Id }, "registros-clinicos", HttpMethods.Get);
    }
}
