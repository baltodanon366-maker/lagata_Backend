using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para solicitud de login
/// </summary>
/// <remarks>
/// Credenciales necesarias para autenticarse en el sistema.
/// 
/// **Ejemplo:**
/// ```json
/// {
///   "nombreUsuario": "admin",
///   "password": "tu-contraseña"
/// }
/// ```
/// </remarks>
public class LoginRequestDto
{
    /// <summary>
    /// Nombre de usuario del sistema
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario (se envía en texto plano, se valida con hash BCrypt en el servidor)
    /// </summary>
    /// <example>MiPassword123</example>
    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}


