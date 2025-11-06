using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de movimiento de stock (SQL Server)
/// </summary>
public class MovimientoStock
{
    [Key]
    public int Id { get; set; }
    public int DetalleProductoId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TipoMovimiento { get; set; } = string.Empty; // Entrada, Salida, Ajuste

    [Column(TypeName = "decimal(18,2)")]
    public decimal Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal StockAnterior { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal StockNuevo { get; set; }

    public int? ReferenciaId { get; set; } // ID de Compra, Venta, etc.

    [MaxLength(50)]
    public string? ReferenciaTipo { get; set; } // Compra, Venta, Devolucion, Ajuste

    [Required]
    public int UsuarioId { get; set; }

    [MaxLength(500)]
    public string? Motivo { get; set; }

    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;

    // Relaciones
    public virtual DetalleProducto DetalleProducto { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}

