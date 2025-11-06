using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.MongoDB;

/// <summary>
/// DTO para notificación
/// </summary>
public class NotificationDto
{
    public string Id { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Read { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// DTO para crear una notificación
/// </summary>
public class CrearNotificationDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El tipo de notificación es requerido")]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es requerido")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es requerido")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public Dictionary<string, object>? Data { get; set; }
}

