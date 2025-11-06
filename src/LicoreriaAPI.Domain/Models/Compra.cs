using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de compra - cabecera (SQL Server)
/// </summary>
public class Compra : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;

    [Required]
    public int ProveedorId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuestos { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Completada, Cancelada

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Relaciones
    public virtual Proveedor Proveedor { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<CompraDetalle> ComprasDetalle { get; set; } = new List<CompraDetalle>();
}

