using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Observability;
using FutureVet.Domain.Exceptions;

namespace FutureVet.Application.Services;

public class ConsultaService : IConsultaService
{
    private readonly IConsultaRepository _repository;

    public ConsultaService(IConsultaRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConsultaResponse> CreateAsync(CreateConsultaRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "ConsultaService.CreateAsync", "Consulta");

        var consulta = request.ToDomain();
        await _repository.AddAsync(consulta);

        activity?.SetTag("futurevet.entity.id", consulta.Id);

        return ConsultaResponse.FromDomain(consulta);
    }

    public async Task<ConsultaResponse?> GetByIdAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "ConsultaService.GetByIdAsync", "Consulta", id);

        var consulta = await _repository.GetByIdAsync(id);

        activity?.SetTag("futurevet.found", consulta != null);

        return consulta == null ? null : ConsultaResponse.FromDomain(consulta);
    }

    public async Task<IEnumerable<ConsultaResponse>> GetAllAsync()
    {
        var consultas = await _repository.GetAllAsync();
        return consultas.Select(ConsultaResponse.FromDomain);
    }

    public async Task<IEnumerable<ConsultaResponse>> GetByPetAsync(Guid petId)
    {
        var consultas = await _repository.GetByPetAsync(petId);
        return consultas.Select(ConsultaResponse.FromDomain);
    }

    public async Task<IEnumerable<ConsultaResponse>> GetByDataAsync(DateTime data)
    {
        var consultas = await _repository.GetByDataAsync(data);
        return consultas.Select(ConsultaResponse.FromDomain);
    }

    public async Task<IEnumerable<ConsultaResponse>> GetByTipoAsync(string tipo)
    {
        var consultas = await _repository.GetByTipoAsync(tipo);
        return consultas.Select(ConsultaResponse.FromDomain);
    }

    public async Task UpdateAsync(Guid id, UpdateConsultaRequest request)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "ConsultaService.UpdateAsync", "Consulta", id);

        var consulta = await _repository.GetByIdAsync(id);
        if (consulta == null)
            throw NotFoundException.For("Consulta", id);

        consulta.DefinirData(request.Data);
        consulta.DefinirHora(request.Hora);
        consulta.AtualizarLocal(request.Local);
        await _repository.UpdateAsync(consulta);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var activity = ApplicationDiagnostics.StartOperation(
            "ConsultaService.DeleteAsync", "Consulta", id);

        var consulta = await _repository.GetByIdAsync(id);
        if (consulta == null) return;

        await _repository.DeleteAsync(consulta);
    }
}
