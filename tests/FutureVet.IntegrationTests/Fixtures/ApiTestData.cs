using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using FutureVet.Application.DTOs.Consulta;
using FutureVet.Application.DTOs.Pet;
using FutureVet.Application.DTOs.Usuario;
using FutureVet.Application.DTOs.Vacina;
using FutureVet.Domain.Enums;
using FutureVet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FutureVet.IntegrationTests.Fixtures;

/// <summary>
/// Configuração JSON espelhando a da API.
/// </summary>
public static class ApiJson
{
    /// <summary>
    /// Enums trafegam como número (<c>EspeciePet.Cao</c> = 1), que é o contrato publicado
    /// no Swagger desde as sprints anteriores — por isso nenhum <c>JsonStringEnumConverter</c>.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Monta os cenários dos testes de integração através dos <b>endpoints reais</b> da API
/// (e não inserindo direto no banco), de modo que o fluxo HTTP completo seja exercitado.
/// </summary>
/// <remarks>
/// Como os testes rodam contra um banco Oracle compartilhado, esta classe registra tudo o que
/// cria e remove ao final da execução. Todo e-mail de teste usa o domínio
/// <see cref="DominioDeTeste"/>, que não existe de verdade: isso identifica sem ambiguidade os
/// registros de teste e permite varrer sobras de execuções anteriores que tenham sido
/// interrompidas.
/// </remarks>
public sealed class ApiTestData
{
    /// <summary>Domínio reservado aos registros criados pelos testes.</summary>
    public const string DominioDeTeste = "testes.futurevet.local";

    private readonly HttpClient _client;
    private readonly ConcurrentBag<Guid> _usuariosCriados = [];

    public ApiTestData(HttpClient client)
    {
        _client = client;
    }

    public async Task<UsuarioResponse> CriarUsuarioAsync(string? prefixoNome = null)
    {
        var sufixo = Sufixo();

        var request = new CreateUsuarioRequest(
            $"{prefixoNome ?? "Usuário"} {sufixo}",
            EmailDeTeste(sufixo),
            "SenhaSegura123",
            GerarCpf(),
            "11999998888");

        var response = await _client.PostAsJsonAsync("/api/Usuario", request, ApiJson.Options);
        response.EnsureSuccessStatusCode();

        var usuario = (await response.Content.ReadFromJsonAsync<UsuarioResponse>(ApiJson.Options))!;
        Registrar(usuario.Id);

        return usuario;
    }

    public async Task<PetResponse> CriarPetAsync(Guid? usuarioId = null)
    {
        var donoId = usuarioId ?? (await CriarUsuarioAsync()).Id;

        var request = new CreatePetRequest(
            $"Pet {Sufixo()}",
            EspeciePet.Cao,
            "Labrador",
            3,
            PortePet.Grande,
            32.5m,
            donoId);

        var response = await _client.PostAsJsonAsync("/api/Pet", request, ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PetResponse>(ApiJson.Options))!;
    }

    public async Task<VacinaResponse> CriarVacinaAsync(Guid? petId = null)
    {
        var pet = petId ?? (await CriarPetAsync()).Id;
        var aplicacao = new DateTime(2026, 1, 10);

        var request = new CreateVacinaRequest(
            "V10",
            aplicacao,
            aplicacao.AddYears(1),
            "Clínica Central",
            pet);

        var response = await _client.PostAsJsonAsync("/api/Vacina", request, ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<VacinaResponse>(ApiJson.Options))!;
    }

    public async Task<ConsultaResponse> CriarConsultaAsync(Guid? petId = null)
    {
        var pet = petId ?? (await CriarPetAsync()).Id;

        var request = new CreateConsultaRequest(
            "Rotina",
            new DateTime(2026, 3, 15),
            "14:30",
            "Clínica Central",
            pet);

        var response = await _client.PostAsJsonAsync("/api/Consulta", request, ApiJson.Options);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ConsultaResponse>(ApiJson.Options))!;
    }

    /// <summary>Registra um usuário criado fora dos helpers para que ele também seja limpo.</summary>
    public void Registrar(Guid usuarioId) => _usuariosCriados.Add(usuarioId);

    /// <summary>E-mail único no domínio reservado aos testes.</summary>
    public static string EmailDeTeste(string? sufixo = null)
        => $"teste.{sufixo ?? Sufixo()}@{DominioDeTeste}";

    /// <summary>CPF de 11 dígitos, apenas para satisfazer o índice único da tabela.</summary>
    public static string GerarCpf()
        => Random.Shared.NextInt64(10_000_000_000, 99_999_999_999).ToString();

    /// <summary>
    /// Remove do Oracle todos os registros de teste. Apagar o usuário é suficiente: as chaves
    /// estrangeiras foram criadas com <c>ON DELETE CASCADE</c>, então pets, vacinas e consultas
    /// vão junto.
    /// </summary>
    /// <remarks>
    /// A limpeza cobre tanto os IDs criados nesta execução quanto qualquer sobra no domínio de
    /// teste — o que devolve o schema ao estado original mesmo se uma execução anterior tiver
    /// sido interrompida.
    /// </remarks>
    public async Task LimparAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.CriarEscopo();
        var context = scope.ServiceProvider.GetRequiredService<FutureVetContext>();

        var idsCriados = _usuariosCriados.Distinct().ToArray();
        var sufixoDominio = $"@{DominioDeTeste}";

        var paraRemover = await context.Usuarios
            .Where(u => idsCriados.Contains(u.Id) || u.Email.EndsWith(sufixoDominio))
            .ToListAsync();

        if (paraRemover.Count == 0) return;

        context.Usuarios.RemoveRange(paraRemover);
        await context.SaveChangesAsync();
    }

    private static string Sufixo() => Guid.NewGuid().ToString("N")[..12];
}
