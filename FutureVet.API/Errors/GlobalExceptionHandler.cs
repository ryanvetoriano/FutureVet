using System.Diagnostics;
using FutureVet.API.Middleware;
using FutureVet.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FutureVet.API.Errors;

/// <summary>
/// Tratamento global de exceções. Traduz as exceções conhecidas do domínio em
/// respostas <c>application/problem+json</c> e transforma qualquer falha inesperada
/// em um 500 genérico, sem vazar stack trace para o cliente.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = Traduzir(exception);

        // Falhas de negócio e 404 são esperadas: ficam em Warning.
        // O que não foi previsto é registrado como Error, com a exceção completa.
        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro não tratado em {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Requisição rejeitada em {Method} {Path} com status {StatusCode}: {Motivo}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                status,
                exception.Message);
        }

        Activity.Current?.SetStatus(ActivityStatusCode.Error, title);

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        problemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? httpContext.TraceIdentifier;

        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.HeaderName, out var correlationId))
            problemDetails.Extensions["correlationId"] = correlationId;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static (int Status, string Title, string Detail) Traduzir(Exception exception)
        => exception switch
        {
            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                exception.Message),

            DomainException => (
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                exception.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno do servidor",
                "Ocorreu um erro inesperado ao processar a requisição.")
        };
}
