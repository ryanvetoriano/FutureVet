using System.Reflection;
using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Application.Interfaces.Services;
using FutureVet.Application.Services;
using FutureVet.Infrastructure.Persistence;
using FutureVet.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger com documentação XML
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FutureVet API",
        Version = "v1",
        Description = "API para gerenciamento veterinário: usuários, pets, vacinas e consultas.",
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
});

// DbContext Oracle
builder.Services.AddDbContext<FutureVetContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("OracleConnection")));

// Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IVacinaRepository, VacinaRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();

// Services
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVacinaService, VacinaService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FutureVet API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();