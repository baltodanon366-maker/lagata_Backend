using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de usuario para autenticación y seguridad (SQL Server)
/// </summary>
public class Usuario : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NombreCompleto { get; set; }

    public string? Rol { get; set; } // Mantener por compatibilidad, pero usar RolesPermisos

    public DateTime? UltimoAcceso { get; set; }

    // Relaciones
    public virtual ICollection<UsuarioRol> UsuariosRoles { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public virtual ICollection<DevolucionVenta> DevolucionesVenta { get; set; } = new List<DevolucionVenta>();
    public virtual ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
    public virtual Empleado? Empleado { get; set; }
}


