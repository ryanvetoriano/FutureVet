using FutureVet.API.Hateoas;
using FutureVet.Application.DTOs.Common;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VacinaController : ControllerBase
{
    private readonly IVacinaService _service;
    private readonly ResourceLinks<VacinaResponse> _links;

    public VacinaController(IVacinaService service, ResourceLinks<VacinaResponse> links)
    {
        _service = service;
        _links = links;
    }

    /// <summary>Lista vacinas com paginação, ordenação e filtros.</summary>
    /// <remarks>
    /// Exemplo: <c>GET /api/Vacina?proximaDoseAte=2026-12-31&amp;ordenarPor=proximaDose&amp;pagina=1</c>
    /// lista as doses que vencem até o fim do ano, da mais próxima para a mais distante.
    /// </remarks>
    /// <response code="200">Página de vacinas.</response>
    /// <response code="400">Parâmetro de paginação, ordenação ou filtro inválido.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<VacinaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<VacinaResponse>>> GetAll([FromQuery] VacinaFiltro filtro)
    {
        var pagina = await _service.SearchAsync(filtro);
        return Ok(_links.Aplicar(pagina));
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
        return Ok(_links.Aplicar(vacina));
    }

    /// <summary>Lista todas as vacinas de um pet.</summary>
    /// <param name="petId">ID do pet.</param>
    /// <response code="200">Lista de vacinas do pet.</response>
    [HttpGet("pet/{petId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VacinaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VacinaResponse>>> GetByPet(Guid petId)
    {
        var vacinas = await _service.GetByPetAsync(petId);
        return Ok(_links.Aplicar(vacinas));
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
        return Ok(_links.Aplicar(vacinas));
    }

    /// <summary>Cria uma nova vacina.</summary>
    /// <response code="201">Vacina criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(VacinaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateVacinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vacina = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = vacina.Id }, _links.Aplicar(vacina));
    }

    /// <summary>Atualiza os dados de uma vacina.</summary>
    /// <param name="id">ID da vacina.</param>
    /// <param name="request">Novos dados da vacina.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Vacina não encontrada.</response>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdateVacinaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // NotFoundException e DomainException são convertidas em 404 e 400
        // pelo GlobalExceptionHandler.
        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Remove uma vacina.</summary>
    /// <param name="id">ID da vacina.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Vacina não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
