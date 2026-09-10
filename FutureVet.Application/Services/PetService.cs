using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Observability;
using FutureVet.Domain.Enums;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Application.Services;

public class PetService : IPetService
{
    private readonly IPetRepository _repository;

    public PetService(IPetRepository repository)
    {
        _repository = repository;
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "PetService.CreateAsync", "Pet");

        var pet = request.ToDomain();
        await _repository.AddAsync(pet);

        activity?.SetTag("futurevet.entity.id", pet.Id);

        return PetResponse.FromDomain(pet);
    }

    public async Task<IEnumerable<PetResponse>> GetAllAsync()
    {
        var pets = await _repository.GetAllAsync();
        return pets.Select(PetResponse.FromDomain);
    }

    public async Task<PetResponse?> GetByIdAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "PetService.GetByIdAsync", "Pet", id);

        var pet = await _repository.GetByIdAsync(id);

        activity?.SetTag("futurevet.found", pet != null);

        return pet == null ? null : PetResponse.FromDomain(pet);
    }

    public async Task<IEnumerable<PetResponse>> GetByNomeAsync(string nome)
    {
        var pets = await _repository.GetByNomeAsync(nome);
        return pets.Select(PetResponse.FromDomain);
    }

    public async Task<IEnumerable<PetResponse>> GetByEspecieAsync(EspeciePet especie)
    {
        var pets = await _repository.GetByEspecieAsync(especie);
        return pets.Select(PetResponse.FromDomain);
    }

    public async Task<IEnumerable<PetResponse>> GetByUsuarioAsync(Guid usuarioId)
    {
        var pets = await _repository.GetByUsuarioAsync(usuarioId);
        return pets.Select(PetResponse.FromDomain);
    }

    public async Task UpdateAsync(Guid id, UpdatePetRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "PetService.UpdateAsync", "Pet", id);

        var pet = await _repository.GetByIdAsync(id);
        if (pet == null)
            throw NotFoundException.For("Pet", id);

        pet.AtualizarNome(request.NomePet);
        pet.AtualizarRaca(request.Raca);
        pet.DefinirIdade(request.Idade);
        pet.DefinirPorte(request.Tamanho);
        pet.AtualizarPeso(request.Peso);
        await _repository.UpdateAsync(pet);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "PetService.DeleteAsync", "Pet", id);

        var pet = await _repository.GetByIdAsync(id);
        if (pet == null) return;

        await _repository.DeleteAsync(pet);
    }
}
