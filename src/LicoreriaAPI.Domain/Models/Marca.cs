using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de marca de productos (SQL Server)
/// </summary>
public class Marca : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    // Relaciones
    public virtual ICollection<DetalleProducto> DetallesProducto { get; set; } = new List<DetalleProducto>();
}

