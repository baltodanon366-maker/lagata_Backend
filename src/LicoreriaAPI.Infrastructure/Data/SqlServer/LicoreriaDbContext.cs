using Microsoft.EntityFrameworkCore;
using LicoreriaAPI.Domain.Models;

namespace LicoreriaAPI.Infrastructure.Data.SqlServer;

public class LicoreriaDbContext : DbContext
{
    public LicoreriaDbContext(DbContextOptions<LicoreriaDbContext> options) : base(options)
    {
    }

    // Seguridad y Autenticación
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Rol> Roles { get; set; }
    public DbSet<Permiso> Permisos { get; set; }
    public DbSet<UsuarioRol> UsuariosRoles { get; set; }
    public DbSet<RolPermiso> RolesPermisos { get; set; }

    // Catálogos
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Marca> Marcas { get; set; }
    public DbSet<Modelo> Modelos { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<DetalleProducto> DetallesProducto { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Proveedor> Proveedores { get; set; }
    public DbSet<Empleado> Empleados { get; set; }

    // Transacciones
    public DbSet<Compra> Compras { get; set; }
    public DbSet<CompraDetalle> ComprasDetalle { get; set; }
    public DbSet<Venta> Ventas { get; set; }
    public DbSet<VentaDetalle> VentasDetalle { get; set; }
    public DbSet<DevolucionVenta> DevolucionesVenta { get; set; }
    public DbSet<DevolucionVentaDetalle> DevolucionesVentaDetalle { get; set; }

    // Inventario
    public DbSet<MovimientoStock> MovimientosStock { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones de entidades se agregarán aquí
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicoreriaDbContext).Assembly);
    }
}


