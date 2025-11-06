using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

/// <summary>
/// Tabla de hechos de compras para Data Warehouse
/// </summary>
public class HechoCompra
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FechaId { get; set; }

    [Required]
    public int ProductoId { get; set; }

    [Required]
    public int ProveedorId { get; set; }

    public int? CategoriaId { get; set; }

    // Medidas
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCompras { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CantidadComprada { get; set; }

    public int CantidadTransacciones { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PromedioCompra { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ImpuestosTotal { get; set; } = 0;

    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
}

