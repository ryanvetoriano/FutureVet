using FutureVet.Application.DTOs.Auth;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService service, ILogger<AuthController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Autentica um usuário e devolve um token JWT.</summary>
    /// <param name="request">E-mail e senha do usuário.</param>
    /// <response code="200">Autenticado com sucesso. O token deve ser enviado no header Authorization: Bearer.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">E-mail ou senha incorretos.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
            return BadRequest("E-mail e senha são obrigatórios.");

        var resultado = await _service.AutenticarAsync(request);

        if (resultado is null)
        {
            // A senha jamais é registrada. O e-mail entra no log por ser necessário
            // para investigar tentativas de acesso indevido.
            _logger.LogWarning(
                "Tentativa de login malsucedida para o e-mail {Email}.", request.Email);

            return Unauthorized("E-mail ou senha inválidos.");
        }

        return Ok(resultado);
    }
}
