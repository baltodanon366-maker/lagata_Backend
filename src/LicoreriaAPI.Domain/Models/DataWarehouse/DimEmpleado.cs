using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.Domain.Models.DataWarehouse;

public class DimEmpleado
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string EmpleadoCodigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string EmpleadoNombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Departamento { get; set; }

    [MaxLength(100)]
    public string? Puesto { get; set; }

    public bool Activo { get; set; } = true;
}

