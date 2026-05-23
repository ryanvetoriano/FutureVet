using FutureVet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FutureVet.Infrastructure.Persistence;

public class FutureVetContext : DbContext
{
    public DbSet<Usuario> Usuarios
        => Set<Usuario>();

    public DbSet<Pet> Pets
        => Set<Pet>();

    public DbSet<Vacina> Vacinas
        => Set<Vacina>();

    public DbSet<Consulta> Consultas
        => Set<Consulta>();


    public FutureVetContext(
        DbContextOptions<FutureVetContext>
            options)
        : base(options)
    {
    }


    protected override void
        OnModelCreating(
            ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfigurationsFromAssembly(
                typeof(
                        FutureVetContext)
                    .Assembly);

        base.OnModelCreating(
            modelBuilder);
    }
}