using FutureVet.Application.DTOs.Common;

namespace FutureVet.Application.DTOs.Usuario;

/// <summary>
/// Filtros, paginação e ordenação de <c>GET /api/Usuario</c>.
/// Ordenação: <c>nome</c> (padrão), <c>email</c> ou <c>dataCriacao</c>.
/// </summary>
public sealed class UsuarioFiltro : PaginacaoQuery
{
    private static readonly string[] Campos = ["nome", "email", "dataCriacao"];

    /// <summary>Parte do nome, sem diferenciar maiúsculas.</summary>
    public string? Nome { get; set; }

    /// <summary>Parte do e-mail, sem diferenciar maiúsculas.</summary>
    public string? Email { get; set; }

    protected override IReadOnlyCollection<string> CamposOrdenacao => Campos;
}
