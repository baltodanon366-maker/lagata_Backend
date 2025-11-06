using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.MongoDB;

/// <summary>
/// DTO para log de usuario
/// </summary>
public class UserLogDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? Details { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO para crear un log de usuario
/// </summary>
public class CrearUserLogDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "La acción es requerida")]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de entidad es requerido")]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// DTO para log del sistema
/// </summary>
public class SystemLogDto
{
    public string Id { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO para crear un log del sistema
/// </summary>
public class CrearSystemLogDto
{
    [Required(ErrorMessage = "El nivel es requerido")]
    [MaxLength(20)]
    public string Level { get; set; } = string.Empty; // Error, Warning, Info

    [Required(ErrorMessage = "El mensaje es requerido")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
}

