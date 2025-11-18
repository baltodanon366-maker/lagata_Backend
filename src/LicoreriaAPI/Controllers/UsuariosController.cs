using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Auth;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de usuarios (solo Administrador)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("👥 Usuarios - SQL Server")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los usuarios del sistema
    /// </summary>
    /// <remarks>
    /// Lista todos los usuarios registrados en el sistema con sus roles asignados.
    /// 
    /// **Solo Administrador:**
    /// Este endpoint solo puede ser accedido por usuarios con rol "Administrador".
    /// 
    /// **Parámetros:**
    /// - `top`: Número máximo de resultados (por defecto: 100, máximo recomendado: 500)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/usuarios?top=50
    /// ```
    /// 
    /// **Información incluida:**
    /// - Datos básicos del usuario (nombre, email, nombre completo)
    /// - Rol principal y roles asignados
    /// - Estado activo/inactivo
    /// - Fechas de creación, modificación y último acceso
    /// 
    /// **Ordenamiento:**
    /// Los usuarios se retornan ordenados por fecha de creación descendente (más recientes primero).
    /// </remarks>
    /// <param name="top">Número máximo de resultados (por defecto: 100)</param>
    /// <returns>Lista de usuarios con sus roles asignados</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de usuarios.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="403">❌ No autorizado. Solo usuarios con rol "Administrador" pueden acceder.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MostrarTodos([FromQuery] int top = 100)
    {
        try
        {
            var usuarios = await _usuarioService.MostrarTodosAsync(top);
            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Asignar un rol a un usuario
    /// </summary>
    /// <remarks>
    /// Asigna un rol específico a un usuario. Solo puede ser ejecutado por un Administrador.
    /// 
    /// **Solo Administrador:**
    /// Este endpoint solo puede ser accedido por usuarios con rol "Administrador".
    /// 
    /// **Proceso:**
    /// 1. Verifica que el usuario y el rol existan
    /// 2. Elimina los roles anteriores del usuario
    /// 3. Asigna el nuevo rol
    /// 4. Actualiza el campo `Rol` en la tabla `Usuarios` (por compatibilidad)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// POST /api/usuarios/5/asignar-rol
    /// Body: { "rolId": 2 }
    /// ```
    /// 
    /// **Nota:** Si el usuario ya tiene el rol asignado, la operación se completa sin error.
    /// </remarks>
    /// <param name="id">ID del usuario al que se le asignará el rol</param>
    /// <param name="asignarRolDto">DTO con el ID del rol a asignar</param>
    /// <returns>Resultado de la operación</returns>
    /// <response code="200">✅ Rol asignado exitosamente.</response>
    /// <response code="400">❌ El usuario o el rol no existen, o datos inválidos.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="403">❌ No autorizado. Solo usuarios con rol "Administrador" pueden acceder.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpPost("{id}/asignar-rol")]
    [ProducesResponseType(typeof(AsignarRolResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AsignarRol(int id, [FromBody] AsignarRolDto asignarRolDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _usuarioService.AsignarRolAsync(id, asignarRolDto.RolId);

            if (!resultado)
            {
                return BadRequest(new AsignarRolResponseDto
                {
                    Exitoso = false,
                    Mensaje = "No se pudo asignar el rol. Verifica que el usuario y el rol existan y estén activos."
                });
            }

            return Ok(new AsignarRolResponseDto
            {
                Exitoso = true,
                Mensaje = "Rol asignado exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar rol al usuario {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los roles disponibles
    /// </summary>
    /// <remarks>
    /// Lista todos los roles activos disponibles en el sistema.
    /// Útil para mostrar opciones al asignar roles a usuarios.
    /// 
    /// **Solo Administrador:**
    /// Este endpoint solo puede ser accedido por usuarios con rol "Administrador".
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/usuarios/roles
    /// ```
    /// </remarks>
    /// <returns>Lista de roles activos</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de roles.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="403">❌ No autorizado. Solo usuarios con rol "Administrador" pueden acceder.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(List<RolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MostrarRoles()
    {
        try
        {
            var roles = await _usuarioService.MostrarRolesAsync();
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

