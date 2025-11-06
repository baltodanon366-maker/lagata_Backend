using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de venta - detalle (SQL Server)
/// </summary>
public class VentaDetalle
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int DetalleProductoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // Relaciones
    public virtual Venta Venta { get; set; } = null!;
    public virtual DetalleProducto DetalleProducto { get; set; } = null!;
    public virtual ICollection<DevolucionVentaDetalle> DevolucionesVentaDetalle { get; set; } = new List<DevolucionVentaDetalle>();
}

