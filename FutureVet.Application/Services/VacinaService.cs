using FutureVet.Application.DTOs.Vacina;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;

namespace FutureVet.Application.Services;

public class VacinaService : IVacinaService
{
    private readonly IVacinaRepository _repository;

    public VacinaService(IVacinaRepository repository)
    {
        _repository = repository;
    }

    public async Task<VacinaResponse> CreateAsync(CreateVacinaRequest request)
    {
        var vacina = request.ToDomain();
        await _repository.AddAsync(vacina);
        return VacinaResponse.FromDomain(vacina);
    }

    public async Task<VacinaResponse?> GetByIdAsync(Guid id)
    {
        var vacina = await _repository.GetByIdAsync(id);
        return vacina == null ? null : VacinaResponse.FromDomain(vacina);
    }

    public async Task<IEnumerable<VacinaResponse>> GetAllAsync()
    {
        var vacinas = await _repository.GetAllAsync();
        return vacinas.Select(VacinaResponse.FromDomain);
    }

    public async Task<IEnumerable<VacinaResponse>> GetByPetAsync(Guid petId)
    {
        var vacinas = await _repository.GetByPetAsync(petId);
        return vacinas.Select(VacinaResponse.FromDomain);
    }

    public async Task<IEnumerable<VacinaResponse>> GetByProximaDoseAsync(DateTime dataLimite)
    {
        var vacinas = await _repository.GetByProximaDoseAsync(dataLimite);
        return vacinas.Select(VacinaResponse.FromDomain);
    }

    public async Task UpdateAsync(Guid id, UpdateVacinaRequest request)
    {
        var vacina = await _repository.GetByIdAsync(id);
        if (vacina == null)
            throw new Exception("Vacina não encontrada.");

        vacina.AtualizarProximaDose(request.ProximaDose);
        vacina.AtualizarLocal(request.LocalAplicacao);
        await _repository.UpdateAsync(vacina);
    }

    public async Task DeleteAsync(Guid id)
    {
        var vacina = await _repository.GetByIdAsync(id);
        if (vacina == null) return;
        await _repository.DeleteAsync(vacina);
    }
}