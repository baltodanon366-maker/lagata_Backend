using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Catalogos;

/// <summary>
/// DTO para respuesta de Empleado
/// </summary>
public class EmpleadoDto
{
    public int Id { get; set; }
    
    public int? UsuarioId { get; set; }
    
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
    
    public DateTime? FechaNacimiento { get; set; }
    
    [Required]
    public DateTime FechaIngreso { get; set; }
    
    public decimal? Salario { get; set; }
    
    [MaxLength(100)]
    public string? Departamento { get; set; }
    
    [MaxLength(100)]
    public string? Puesto { get; set; }
    
    public bool Activo { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Empleado
/// </summary>
public class CrearEmpleadoDto
{
    public int? UsuarioId { get; set; }
    
    [Required(ErrorMessage = "El código de empleado es requerido")]
    [MaxLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string CodigoEmpleado { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;
    
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
    
    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    public DateTime? FechaNacimiento { get; set; }
    
    [Required(ErrorMessage = "La fecha de ingreso es requerida")]
    public DateTime FechaIngreso { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un valor positivo")]
    public decimal? Salario { get; set; }
    
    [MaxLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Departamento { get; set; }
    
    [MaxLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
    public string? Puesto { get; set; }
}

/// <summary>
/// DTO para editar un Empleado existente
/// </summary>
public class EditarEmpleadoDto
{
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;
    
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
    
    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    public DateTime? FechaNacimiento { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser un valor positivo")]
    public decimal? Salario { get; set; }
    
    [MaxLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Departamento { get; set; }
    
    [MaxLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
    public string? Puesto { get; set; }
}

