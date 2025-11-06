using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de devolución de venta - cabecera (SQL Server)
/// </summary>
public class DevolucionVenta : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Folio { get; set; } = string.Empty;

    [Required]
    public int VentaId { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    public DateTime FechaDevolucion { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Motivo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDevolucion { get; set; }

    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Completada, Rechazada

    [MaxLength(1000)]
    public string? Observaciones { get; set; }

    // Relaciones
    public virtual Venta Venta { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
    public virtual ICollection<DevolucionVentaDetalle> DevolucionesVentaDetalle { get; set; } = new List<DevolucionVentaDetalle>();
}

