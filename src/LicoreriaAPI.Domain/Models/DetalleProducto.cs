using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de detalle de producto - Con todas las relaciones y campos de negocio (SQL Server)
/// </summary>
public class DetalleProducto : BaseEntity
{
    [Required]
    public int ProductoId { get; set; }

    [Required]
    public int CategoriaId { get; set; }

    [Required]
    public int MarcaId { get; set; }

    [Required]
    public int ModeloId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SKU { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioCompra { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioVenta { get; set; }

    public int Stock { get; set; } = 0; // Cacheado, actualizado automáticamente por trigger desde MovimientosStock

    public int StockMinimo { get; set; } = 0; // Valor de referencia para alertas

    [MaxLength(50)]
    public string? UnidadMedida { get; set; } // Botella, Lata, Caja, etc.

    public DateTime? FechaUltimoMovimiento { get; set; } // Timestamp de última actualización de stock

    // Relaciones
    public virtual Producto Producto { get; set; } = null!;
    public virtual Categoria Categoria { get; set; } = null!;
    public virtual Marca Marca { get; set; } = null!;
    public virtual Modelo Modelo { get; set; } = null!;
    
    // Relaciones con transacciones
    public virtual ICollection<CompraDetalle> ComprasDetalle { get; set; } = new List<CompraDetalle>();
    public virtual ICollection<VentaDetalle> VentasDetalle { get; set; } = new List<VentaDetalle>();
    public virtual ICollection<DevolucionVentaDetalle> DevolucionesVentaDetalle { get; set; } = new List<DevolucionVentaDetalle>();
    public virtual ICollection<MovimientoStock> MovimientosStock { get; set; } = new List<MovimientoStock>();
}
