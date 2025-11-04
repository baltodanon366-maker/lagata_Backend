using Microsoft.EntityFrameworkCore;
using LicoreriaAPI.Domain.Models;

namespace LicoreriaAPI.Infrastructure.Data.SqlServer;

public class LicoreriaDbContext : DbContext
{
    public LicoreriaDbContext(DbContextOptions<LicoreriaDbContext> options) : base(options)
    {
    }

    // Aquí se agregarán los DbSet para las entidades de SQL Server
    // Ejemplo: public DbSet<Usuario> Usuarios { get; set; }
    // public DbSet<Categoria> Categorias { get; set; }
    // etc.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de entidades se agregarán aquí
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicoreriaDbContext).Assembly);
    }
}


