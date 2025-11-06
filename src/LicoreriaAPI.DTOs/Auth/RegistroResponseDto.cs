namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para respuesta de registro exitoso
/// </summary>
public class RegistroResponseDto
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Rol { get; set; }
    public string Message { get; set; } = "Usuario registrado exitosamente";
}

