using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FutureVet.API.Auth;

/// <summary>
/// Isenta do requisito de Bearer os endpoints que são realmente públicos.
/// </summary>
/// <remarks>
/// <para>O requisito é declarado no nível do documento (ver
/// <c>AuthenticationExtensions.AddJwtSecurityDefinition</c>), onde a referência ao esquema
/// consegue ser resolvida contra o <c>OpenApiDocument</c>. Pela especificação OpenAPI, um
/// <c>security</c> vazio na operação anula o requisito global — é assim que leitura,
/// cadastro e login ficam sem cadeado na interface.</para>
/// <para>Sem isso, a documentação marcaria toda a API como protegida, o que não corresponde
/// ao comportamento real.</para>
/// </remarks>
public sealed class SecurityRequirementOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var exigeAutenticacao =
            metadata.OfType<IAuthorizeData>().Any() &&
            !metadata.OfType<IAllowAnonymous>().Any();

        if (exigeAutenticacao)
        {
            operation.Responses ??= [];
            operation.Responses.TryAdd(
                "401",
                new OpenApiResponse { Description = "Token ausente, inválido ou expirado." });

            return;
        }

        // Lista vazia = operação pública, ignorando o requisito declarado no documento.
        operation.Security = [];
    }
}
