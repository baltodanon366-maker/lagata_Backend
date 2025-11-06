using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de permiso para autorización (SQL Server)
/// </summary>
public class Permiso : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(100)]
    public string? Modulo { get; set; } // Ventas, Compras, Inventario, etc.

    // Relaciones
    public virtual ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
}

