using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de compra - detalle (SQL Server)
/// </summary>
public class CompraDetalle
{
    public int Id { get; set; }
    public int CompraId { get; set; }
    public int DetalleProductoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // Relaciones
    public virtual Compra Compra { get; set; } = null!;
    public virtual DetalleProducto DetalleProducto { get; set; } = null!;
}

