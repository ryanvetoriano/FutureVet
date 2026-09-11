using System.Text;
using FutureVet.API.Auth;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace FutureVet.API.Extensions;

/// <summary>
/// Autenticação por JWT Bearer.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        // Falha rápida: sem chave válida a API não sobe, em vez de emitir tokens inseguros.
        options.Validar();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(options.SigningKey)),
                    // Sem tolerância de relógio: um token expirado é recusado na hora.
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Adiciona o esquema Bearer ao Swagger, permitindo testar os endpoints
    /// protegidos direto pela interface.
    /// </summary>
    public static void AddJwtSecurityDefinition(this Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Autentique-se em POST /api/Auth/login e informe apenas o token."
        });

        // O requisito vai no nível do documento, único lugar onde a referência ao esquema
        // resolve contra o OpenApiDocument. O filtro abaixo isenta as operações públicas.
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document),
                new List<string>()
            }
        });

        options.OperationFilter<SecurityRequirementOperationFilter>();
    }
}
