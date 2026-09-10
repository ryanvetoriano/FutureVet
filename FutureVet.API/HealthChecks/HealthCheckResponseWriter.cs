using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FutureVet.API.HealthChecks;

/// <summary>
/// Serializa o resultado dos health checks em JSON estruturado.
/// Expõe apenas nome, status, descrição, duração e tags — nunca connection strings,
/// credenciais ou a mensagem da exceção original, que poderia conter dados sensíveis.
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Sem o encoder relaxado, acentos sairiam escapados ("execução").
        // O payload é montado inteiramente pela aplicação e servido como
        // application/json, nunca interpolado em HTML, então liberar o range
        // completo é seguro e deixa a resposta legível.
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.ToString(),
                tags = entry.Value.Tags
            })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(payload, SerializerOptions),
            context.RequestAborted);
    }
}
