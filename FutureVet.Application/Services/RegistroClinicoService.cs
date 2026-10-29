using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Observability;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Application.Services;

/// <summary>
/// Prontuário dos pets. Os registros ficam no MongoDB e os pets no Oracle: este serviço é o
/// ponto em que as duas bases se encontram, garantindo que nenhum registro seja criado para
/// um pet inexistente.
/// </summary>
public class RegistroClinicoService : IRegistroClinicoService
{
    private readonly IRegistroClinicoRepository _repository;
    private readonly IPetRepository _petRepository;

    public RegistroClinicoService(
        IRegistroClinicoRepository repository,
        IPetRepository petRepository)
    {
        _repository = repository;
        _petRepository = petRepository;
    }

    public async Task<RegistroClinicoResponse> CreateAsync(CreateRegistroClinicoRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "RegistroClinicoService.CreateAsync", "RegistroClinico");

        // As regras do domínio são verificadas antes de qualquer acesso a banco.
        var registro = request.ToDomain();

        if (!await _petRepository.ExistsAsync(request.PetId))
            throw NotFoundException.For("Pet", request.PetId);

        await _repository.AddAsync(registro);

        activity?.SetTag("futurevet.entity.id", registro.Id);

        return RegistroClinicoResponse.FromDomain(registro);
    }

    public async Task<RegistroClinicoResponse?> GetByIdAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "RegistroClinicoService.GetByIdAsync", "RegistroClinico", id);

        var registro = await _repository.GetByIdAsync(id);

        activity?.SetTag("futurevet.found", registro != null);

        return registro == null ? null : RegistroClinicoResponse.FromDomain(registro);
    }

    public async Task<PagedResponse<RegistroClinicoResponse>> SearchAsync(RegistroClinicoFiltro filtro)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "RegistroClinicoService.SearchAsync", "RegistroClinico");

        var resultado = await _repository.SearchAsync(filtro);

        activity?.SetTag("futurevet.total", resultado.TotalItens);

        return PagedResponse<RegistroClinicoResponse>.De(
            resultado, filtro, RegistroClinicoResponse.FromDomain);
    }

    public async Task<IEnumerable<RegistroClinicoResponse>> GetByPetAsync(Guid petId)
    {
        var registros = await _repository.GetByPetAsync(petId);
        return registros.Select(RegistroClinicoResponse.FromDomain);
    }

    public async Task UpdateAsync(Guid id, UpdateRegistroClinicoRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "RegistroClinicoService.UpdateAsync", "RegistroClinico", id);

        var registro = await _repository.GetByIdAsync(id);
        if (registro == null)
            throw NotFoundException.For("Registro clínico", id);

        registro.Atualizar(
            request.Tipo,
            request.Titulo,
            request.Descricao,
            request.Veterinario,
            request.DataRegistro,
            request.Sintomas,
            request.Medicamentos?.Select(m => m.ToDomain()),
            request.Tags);

        await _repository.UpdateAsync(registro);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "RegistroClinicoService.DeleteAsync", "RegistroClinico", id);

        var registro = await _repository.GetByIdAsync(id);
        if (registro == null) return;

        await _repository.DeleteAsync(registro);
    }
}
