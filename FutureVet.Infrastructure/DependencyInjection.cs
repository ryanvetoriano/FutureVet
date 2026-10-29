using FutureVet.Application.Interfaces.Repositories;
using FutureVet.Infrastructure.NoSql;
using FutureVet.Infrastructure.NoSql.Repositories;
using FutureVet.Infrastructure.Persistence;
using FutureVet.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FutureVet.Infrastructure;

/// <summary>
/// Registro da camada de Infrastructure: persistência relacional (Oracle via EF Core) e
/// NoSQL (MongoDB), com os repositórios concretos que implementam as interfaces da
/// Application.
/// </summary>
public static class DependencyInjection
{
    public const string OracleConnectionStringName = "OracleConnection";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------- Oracle (EF Core) ----------
        services.AddDbContext<FutureVetContext>(options =>
            options.UseOracle(
                configuration.GetConnectionString(OracleConnectionStringName)));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IVacinaRepository, VacinaRepository>();
        services.AddScoped<IConsultaRepository, ConsultaRepository>();

        // ---------- MongoDB ----------
        services.AddMongoDb(configuration);

        return services;
    }

    private static void AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .Validate(
                o => !string.IsNullOrWhiteSpace(o.DatabaseName),
                "MongoDb:DatabaseName é obrigatório.")
            .Validate(
                o => o.ServerSelectionTimeoutSeconds is >= 1 and <= 60,
                "MongoDb:ServerSelectionTimeoutSeconds deve estar entre 1 e 60.")
            .ValidateOnStart();

        // O MongoClient não abre conexão ao ser criado: uma indisponibilidade do MongoDB não
        // impede a API de subir, apenas as operações que dependem dele.
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            var connectionString = configuration.GetConnectionString(MongoDbOptions.ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"A connection string '{MongoDbOptions.ConnectionStringName}' não foi configurada. " +
                    "Defina ConnectionStrings:MongoConnection no appsettings, em User Secrets ou " +
                    "na variável de ambiente ConnectionStrings__MongoConnection.");
            }

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(options.ServerSelectionTimeoutSeconds);
            settings.ApplicationName = "FutureVet.API";

            return new MongoClient(settings);
        });

        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IRegistroClinicoRepository, RegistroClinicoRepository>();
        services.AddHostedService<MongoIndexInitializer>();
    }
}
