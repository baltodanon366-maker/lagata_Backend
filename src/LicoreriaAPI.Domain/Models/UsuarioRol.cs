namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Tabla de relación entre Usuarios y Roles (SQL Server)
/// </summary>
public class UsuarioRol
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Rol Rol { get; set; } = null!;
}

