using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

/// <summary>
/// Tabla de hechos de inventario para Data Warehouse
/// </summary>
public class HechoInventario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FechaId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    public int? CategoriaId { get; set; }

    // Medidas
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorInventario { get; set; } // Stock * PrecioCompra

    public int ProductosConStockBajo { get; set; } = 0; // 1 si Stock < StockMinimo

    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
}

