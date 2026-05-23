using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutureVet.Infrastructure.Persistence.Configurations;

public class ConsultaConfiguration
    : IEntityTypeConfiguration<Consulta>
{
    public void Configure(
        EntityTypeBuilder<Consulta> builder)
    {
        builder.ToTable(
            "TB_CONSULTA");

        builder.HasKey(
            x => x.Id);

        builder.Property(
                x => x.Id)
            .ValueGeneratedNever();

        builder.Property(
                x => x.TipoConsulta)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(
                x => x.Local)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(
                x => x.Data)
            .IsRequired();

        builder.Property(x => x.Hora).HasMaxLength(5).IsRequired();

        builder.Property(
            x => x.Disponivel);

        builder.Property(
            x => x.DataCriacao);

        builder.Property(
            x => x.DataAtualizacao);
    }
}