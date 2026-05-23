using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FutureVet.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration
    : IEntityTypeConfiguration<Usuario>
{
    public void Configure(
        EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable(
            "TB_USUARIO");

        builder.HasKey(
            x => x.Id);

        builder.Property(
                x => x.Id)
            .ValueGeneratedNever();

        builder.Property(
                x => x.Nome)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(
                x => x.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(
                x => x.Email)
            .IsUnique();

        builder.Property(
                x => x.Cpf)
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(
                x => x.Cpf)
            .IsUnique();

        builder.Property(
                x => x.Telefone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(
                x => x.Disponivel)
            .IsRequired();

        builder.Property(
                x => x.DataCriacao)
            .IsRequired();

        builder.Property(
            x => x.DataAtualizacao);

        builder.HasMany(
                x => x.Pets)
            .WithOne(
                x => x.Usuario)
            .HasForeignKey(
                x => x.UsuarioId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}