using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;

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
        var consulta = request.ToDomain();
        await _repository.AddAsync(consulta);
        return ConsultaResponse.FromDomain(consulta);
    }

    public async Task<ConsultaResponse?> GetByIdAsync(Guid id)
    {
        var consulta = await _repository.GetByIdAsync(id);
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
        var consulta = await _repository.GetByIdAsync(id);
        if (consulta == null)
            throw new Exception("Consulta não encontrada.");

        consulta.DefinirData(request.Data);
        consulta.DefinirHora(request.Hora);
        consulta.AtualizarLocal(request.Local);
        await _repository.UpdateAsync(consulta);
    }

    public async Task DeleteAsync(Guid id)
    {
        var consulta = await _repository.GetByIdAsync(id);
        if (consulta == null) return;
        await _repository.DeleteAsync(consulta);
    }
}