using LicoreriaAPI.DTOs.Auth;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de autenticación (SQL Server)
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
    Task<string> GenerateTokenAsync(string nombreUsuario, string rol);
}


