using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de proveedor (SQL Server)
/// </summary>
public class Proveedor : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string CodigoProveedor { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

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
    public virtual ICollection<Compra> Compras { get; set; } = new List<Compra>();
}

