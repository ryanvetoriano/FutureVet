using FutureVet.Application.DTOs.Vacina;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VacinaController : ControllerBase
{
    private readonly IVacinaService _service;

    public VacinaController(IVacinaService service)
    {
        _service = service;
    }

    /// <summary>Lista todas as vacinas.</summary>
    /// <response code="200">Retorna a lista de vacinas.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>Busca uma vacina pelo ID.</summary>
    /// <param name="id">ID da vacina.</param>
    /// <response code="200">Vacina encontrada.</response>
    /// <response code="404">Vacina não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VacinaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VacinaResponse>> GetById(Guid id)
    {
        var vacina = await _service.GetByIdAsync(id);
        if (vacina == null) return NotFound();
        return Ok(vacina);
    }

    /// <summary>Lista todas as vacinas de um pet.</summary>
    /// <param name="petId">ID do pet.</param>
    /// <response code="200">Lista de vacinas do pet.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponse>>> GetByPet(Guid petId)
    {
        var vacinas = await _service.GetByPetAsync(petId);
        return Ok(vacinas);
    }

    /// <summary>Lista vacinas com próxima dose até uma data.</summary>
    /// <param name="data">Data limite (formato: yyyy-MM-dd).</param>
    /// <response code="200">Lista de vacinas com dose pendente.</response>
    /// <response code="400">Data inválida.</response>
    [HttpGet("proxima-dose/{data}")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<VacinaResponse>>> GetByProximaDose(string data)
    {
        if (!DateTime.TryParse(data, out var dataLimite))
            return BadRequest("Data inválida. Use o formato yyyy-MM-dd.");

        var vacinas = await _service.GetByProximaDoseAsync(dataLimite);
        return Ok(vacinas);
    }

    /// <summary>Cria uma nova vacina.</summary>
    /// <response code="201">Vacina criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VacinaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateVacinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vacina = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = vacina.Id }, vacina);
    }

    /// <summary>Atualiza os dados de uma vacina.</summary>
    /// <param name="id">ID da vacina.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Vacina não encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdateVacinaRequest request)
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

    /// <summary>Remove uma vacina.</summary>
    /// <param name="id">ID da vacina.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Vacina não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var vacina = await _service.GetByIdAsync(id);
        if (vacina == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}