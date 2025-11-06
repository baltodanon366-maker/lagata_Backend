using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para solicitud de registro de nuevo usuario
/// </summary>
public class RegistroRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MaxLength(100, ErrorMessage = "El nombre de usuario no puede exceder 100 caracteres")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(200, ErrorMessage = "El email no puede exceder 200 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder 200 caracteres")]
    public string? NombreCompleto { get; set; }

    [MaxLength(50, ErrorMessage = "El rol no puede exceder 50 caracteres")]
    public string Rol { get; set; } = "Vendedor";
}

