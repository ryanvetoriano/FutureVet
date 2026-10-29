using FutureVet.API.Hateoas;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.RegistroClinico;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

/// <summary>
/// Prontuário dos pets, persistido no MongoDB.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RegistroClinicoController : ControllerBase
{
    private readonly IRegistroClinicoService _service;
    private readonly ResourceLinks<RegistroClinicoResponse> _links;

    public RegistroClinicoController(
        IRegistroClinicoService service,
        ResourceLinks<RegistroClinicoResponse> links)
    {
        _service = service;
        _links = links;
    }

    /// <summary>Lista registros clínicos com paginação, ordenação e filtros.</summary>
    /// <remarks>
    /// Exemplo: <c>GET /api/RegistroClinico?petId={id}&amp;tipo=4&amp;tag=alergia&amp;texto=coceira</c>.
    /// Sem <c>ordenarPor</c>, os registros vêm do mais recente para o mais antigo.
    /// </remarks>
    /// <response code="200">Página de registros clínicos.</response>
    /// <response code="400">Parâmetro de paginação, ordenação ou filtro inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<RegistroClinicoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<RegistroClinicoResponse>>> GetAll(
        [FromQuery] RegistroClinicoFiltro filtro)
    {
        var pagina = await _service.SearchAsync(filtro);
        return Ok(_links.Aplicar(pagina));
    }

    /// <summary>Busca um registro clínico pelo ID.</summary>
    /// <param name="id">ID do registro.</param>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RegistroClinicoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistroClinicoResponse>> GetById(Guid id)
    {
        var registro = await _service.GetByIdAsync(id);
        if (registro == null) return NotFound();
        return Ok(_links.Aplicar(registro));
    }

    /// <summary>Prontuário completo de um pet, do registro mais recente para o mais antigo.</summary>
    /// <param name="petId">ID do pet.</param>
    /// <response code="200">Registros clínicos do pet.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<RegistroClinicoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RegistroClinicoResponse>>> GetByPet(Guid petId)
    {
        var registros = await _service.GetByPetAsync(petId);
        return Ok(_links.Aplicar(registros));
    }

    /// <summary>Cria um registro no prontuário de um pet.</summary>
    /// <response code="201">Registro criado com sucesso.</response>
    /// <response code="400">Dados inválidos (ex.: prescrição sem medicamento).</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RegistroClinicoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Create(CreateRegistroClinicoRequest request)
    {
        // DomainException (400) e NotFoundException do pet (404) são convertidas
        // pelo GlobalExceptionHandler.
        var registro = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = registro.Id }, _links.Aplicar(registro));
    }

    /// <summary>Substitui o conteúdo de um registro clínico.</summary>
    /// <param name="id">ID do registro.</param>
    /// <param name="request">Novo conteúdo do registro.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdateRegistroClinicoRequest request)
    {
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Remove um registro clínico.</summary>
    /// <param name="id">ID do registro.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var registro = await _service.GetByIdAsync(id);
        if (registro == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
