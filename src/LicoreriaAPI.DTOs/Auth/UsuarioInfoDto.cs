namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para información del usuario autenticado
/// </summary>
public class UsuarioInfoDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? Rol { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public List<string> Permisos { get; set; } = new List<string>();
}

