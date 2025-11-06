using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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

    /// <summary>
    /// Registrar nuevo usuario en el sistema
    /// </summary>
    /// <remarks>
    /// Permite crear una nueva cuenta de usuario en el sistema.
    /// 
    /// **Validaciones:**
    /// - El nombre de usuario debe ser único
    /// - El email debe ser único y tener formato válido
    /// - La contraseña debe tener al menos 6 caracteres
    /// - El rol es opcional (por defecto se asigna "Vendedor")
    /// 
    /// **Ejemplo de uso:**
    /// ```json
    /// {
    ///   "nombreUsuario": "juan.perez",
    ///   "email": "juan.perez@licoreria.com",
    ///   "password": "MiPassword123",
    ///   "nombreCompleto": "Juan Pérez",
    ///   "rol": "Vendedor"
    /// }
    /// ```
    /// 
    /// **Nota:** La contraseña se almacena con hash BCrypt para seguridad.
    /// </remarks>
    /// <param name="registroRequest">Datos del nuevo usuario (nombre, email, contraseña, etc.)</param>
    /// <returns>Información del usuario registrado (sin la contraseña)</returns>
    /// <response code="201">✅ Usuario registrado exitosamente. Retorna los datos del usuario creado.</response>
    /// <response code="400">❌ Datos inválidos o el nombre de usuario/email ya existe en el sistema.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(RegistroResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Registro([FromBody] RegistroRequestDto registroRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegistrarAsync(registroRequest);

            if (result == null)
            {
                return BadRequest(new { message = "El nombre de usuario o email ya existe" });
            }

            return CreatedAtAction(nameof(Registro), result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualizar contraseña del usuario autenticado
    /// </summary>
    /// <param name="actualizarPasswordDto">Contraseña actual y nueva contraseña</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="200">Contraseña actualizada exitosamente</response>
    /// <response code="400">Contraseña actual incorrecta</response>
    /// <response code="401">No autenticado</response>
    [HttpPut("cambiar-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ActualizarPassword([FromBody] ActualizarPasswordDto actualizarPasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener el ID del usuario desde el token JWT
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                // Si no hay NameIdentifier, intentar obtener el usuario desde el nombre
                var nombreUsuario = User.FindFirst(ClaimTypes.Name)?.Value;
                if (string.IsNullOrEmpty(nombreUsuario))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // Buscar el usuario por nombre
                var usuario = await _authService.GetUsuarioByNombreAsync(nombreUsuario);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }
                usuarioId = usuario.Id;
            }

            var resultado = await _authService.ActualizarPasswordAsync(usuarioId, actualizarPasswordDto);

            if (!resultado)
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }

            return Ok(new { message = "Contraseña actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar contraseña");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener información del usuario autenticado
    /// </summary>
    /// <returns>Información del usuario y permisos</returns>
    /// <response code="200">Información del usuario</response>
    /// <response code="401">No autenticado</response>
    [HttpGet("mi-informacion")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerMiInformacion()
    {
        try
        {
            var nombreUsuario = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var usuario = await _authService.GetUsuarioByNombreAsync(nombreUsuario);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var permisos = await _authService.ObtenerPermisosAsync(usuario.Id);

            var usuarioInfo = new UsuarioInfoDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                NombreCompleto = usuario.NombreCompleto,
                Rol = usuario.Rol,
                UltimoAcceso = usuario.UltimoAcceso,
                Permisos = permisos
            };

            return Ok(usuarioInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información del usuario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener permisos del usuario autenticado
    /// </summary>
    /// <returns>Lista de permisos</returns>
    /// <response code="200">Lista de permisos</response>
    /// <response code="401">No autenticado</response>
    [HttpGet("permisos")]
    [Authorize]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerPermisos()
    {
        try
        {
            var nombreUsuario = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }

            var usuario = await _authService.GetUsuarioByNombreAsync(nombreUsuario);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var permisos = await _authService.ObtenerPermisosAsync(usuario.Id);

            return Ok(permisos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener permisos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}


