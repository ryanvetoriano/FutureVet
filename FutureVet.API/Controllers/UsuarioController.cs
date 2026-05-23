using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuarioController(IUsuarioService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os usuários.</summary>
    /// <response code="200">Retorna a lista de usuários.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>Busca um usuário pelo ID.</summary>
    /// <param name="id">ID do usuário.</param>
    /// <response code="200">Usuário encontrado.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> GetById(Guid id)
    {
        var usuario = await _service.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    /// <summary>Busca um usuário pelo e-mail.</summary>
    /// <param name="email">E-mail do usuário.</param>
    /// <response code="200">Usuário encontrado.</response>
    /// <response code="400">E-mail inválido.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> GetByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return BadRequest("E-mail inválido.");

        var usuario = await _service.GetByEmailAsync(email);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    /// <summary>Busca usuários pelo nome (busca parcial).</summary>
    /// <param name="nome">Nome ou parte do nome.</param>
    /// <response code="200">Lista de usuários encontrados.</response>
    /// <response code="400">Nome inválido.</response>
    [HttpGet("nome/{nome}")]
    [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetByNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return BadRequest("Nome inválido.");

        var usuarios = await _service.GetByNomeAsync(nome);
        return Ok(usuarios);
    }

    /// <summary>Cria um novo usuário.</summary>
    /// <response code="201">Usuário criado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var usuario = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    /// <summary>Atualiza nome e telefone de um usuário.</summary>
    /// <param name="id">ID do usuário.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, UpdateUsuarioRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _service.UpdateAsync(id, request);
            return NoContent();
        }
        catch (Exception ex) when (ex.Message.Contains("não encontrado"))
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Remove um usuário.</summary>
    /// <param name="id">ID do usuário.</param>
    /// <response code="204">Removido com sucesso.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var usuario = await _service.GetByIdAsync(id);
        if (usuario == null) return NotFound();

        await _service.DeleteAsync(id);
        return NoContent();
    }
}