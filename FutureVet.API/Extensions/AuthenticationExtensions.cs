using System.Text;
using FutureVet.API.Auth;
using FutureVet.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.OpenApi;

namespace FutureVet.API.Extensions;

/// <summary>
/// Autenticação por JWT Bearer.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var options = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        // Em Development, uma chave ausente não deve impedir a aplicação de subir: quem
        // acabou de clonar o repositório consegue rodar e explorar a API de imediato.
        // A chave é gerada por processo, então os tokens deixam de valer a cada reinício —
        // aceitável localmente, inaceitável em produção.
        if (string.IsNullOrWhiteSpace(options.SigningKey) && environment.IsDevelopment())
        {
            options.SigningKey = JwtOptions.GerarChaveAleatoria();

            Log.Warning(
                "Nenhuma chave de assinatura do JWT foi configurada. Uma chave efêmera foi "
                + "gerada para este processo, e os tokens emitidos deixarão de valer quando a "
                + "API reiniciar. Isso só acontece em Development. Para uma chave estável: "
                + "dotnet user-secrets set \"Jwt:SigningKey\" \"<chave>\" --project FutureVet.API");
        }

        // Fora de Development a falha é imediata: melhor não subir do que emitir tokens
        // assinados com uma chave fraca, ausente ou improvisada.
        options.Validar();

        // A configuração registrada no DI precisa carregar a MESMA chave usada acima para
        // validar os tokens — inclusive quando ela veio do fallback de desenvolvimento.
        services.Configure<JwtOptions>(jwt =>
        {
            jwt.Issuer = options.Issuer;
            jwt.Audience = options.Audience;
            jwt.SigningKey = options.SigningKey;
            jwt.ExpiracaoEmMinutos = options.ExpiracaoEmMinutos;
        });

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
