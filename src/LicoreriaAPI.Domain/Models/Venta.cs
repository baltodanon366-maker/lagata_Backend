using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de venta - cabecera (SQL Server)
/// </summary>
public class Venta : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;

    public int? ClienteId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    public int? EmpleadoId { get; set; }

    [Required]
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Impuestos { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(50)]
    public string? MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

    [MaxLength(50)]
    public string Estado { get; set; } = "Completada"; // Completada, Cancelada, Pendiente

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Relaciones
    public virtual Cliente? Cliente { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual Empleado? Empleado { get; set; }
    public virtual ICollection<VentaDetalle> VentasDetalle { get; set; } = new List<VentaDetalle>();
    public virtual ICollection<DevolucionVenta> DevolucionesVenta { get; set; } = new List<DevolucionVenta>();
}

