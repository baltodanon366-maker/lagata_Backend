namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para respuesta de login exitoso
/// </summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? Rol { get; set; }
}


