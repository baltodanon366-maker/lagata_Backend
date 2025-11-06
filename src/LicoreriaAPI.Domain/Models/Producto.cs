using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de producto - Información básica sin relaciones (SQL Server)
/// </summary>
public class Producto : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    // Relaciones solo con DetalleProducto
    public virtual ICollection<DetalleProducto> DetallesProducto { get; set; } = new List<DetalleProducto>();
}
