using Microsoft.AspNetCore.Mvc;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Auth;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para autenticación y seguridad (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🔐 Autenticación - SQL Server")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar sesión en el sistema
    /// </summary>
    /// <param name="loginRequest">Credenciales de acceso</param>
    /// <returns>Token JWT y información del usuario</returns>
    /// <response code="200">Login exitoso</response>
    /// <response code="401">Credenciales inválidas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            var result = await _authService.LoginAsync(loginRequest);

            if (result == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar hacer login");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}


