using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PetController : ControllerBase
{
    private readonly IPetService _service;

    public PetController(IPetService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os pets.</summary>
    /// <response code="200">Retorna a lista de pets.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PetResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>Busca um pet pelo ID.</summary>
    /// <param name="id">ID do pet.</param>
    /// <response code="200">Pet encontrado.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetResponse>> GetById(Guid id)
    {
        var pet = await _service.GetByIdAsync(id);
        if (pet == null) return NotFound();
        return Ok(pet);
    }

    /// <summary>Busca pets pelo nome (busca parcial).</summary>
    /// <param name="nome">Nome ou parte do nome do pet.</param>
    /// <response code="200">Lista de pets encontrados.</response>
    /// <response code="400">Nome inválido.</response>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(typeof(IEnumerable<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PetResponse>>> GetByNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest("Nome inválido.");

        var pets = await _service.GetByNomeAsync(nome);
        return Ok(pets);
    }

    /// <summary>Busca pets por espécie.</summary>
    /// <param name="especie">Espécie do pet: Cao=1, Gato=2, Coelho=3, Outro=4.</param>
    /// <response code="200">Lista de pets da espécie.</response>
    /// <response code="400">Espécie inválida.</response>
    [HttpGet("especie/{especie:int}")]
    [ProducesResponseType(typeof(IEnumerable<PetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PetResponse>>> GetByEspecie(int especie)
    {
        if (!Enum.IsDefined(typeof(EspeciePet), especie))
            return BadRequest("Espécie inválida. Valores aceitos: 1=Cão, 2=Gato, 3=Coelho, 4=Outro.");

        var pets = await _service.GetByEspecieAsync((EspeciePet)especie);
        return Ok(pets);
    }

    /// <summary>Lista todos os pets de um usuário.</summary>
    /// <param name="usuarioId">ID do usuário dono dos pets.</param>
    /// <response code="200">Lista de pets do usuário.</response>
    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PetResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PetResponse>>> GetByUsuario(Guid usuarioId)
    {
        var pets = await _service.GetByUsuarioAsync(usuarioId);
        return Ok(pets);
    }

    /// <summary>Cria um novo pet.</summary>
    /// <response code="201">Pet criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreatePetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var pet = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, pet);
    }

    /// <summary>Atualiza os dados de um pet.</summary>
    /// <param name="id">ID do pet.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdatePetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var pet = await _service.GetByIdAsync(id);
        if (pet == null) return NotFound();

        await _service.UpdateAsync(id, request);
        return NoContent();
    }

    /// <summary>Remove um pet.</summary>
    /// <param name="id">ID do pet.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Pet não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var pet = await _service.GetByIdAsync(id);
        if (pet == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}