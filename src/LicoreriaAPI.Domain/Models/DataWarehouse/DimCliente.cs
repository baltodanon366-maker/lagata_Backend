using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

public class DimCliente
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ClienteCodigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ClienteNombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? RFC { get; set; }

    [MaxLength(50)]
    public string? TipoCliente { get; set; } // Mayorista, Minorista, etc.

    public bool Activo { get; set; } = true;
}

