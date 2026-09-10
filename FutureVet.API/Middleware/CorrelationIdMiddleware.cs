using System.Diagnostics;
using Serilog.Context;

namespace FutureVet.API.Middleware;

/// <summary>
/// Garante que toda requisição possua um identificador de correlação.
/// O valor enviado pelo cliente em <c>X-Correlation-ID</c> é reaproveitado;
/// caso contrário um novo é gerado. O identificador é devolvido no header de
/// resposta, empurrado para o contexto do Serilog (aparecendo em todos os logs
/// da requisição) e anexado ao <see cref="Activity"/> corrente, ligando logs e traces.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private const int MaxLength = 128;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        context.Items[HeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        // Escrito no OnStarting porque os headers não podem ser alterados
        // depois que a resposta começou a ser enviada.
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        Activity.Current?.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
            return Guid.NewGuid().ToString();

        var candidate = values.FirstOrDefault();

        // Um header vazio ou absurdamente grande vem do cliente e não é confiável:
        // nesses casos geramos um identificador próprio.
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Length > MaxLength)
            return Guid.NewGuid().ToString();

        return candidate;
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
