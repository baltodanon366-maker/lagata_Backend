namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Tabla de relación entre Roles y Permisos (SQL Server)
/// </summary>
public class RolPermiso
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public int PermisoId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Rol Rol { get; set; } = null!;
    public virtual Permiso Permiso { get; set; } = null!;
}

