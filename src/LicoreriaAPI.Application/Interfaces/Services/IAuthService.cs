using LicoreriaAPI.DTOs.Auth;
using LicoreriaAPI.Domain.Models;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de autenticación (SQL Server)
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
    Task<RegistroResponseDto?> RegistrarAsync(RegistroRequestDto registroRequest);
    Task<bool> ActualizarPasswordAsync(int usuarioId, ActualizarPasswordDto actualizarPasswordDto);
    Task<List<string>> ObtenerPermisosAsync(int usuarioId);
    Task<Usuario?> GetUsuarioByNombreAsync(string nombreUsuario);
    Task<string> GenerateTokenAsync(int usuarioId, string nombreUsuario, string rol);
    Task<string> GenerateTokenAsync(string nombreUsuario, string rol); // Sobrecarga para compatibilidad
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}


