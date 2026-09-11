namespace FutureVet.API.Auth;

/// <summary>
/// Configuração do JWT, lida da seção <c>Jwt</c>.
/// </summary>
/// <remarks>
/// A <see cref="SigningKey"/> <b>nunca</b> é versionada: vem de User Secrets, de variável de
/// ambiente (<c>Jwt__SigningKey</c>) ou do cofre do ambiente de produção. O
/// <c>appsettings.json</c> do repositório traz apenas Issuer, Audience e expiração.
/// </remarks>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Tamanho mínimo da chave para HMAC-SHA256 (256 bits).</summary>
    public const int TamanhoMinimoDaChaveEmBytes = 32;

    public string Issuer { get; set; } = "FutureVet.API";

    public string Audience { get; set; } = "FutureVet.Client";

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiracaoEmMinutos { get; set; } = 60;

    /// <summary>
    /// Valida a configuração na inicialização. Falhar aqui é melhor do que subir a API
    /// emitindo tokens assinados com uma chave fraca ou inexistente.
    /// </summary>
    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException(
                $"A chave de assinatura do JWT não foi configurada. Defina '{SectionName}:SigningKey' "
                + "via User Secrets ou a variável de ambiente 'Jwt__SigningKey'. "
                + "Ela não deve ser versionada no repositório.");
        }

        if (System.Text.Encoding.UTF8.GetByteCount(SigningKey) < TamanhoMinimoDaChaveEmBytes)
        {
            throw new InvalidOperationException(
                $"A chave de assinatura do JWT precisa de no mínimo {TamanhoMinimoDaChaveEmBytes} "
                + "bytes para HMAC-SHA256.");
        }

        if (ExpiracaoEmMinutos <= 0)
            throw new InvalidOperationException("'Jwt:ExpiracaoEmMinutos' deve ser maior que zero.");
    }
}
