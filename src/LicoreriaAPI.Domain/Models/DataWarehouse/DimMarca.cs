using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

public class DimMarca
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string MarcaNombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}

