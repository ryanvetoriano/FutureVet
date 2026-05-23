using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutureVet.Infrastructure.Persistence.Configurations;

public class PetConfiguration
    : IEntityTypeConfiguration<Pet>
{
    public void Configure(
        EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable(
            "TB_PET");

        builder.HasKey(
            x => x.Id);

        builder.Property(
                x => x.Id)
            .ValueGeneratedNever();

        builder.Property(
                x => x.NomePet)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(
                x => x.Raca)
            .HasMaxLength(100);

        builder.Property(
                x => x.Idade)
            .IsRequired();

        builder.Property(
                x => x.Peso)
            .HasPrecision(
                10,
                2);

        builder.Property(
                x => x.Especie)
            .HasConversion<int>();

        builder.Property(
                x => x.Tamanho)
            .HasConversion<int>();

        builder.Property(
            x => x.Disponivel);

        builder.Property(
            x => x.DataCriacao);

        builder.Property(
            x => x.DataAtualizacao);

        builder.HasMany(
                x => x.Vacinas)
            .WithOne(
                x => x.Pet)
            .HasForeignKey(
                x => x.PetId)
            .OnDelete(
                DeleteBehavior.Cascade);

        builder.HasMany(
                x => x.Consultas)
            .WithOne(
                x => x.Pet)
            .HasForeignKey(
                x => x.PetId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}