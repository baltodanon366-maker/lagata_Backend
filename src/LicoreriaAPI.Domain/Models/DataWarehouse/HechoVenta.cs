using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

/// <summary>
/// Tabla de hechos de ventas para Data Warehouse
/// </summary>
public class HechoVenta
{
    [Key]
    public int Id { get; set; }

    // Claves Foráneas (Dimensiones)
    [Required]
    public int FechaId { get; set; } // FK → DimTiempo

    [Required]
    public int ProductoId { get; set; } // FK → DimProducto

    public int? ClienteId { get; set; } // FK → DimCliente
    public int? EmpleadoId { get; set; } // FK → DimEmpleado
    public int? CategoriaId { get; set; } // FK → DimCategoria

    // Medidas (Métricas)
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalVentas { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CantidadVendida { get; set; }

    public int CantidadTransacciones { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PromedioTicket { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DescuentoTotal { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImpuestosTotal { get; set; } = 0;

    // Metadatos
    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
}

