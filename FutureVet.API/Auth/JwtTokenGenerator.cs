using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FutureVet.API.Auth;

/// <summary>
/// Emite tokens JWT assinados com HMAC-SHA256.
/// </summary>
public sealed class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_options.ExpiracaoEmMinutos);

        // O token carrega apenas identificadores. Nada de CPF, telefone ou senha:
        // o payload de um JWT é apenas codificado em Base64, não criptografado.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome)
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
