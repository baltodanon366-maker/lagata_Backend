using LicoreriaAPI.DTOs.MongoDB;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Notificaciones (MongoDB)
/// </summary>
public interface INotificationService
{
    Task<List<NotificationDto>> ListarAsync(int userId, bool soloNoLeidas = false, int top = 50);
    Task<NotificationDto?> ObtenerPorIdAsync(string id);
    Task<NotificationDto> CrearAsync(CrearNotificationDto crearDto);
    Task<bool> MarcarComoLeidaAsync(string id);
}

