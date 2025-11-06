using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de devolución de venta - detalle (SQL Server)
/// </summary>
public class DevolucionVentaDetalle
{
    public int Id { get; set; }
    public int DevolucionVentaId { get; set; }
    public int VentaDetalleId { get; set; }
    public int DetalleProductoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CantidadDevolver { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // Relaciones
    public virtual DevolucionVenta DevolucionVenta { get; set; } = null!;
    public virtual VentaDetalle VentaDetalle { get; set; } = null!;
    public virtual DetalleProducto DetalleProducto { get; set; } = null!;
}

