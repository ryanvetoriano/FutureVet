using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutureVet.Infrastructure.Persistence.Configurations;

public class VacinaConfiguration
    : IEntityTypeConfiguration<Vacina>
{
    public void Configure(
        EntityTypeBuilder<Vacina> builder)
    {
        builder.ToTable(
            "TB_VACINA");

        builder.HasKey(
            x => x.Id);

        builder.Property(
                x => x.Id)
            .ValueGeneratedNever();

        builder.Property(
                x => x.NomeVacina)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(
                x => x.LocalAplicacao)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(
                x => x.DataAplicacao)
            .IsRequired();

        builder.Property(
                x => x.ProximaDose)
            .IsRequired();

        builder.Property(x => x.Disponivel)
            .HasColumnType("NUMBER(1)");

        builder.Property(
            x => x.DataCriacao);

        builder.Property(
            x => x.DataAtualizacao);
    }
}