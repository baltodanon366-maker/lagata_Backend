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

    public string? Rol { get; set; }

    // Relaciones con otras entidades (se agregarán cuando se creen)
    // public virtual ICollection<Venta> Ventas { get; set; }
}


