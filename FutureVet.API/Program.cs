using System.Reflection;
using FutureVet.API.Auth;
using FutureVet.API.Errors;
using FutureVet.API.Extensions;
using FutureVet.API.Middleware;
using FutureVet.Application;
using FutureVet.Infrastructure;
using Microsoft.OpenApi;
using Serilog;

// Logger de bootstrap: cobre o intervalo entre o início do processo e a construção
// do host, garantindo que uma falha nessa janela ainda seja registrada.
// O logger completo (console + arquivo + enrichers) é o injetado via DI.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- Observabilidade: logging estruturado ----------
    builder.Host.AddSerilogLogging();

    // ---------- MVC / Swagger ----------
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger com documentação XML
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FutureVet API",
            Version = "v1",
            Description =
                """
                API para gerenciamento veterinário: usuários, pets, vacinas, consultas e o
                prontuário clínico dos pets.

                **Persistência:** usuários, pets, vacinas e consultas ficam no **Oracle**
                (EF Core); o prontuário (`/api/RegistroClinico`) fica no **MongoDB**.

                **Listagens:** o `GET` raiz de cada recurso aceita paginação (`pagina`,
                `tamanhoPagina`), ordenação (`ordenarPor`, `direcao=asc|desc`) e filtros
                próprios do recurso.

                **HATEOAS:** toda resposta de consulta traz `links` com as ações e os recursos
                relacionados (`self`, `update`, `delete`, `pet`, `vacinas`...) e, nas listagens,
                a navegação entre páginas (`first`, `prev`, `next`, `last`).

                **Como testar as operações protegidas (POST, PUT e DELETE):**

                1. Crie um usuário em `POST /api/Usuario` — este endpoint é público.
                2. Autentique-se em `POST /api/Auth/login` com o mesmo e-mail e senha e copie o `token` da resposta.
                3. Clique em **Authorize** (no topo à direita), cole apenas o token e confirme.

                As operações marcadas com 🔒 exigem esse passo. Todas as consultas (`GET`) são
                públicas e podem ser testadas sem autenticação.

                **Observabilidade:** `/health`, `/health/live`, `/health/ready` e `/metrics`
                (formato Prometheus) também são públicos.
                """,
            Contact = new OpenApiContact
            {
                Name = "FutureVet",
                Email = "contato@futureVet.com"
            }
        });

        // Comentários XML da API (controllers) e da Application (DTOs e filtros).
        foreach (var assembly in new[] { Assembly.GetExecutingAssembly(), typeof(FutureVet.Application.DependencyInjection).Assembly })
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        }

        options.AddJwtSecurityDefinition();

        // Os filtros são classes [FromQuery]; sem isto, o Swagger exibiria "Pagina" e
        // "OrdenarPor" em vez de "pagina" e "ordenarPor", como estão na documentação.
        options.DescribeAllParametersInCamelCase();
    });

    // ---------- Autenticacao ----------
    builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);

    // ---------- Tratamento global de erros ----------
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // ---------- Camadas (cada uma registra as próprias dependências) ----------
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ---------- HATEOAS ----------
    builder.Services.AddHateoas();

    // ---------- Observabilidade: health checks, tracing e métricas ----------
    builder.Services.AddApplicationHealthChecks(builder.Configuration);
    builder.Services.AddOpenTelemetryConfiguration(
        builder.Configuration,
        builder.Environment);

    var app = builder.Build();

    // O correlation ID precisa existir antes de qualquer log ou tratamento de erro.
    app.UseCorrelationId();
    app.UseRequestLogging();
    app.UseExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FutureVet API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });

    app.UseHttpsRedirection();

    app.UseMetricsEndpoint();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapApplicationHealthChecks();

    Log.Information(
        "Iniciando a FutureVet API no ambiente {Environment}.",
        app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "A FutureVet API foi encerrada de forma inesperada.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Exposta para que <c>WebApplicationFactory&lt;Program&gt;</c> consiga
/// inicializar a API nos testes de integração.
/// </summary>
public partial class Program;
