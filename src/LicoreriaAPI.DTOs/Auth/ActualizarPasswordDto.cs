using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para actualizar contraseña
/// </summary>
public class ActualizarPasswordDto
{
    [Required(ErrorMessage = "La contraseña actual es requerida")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres")]
    public string PasswordNuevo { get; set; } = string.Empty;
}

