using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de cliente (SQL Server)
/// </summary>
public class Cliente : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CodigoCliente { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? RazonSocial { get; set; }

    [MaxLength(20)]
    public string? RFC { get; set; }

    [MaxLength(500)]
    public string? Direccion { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    // Relaciones
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

