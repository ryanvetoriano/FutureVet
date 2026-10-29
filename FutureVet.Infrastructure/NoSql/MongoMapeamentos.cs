using FutureVet.Domain.Common;
using FutureVet.Domain.Entities;
using FutureVet.Domain.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace FutureVet.Infrastructure.NoSql;

/// <summary>
/// Mapeamento das entidades de domínio para documentos BSON, equivalente ao que as
/// <c>IEntityTypeConfiguration</c> fazem para o EF Core: o Domain continua sem nenhum
/// atributo ou dependência do driver.
/// </summary>
/// <remarks>
/// O registro de class maps e convenções no driver é global ao processo e só pode ocorrer uma
/// vez — o construtor estático garante isso mesmo quando vários hosts sobem no mesmo processo,
/// como nos testes de integração.
/// </remarks>
internal static class MongoMapeamentos
{
    static MongoMapeamentos()
    {
        var convencoes = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            // Enums gravados pelo nome ("Exame"), legíveis em consultas diretas no banco.
            new EnumRepresentationConvention(BsonType.String),
            // Campos desconhecidos são ignorados: documentos importados por outras ferramentas
            // ou gravados por versões futuras não quebram a leitura.
            new IgnoreExtraElementsConvention(true)
        };

        ConventionRegistry.Register(
            "FutureVet",
            convencoes,
            tipo => tipo.Namespace?.StartsWith("FutureVet.Domain", StringComparison.Ordinal) == true);

        // UUID padrão (subtipo 4), o formato que as demais ferramentas do MongoDB entendem.
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonClassMap.TryRegisterClassMap<BaseEntity>(mapa =>
        {
            mapa.AutoMap();
            mapa.MapIdMember(x => x.Id)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
        });

        BsonClassMap.TryRegisterClassMap<RegistroClinico>(mapa =>
        {
            mapa.AutoMap();
            UsarConstrutorPrivado(mapa);

            mapa.MapMember(x => x.PetId)
                .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // As listas são expostas como IReadOnlyList no domínio; aqui elas viram arrays
            // simples no documento, sem metadados de tipo.
            mapa.MapMember(x => x.Sintomas)
                .SetSerializer(new ImpliedImplementationInterfaceSerializer<IReadOnlyList<string>, List<string>>());
            mapa.MapMember(x => x.Medicamentos)
                .SetSerializer(new ImpliedImplementationInterfaceSerializer<IReadOnlyList<Medicamento>, List<Medicamento>>());
            mapa.MapMember(x => x.Tags)
                .SetSerializer(new ImpliedImplementationInterfaceSerializer<IReadOnlyList<string>, List<string>>());
        });

        BsonClassMap.TryRegisterClassMap<Medicamento>(mapa =>
        {
            mapa.AutoMap();
            UsarConstrutorPrivado(mapa);
        });
    }

    /// <summary>
    /// Reidrata o documento pelo construtor privado sem parâmetros, como o EF Core faz, e
    /// preenche as propriedades pelos setters privados.
    /// </summary>
    /// <remarks>
    /// Como as entidades só têm setters privados, a convenção padrão do driver as trata como
    /// imutáveis e tenta usar o construtor público. Isso reaplicaria as validações de criação
    /// a dados já persistidos e geraria um novo ID; por isso os construtores públicos são
    /// retirados do mapeamento.
    /// </remarks>
    private static void UsarConstrutorPrivado<T>(BsonClassMap<T> mapa)
    {
        foreach (var construtor in typeof(T).GetConstructors())
            mapa.UnmapConstructor(construtor);

        mapa.SetCreator(() => Activator.CreateInstance(typeof(T), nonPublic: true)!);
    }

    /// <summary>Garante que o mapeamento foi aplicado. Chamadas repetidas não têm efeito.</summary>
    public static void Registrar()
    {
        // O trabalho acontece no construtor estático, executado uma única vez pelo runtime.
    }
}
