using LicoreriaAPI.DTOs.Auth;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de gestión de usuarios (solo admin)
/// </summary>
public interface IUsuarioService
{
    Task<List<UsuarioDto>> MostrarTodosAsync(int top = 100);
    Task<bool> AsignarRolAsync(int usuarioId, int rolId);
    Task<List<RolDto>> MostrarRolesAsync();
}

