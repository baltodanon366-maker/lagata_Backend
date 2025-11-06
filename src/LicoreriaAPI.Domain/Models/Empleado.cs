using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicoreriaAPI.Domain.Models;

/// <summary>
/// Entidad de empleado (SQL Server)
/// </summary>
public class Empleado : BaseEntity
{
    public int? UsuarioId { get; set; } // Relación opcional con Usuarios

    [Required]
    [MaxLength(20)]
    public string CodigoEmpleado { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NombreCompleto { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Direccion { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaNacimiento { get; set; }

    [Required]
    [Column(TypeName = "date")]
    public DateTime FechaIngreso { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salario { get; set; }

    [MaxLength(100)]
    public string? Departamento { get; set; }

    [MaxLength(100)]
    public string? Puesto { get; set; }

    // Relaciones
    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}

