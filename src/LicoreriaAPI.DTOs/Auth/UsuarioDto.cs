namespace LicoreriaAPI.DTOs.Auth;

/// <summary>
/// DTO para listar usuarios
/// </summary>
public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string? RolNombre { get; set; }
    public string? RolesAsignados { get; set; } // Roles separados por coma
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
}

/// <summary>
/// DTO para asignar rol a un usuario
/// </summary>
public class AsignarRolDto
{
    public int RolId { get; set; }
}

/// <summary>
/// DTO para respuesta de asignación de rol
/// </summary>
public class AsignarRolResponseDto
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// DTO para listar roles disponibles
/// </summary>
public class RolDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

