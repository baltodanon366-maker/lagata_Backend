using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

public class DimProveedor
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProveedorCodigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProveedorNombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? RFC { get; set; }

    public bool Activo { get; set; } = true;
}

