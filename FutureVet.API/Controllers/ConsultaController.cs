using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConsultaController : ControllerBase
{
    private readonly IConsultaService _service;

    public ConsultaController(IConsultaService service)
    {
        _service = service;
    }

    /// <summary>Lista todas as consultas.</summary>
    /// <response code="200">Retorna a lista de consultas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsultaResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>Busca uma consulta pelo ID.</summary>
    /// <param name="id">ID da consulta.</param>
    /// <response code="200">Consulta encontrada.</response>
    /// <response code="404">Consulta não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConsultaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConsultaResponse>> GetById(Guid id)
    {
        var consulta = await _service.GetByIdAsync(id);
        if (consulta == null) return NotFound();
        return Ok(consulta);
    }

    /// <summary>Lista todas as consultas de um pet.</summary>
    /// <param name="petId">ID do pet.</param>
    /// <response code="200">Lista de consultas do pet.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConsultaResponse>>> GetByPet(Guid petId)
    {
        var consultas = await _service.GetByPetAsync(petId);
        return Ok(consultas);
    }

    /// <summary>Lista consultas por data (formato: yyyy-MM-dd).</summary>
    /// <param name="data">Data da consulta.</param>
    /// <response code="200">Lista de consultas na data informada.</response>
    /// <response code="400">Data inválida.</response>
    [HttpGet("data/{data}")]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ConsultaResponse>>> GetByData(string data)
    {
        if (!DateTime.TryParse(data, out var dataParsed))
            return BadRequest("Data inválida. Use o formato yyyy-MM-dd.");

        var consultas = await _service.GetByDataAsync(dataParsed);
        return Ok(consultas);
    }

    /// <summary>Lista consultas por tipo.</summary>
    /// <param name="tipo">Tipo da consulta (ex: Rotina, Emergência).</param>
    /// <response code="200">Lista de consultas do tipo informado.</response>
    /// <response code="400">Tipo inválido.</response>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(IEnumerable<ConsultaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ConsultaResponse>>> GetByTipo(string tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            return BadRequest("Tipo de consulta inválido.");

        var consultas = await _service.GetByTipoAsync(tipo);
        return Ok(consultas);
    }

    /// <summary>Cria uma nova consulta.</summary>
    /// <response code="201">Consulta criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ConsultaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateConsultaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var consulta = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = consulta.Id }, consulta);
    }

    /// <summary>Atualiza os dados de uma consulta.</summary>
    /// <param name="id">ID da consulta.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Consulta não encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdateConsultaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _service.UpdateAsync(id, request);
            return NoContent();
        }
        catch (Exception ex) when (ex.Message.Contains("não encontrada"))
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Remove uma consulta.</summary>
    /// <param name="id">ID da consulta.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Consulta não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var consulta = await _service.GetByIdAsync(id);
        if (consulta == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}