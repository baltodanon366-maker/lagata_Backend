using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

public class DimModelo
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ModeloNombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}

