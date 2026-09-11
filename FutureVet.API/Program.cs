using System.Reflection;
using FutureVet.API.Auth;
using FutureVet.API.Errors;
using FutureVet.API.Extensions;
using FutureVet.API.Middleware;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Services;
using FutureVet.Infrastructure.Persistence;
using FutureVet.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
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
            Description = "API para gerenciamento veterinário: usuários, pets, vacinas e consultas. "
                          + "Endpoints de observabilidade: /health, /health/live, /health/ready e /metrics.",
            Contact = new OpenApiContact
            {
                Name = "FutureVet",
                Email = "contato@futureVet.com"
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);

        options.AddJwtSecurityDefinition();
    });

    // ---------- Autenticacao ----------
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // ---------- Tratamento global de erros ----------
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // ---------- DbContext Oracle ----------
    builder.Services.AddDbContext<FutureVetContext>(options =>
        options.UseOracle(
            builder.Configuration.GetConnectionString("OracleConnection")));

    // ---------- Repositories ----------
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IPetRepository, PetRepository>();
    builder.Services.AddScoped<IVacinaRepository, VacinaRepository>();
    builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();

    // ---------- Services ----------
    builder.Services.AddScoped<IUsuarioService, UsuarioService>();
    builder.Services.AddScoped<IPetService, PetService>();
    builder.Services.AddScoped<IVacinaService, VacinaService>();
    builder.Services.AddScoped<IConsultaService, ConsultaService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

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
