using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de rol para autorización (SQL Server)
/// </summary>
public class Rol : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    // Relaciones
    public virtual ICollection<UsuarioRol> UsuariosRoles { get; set; } = new List<UsuarioRol>();
    public virtual ICollection<RolPermiso> RolesPermisos { get; set; } = new List<RolPermiso>();
}

