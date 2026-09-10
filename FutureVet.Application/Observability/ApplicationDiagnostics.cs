using System.Diagnostics;

namespace FutureVet.Application.Observability;

/// <summary>
/// Fonte de <see cref="Activity"/> da camada de Application.
/// A instrumentação automática do OpenTelemetry cobre o ASP.NET Core (requisição),
/// o HttpClient e o EF Core (banco); esta fonte preenche a lacuna entre eles,
/// tornando visível a operação de negócio executada pelos Application Services.
/// </summary>
public static class ApplicationDiagnostics
{
    public const string ActivitySourceName = "FutureVet.Application";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    /// <summary>
    /// Inicia um span para uma operação de negócio. Retorna <c>null</c> quando não há
    /// listener registrado, o que torna a instrumentação praticamente sem custo.
    /// </summary>
    /// <param name="operacao">Nome da operação, ex.: <c>UsuarioService.CreateAsync</c>.</param>
    /// <param name="entidade">Entidade de domínio envolvida.</param>
    /// <param name="entidadeId">Identificador da entidade, quando conhecido.</param>
    public static Activity? StartOperation(
        string operacao,
        string entidade,
        Guid? entidadeId = null)
    {
        var activity = ActivitySource.StartActivity(operacao, ActivityKind.Internal);

        activity?.SetTag("futurevet.entity", entidade);

        if (entidadeId.HasValue)
            activity?.SetTag("futurevet.entity.id", entidadeId.Value);

        return activity;
    }
}
