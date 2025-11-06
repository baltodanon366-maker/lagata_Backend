using Microsoft.EntityFrameworkCore;
using LicoreriaAPI.Domain.Models.DataWarehouse;

namespace LicoreriaAPI.Infrastructure.Data.DataWarehouse;

/// <summary>
/// Contexto de Entity Framework para Data Warehouse (Solo lectura)
/// </summary>
public class DataWarehouseContext : DbContext
{
    public DataWarehouseContext(DbContextOptions<DataWarehouseContext> options) : base(options)
    {
        // Data Warehouse es solo lectura
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    // Tablas de Hechos
    public DbSet<HechoVenta> HechoVentas { get; set; }
    public DbSet<HechoCompra> HechoCompras { get; set; }
    public DbSet<HechoInventario> HechoInventarios { get; set; }

    // Tablas de Dimensiones
    public DbSet<DimTiempo> DimTiempo { get; set; }
    public DbSet<DimProducto> DimProductos { get; set; }
    public DbSet<DimCliente> DimClientes { get; set; }
    public DbSet<DimProveedor> DimProveedores { get; set; }
    public DbSet<DimEmpleado> DimEmpleados { get; set; }
    public DbSet<DimCategoria> DimCategorias { get; set; }
    public DbSet<DimMarca> DimMarcas { get; set; }
    public DbSet<DimModelo> DimModelos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones específicas del Data Warehouse
        // Las relaciones se configuran aquí si es necesario
    }
}

