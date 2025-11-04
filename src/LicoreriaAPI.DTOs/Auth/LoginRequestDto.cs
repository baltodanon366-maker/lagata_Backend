using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para solicitud de login
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}


