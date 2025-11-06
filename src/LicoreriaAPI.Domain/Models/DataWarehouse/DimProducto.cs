using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

/// <summary>
/// Dimensión de producto para Data Warehouse
/// </summary>
public class DimProducto
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProductoCodigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductoNombre { get; set; } = string.Empty;

    public int? CategoriaId { get; set; }
    public int? MarcaId { get; set; }
    public int? ModeloId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioCompraPromedio { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioVentaPromedio { get; set; }

    [MaxLength(50)]
    public string? UnidadMedida { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; } // Para SCD Type 2
}

