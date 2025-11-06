using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Catalogos;

/// <summary>
/// DTO para respuesta de Marca
/// </summary>
public class MarcaDto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Descripcion { get; set; }
    
    public bool Activo { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// DTO para crear una nueva Marca
/// </summary>
public class CrearMarcaDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
}

/// <summary>
/// DTO para editar una Marca existente
/// </summary>
public class EditarMarcaDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
}

