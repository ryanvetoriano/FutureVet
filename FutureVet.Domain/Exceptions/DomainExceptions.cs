namespace FutureVet.Domain.Exceptions;

/// <summary>
/// Violação de uma regra de negócio do domínio.
/// Traduzida para HTTP 400 pelo tratamento global de exceções.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Recurso solicitado não existe.
/// Traduzida para HTTP 404 pelo tratamento global de exceções.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException For(string recurso, Guid id)
        => new($"{recurso} com o ID '{id}' não foi encontrado(a).");
}
