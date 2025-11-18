using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Compras (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🛒 Transacciones - SQL Server")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;
    private readonly ILogger<ComprasController> _logger;

    public ComprasController(ICompraService compraService, ILogger<ComprasController> logger)
    {
        _compraService = compraService;
        _logger = logger;
    }

    /// <summary>
    /// Crear una nueva compra
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CompraDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearCompraDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var (compraId, errorMessage) = await _compraService.CrearAsync(crearDto, usuarioId);

            if (compraId == -1)
                return BadRequest(new { message = errorMessage ?? "Error al crear la compra" });

            var compra = await _compraService.ObtenerPorIdAsync(compraId);

            if (compra == null)
                return BadRequest(new { message = "La compra se creó pero no se pudo recuperar" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = compraId }, compra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear compra");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una compra por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompraDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var compra = await _compraService.ObtenerPorIdAsync(id);

            if (compra == null)
                return NotFound(new { message = "Compra no encontrada" });

            return Ok(compra);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compra ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener compras por rango de fechas
    /// </summary>
    [HttpGet("rango-fechas")]
    [ProducesResponseType(typeof(List<CompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MostrarPorRangoFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int top = 100)
    {
        try
        {
            var compras = await _compraService.MostrarPorRangoFechasAsync(fechaInicio, fechaFin, top);
            return Ok(compras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todas las compras activas (completadas)
    /// </summary>
    /// <remarks>
    /// Obtiene todas las compras con estado "Completada" sin necesidad de filtrar por fechas.
    /// Útil para ver todas las compras recientes del sistema.
    /// 
    /// **Parámetros:**
    /// - `top`: Número máximo de resultados (por defecto: 100, máximo recomendado: 500)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/compras/activas?top=50
    /// ```
    /// 
    /// **Ordenamiento:**
    /// Las compras se retornan ordenadas por fecha descendente (más recientes primero).
    /// 
    /// **Incluye:**
    /// - Información del proveedor
    /// - Información del usuario que realizó la compra
    /// - Detalles completos de productos comprados
    /// - Totales y subtotales calculados
    /// </remarks>
    /// <param name="top">Número máximo de resultados (por defecto: 100)</param>
    /// <returns>Lista de compras activas con todos sus detalles</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de compras activas.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("activas")]
    [ProducesResponseType(typeof(List<CompraDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MostrarActivas([FromQuery] int top = 100)
    {
        try
        {
            var compras = await _compraService.MostrarActivasAsync(top);
            return Ok(compras);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras activas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

