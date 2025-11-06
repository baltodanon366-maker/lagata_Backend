using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.MongoDB;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para operaciones con MongoDB (Notificaciones, Logs, Documentos)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🍃 Funcionalidades (MongoDB)")]
[Authorize]
public class MongoDBController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogService _logService;
    private readonly IDocumentService _documentService;
    private readonly ILogger<MongoDBController> _logger;

    public MongoDBController(
        INotificationService notificationService,
        ILogService logService,
        IDocumentService documentService,
        ILogger<MongoDBController> logger)
    {
        _notificationService = notificationService;
        _logService = logService;
        _documentService = documentService;
        _logger = logger;
    }

    // ============================================
    // NOTIFICACIONES
    // ============================================

    /// <summary>
    /// Listar notificaciones del usuario autenticado
    /// </summary>
    /// <remarks>
    /// Obtiene las notificaciones del usuario que está autenticado. Las notificaciones se filtran automáticamente por el usuario del token JWT.
    /// 
    /// **Tipos de notificaciones comunes:**
    /// - `StockBajo`: Alerta cuando un producto tiene stock bajo
    /// - `VentaCompletada`: Confirmación de venta realizada
    /// - `CompraRecibida`: Confirmación de compra recibida
    /// - `DevolucionProcesada`: Notificación de devolución procesada
    /// 
    /// **Parámetros:**
    /// - `soloNoLeidas`: Si es `true`, solo retorna notificaciones no leídas (por defecto: `false`)
    /// - `top`: Número máximo de notificaciones a retornar (por defecto: 50, máximo recomendado: 100)
    /// 
    /// **Ordenamiento:**
    /// Las notificaciones se ordenan por fecha de creación descendente (más recientes primero).
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/mongodb/notificaciones
    /// GET /api/mongodb/notificaciones?soloNoLeidas=true&top=20
    /// ```
    /// 
    /// **Respuesta incluye:**
    /// - `id`: ID único de la notificación (MongoDB ObjectId)
    /// - `type`: Tipo de notificación
    /// - `title`: Título de la notificación
    /// - `message`: Mensaje completo
    /// - `read`: Indica si ha sido leída
    /// - `createdAt`: Fecha de creación
    /// - `readAt`: Fecha en que fue marcada como leída (null si no está leída)
    /// - `data`: Datos adicionales flexibles (opcional)
    /// 
    /// **Uso recomendado:**
    /// - App móvil: Lista de notificaciones en pantalla de notificaciones
    /// - Dashboard: Badge con contador de notificaciones no leídas
    /// - Web: Panel de notificaciones en tiempo real
    /// </remarks>
    /// <param name="soloNoLeidas">Si es `true`, solo retorna notificaciones no leídas (por defecto: `false`)</param>
    /// <param name="top">Número máximo de notificaciones a retornar (por defecto: 50)</param>
    /// <returns>Lista de notificaciones del usuario autenticado</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de notificaciones del usuario.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("notificaciones")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListarNotificaciones([FromQuery] bool soloNoLeidas = false, [FromQuery] int top = 50)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var notificaciones = await _notificationService.ListarAsync(userId, soloNoLeidas, top);
            return Ok(notificaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar notificaciones");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una notificación por ID
    /// </summary>
    [HttpGet("notificaciones/{id}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerNotificacion(string id)
    {
        try
        {
            var notificacion = await _notificationService.ObtenerPorIdAsync(id);
            if (notificacion == null)
                return NotFound(new { message = "Notificación no encontrada" });

            return Ok(notificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificación ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear una nueva notificación
    /// </summary>
    [HttpPost("notificaciones")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearNotificacion([FromBody] CrearNotificationDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var notificacion = await _notificationService.CrearAsync(crearDto);
            return CreatedAtAction(nameof(ObtenerNotificacion), new { id = notificacion.Id }, notificacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Marcar una notificación como leída
    /// </summary>
    [HttpPatch("notificaciones/{id}/marcar-leida")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarNotificacionComoLeida(string id)
    {
        try
        {
            var resultado = await _notificationService.MarcarComoLeidaAsync(id);
            if (!resultado)
                return NotFound(new { message = "Notificación no encontrada" });

            return Ok(new { message = "Notificación marcada como leída" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación como leída ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // LOGS
    // ============================================

    /// <summary>
    /// Listar logs de usuario
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(List<UserLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarLogs([FromQuery] int? userId = null, [FromQuery] int top = 100)
    {
        try
        {
            var logs = await _logService.ListarUserLogsAsync(userId, top);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar logs");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear un log de usuario
    /// </summary>
    [HttpPost("logs")]
    [ProducesResponseType(typeof(UserLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearLog([FromBody] CrearUserLogDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var log = await _logService.CrearUserLogAsync(crearDto);
            return CreatedAtAction(nameof(ObtenerLog), new { id = log.Id }, log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear log");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un log por ID
    /// </summary>
    [HttpGet("logs/{id}")]
    [ProducesResponseType(typeof(UserLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerLog(string id)
    {
        try
        {
            var log = await _logService.ObtenerUserLogPorIdAsync(id);
            if (log == null)
                return NotFound(new { message = "Log no encontrado" });

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // DOCUMENTOS
    // ============================================

    /// <summary>
    /// Listar documentos
    /// </summary>
    [HttpGet("documentos")]
    [ProducesResponseType(typeof(List<DocumentMetadataDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDocumentos([FromQuery] string? entityType = null, [FromQuery] int? entityId = null, [FromQuery] int top = 100)
    {
        try
        {
            var documentos = await _documentService.ListarAsync(entityType, entityId, top);
            return Ok(documentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar documentos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un documento por ID
    /// </summary>
    [HttpGet("documentos/{id}")]
    [ProducesResponseType(typeof(DocumentMetadataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerDocumento(string id)
    {
        try
        {
            var documento = await _documentService.ObtenerPorIdAsync(id);
            if (documento == null)
                return NotFound(new { message = "Documento no encontrado" });

            return Ok(documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documento ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crear metadatos de documento
    /// </summary>
    [HttpPost("documentos")]
    [ProducesResponseType(typeof(DocumentMetadataDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CrearDocumento([FromBody] CrearDocumentMetadataDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var documento = await _documentService.CrearAsync(crearDto);
            return CreatedAtAction(nameof(ObtenerDocumento), new { id = documento.Id }, documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear documento");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}


