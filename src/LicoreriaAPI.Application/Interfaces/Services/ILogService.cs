using LicoreriaAPI.DTOs.MongoDB;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Logs (MongoDB)
/// </summary>
public interface ILogService
{
    Task<List<UserLogDto>> ListarUserLogsAsync(int? userId = null, int top = 100);
    Task<UserLogDto?> ObtenerUserLogPorIdAsync(string id);
    Task<UserLogDto> CrearUserLogAsync(CrearUserLogDto crearDto);
    Task<List<SystemLogDto>> ListarSystemLogsAsync(string? level = null, int top = 100);
    Task<SystemLogDto?> ObtenerSystemLogPorIdAsync(string id);
    Task<SystemLogDto> CrearSystemLogAsync(CrearSystemLogDto crearDto);
}

