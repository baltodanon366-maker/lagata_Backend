using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Catalogos;

/// <summary>
/// DTO para respuesta de Proveedor
/// </summary>
public class ProveedorDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CodigoProveedor { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? RazonSocial { get; set; }
    
    [MaxLength(20)]
    public string? RFC { get; set; }
    
    [MaxLength(500)]
    public string? Direccion { get; set; }
    
    [MaxLength(20)]
    public string? Telefono { get; set; }
    
    [MaxLength(200)]
    public string? Email { get; set; }
    
    public bool Activo { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para crear un nuevo Proveedor
/// </summary>
public class CrearProveedorDto
{
    [Required(ErrorMessage = "El código de proveedor es requerido")]
    [MaxLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
    public string CodigoProveedor { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
    public string? RazonSocial { get; set; }
    
    [MaxLength(20, ErrorMessage = "El RFC no puede exceder 20 caracteres")]
    public string? RFC { get; set; }
    
    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
}

/// <summary>
/// DTO para editar un Proveedor existente
/// </summary>
public class EditarProveedorDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(200, ErrorMessage = "La razón social no puede exceder 200 caracteres")]
    public string? RazonSocial { get; set; }
    
    [MaxLength(20, ErrorMessage = "El RFC no puede exceder 20 caracteres")]
    public string? RFC { get; set; }
    
    [MaxLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
    public string? Direccion { get; set; }
    
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string? Telefono { get; set; }
    
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string? Email { get; set; }
}

