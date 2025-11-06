using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Devoluciones de Venta (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🛒 Transacciones - SQL Server")]
[Authorize]
public class DevolucionesVentaController : ControllerBase
{
    private readonly IDevolucionVentaService _devolucionVentaService;
    private readonly ILogger<DevolucionesVentaController> _logger;

    public DevolucionesVentaController(IDevolucionVentaService devolucionVentaService, ILogger<DevolucionesVentaController> logger)
    {
        _devolucionVentaService = devolucionVentaService;
        _logger = logger;
    }

    /// <summary>
    /// Crear una nueva devolución de venta
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DevolucionVentaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearDevolucionVentaDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var (devolucionVentaId, errorMessage) = await _devolucionVentaService.CrearAsync(crearDto, usuarioId);

            if (devolucionVentaId == -1)
                return BadRequest(new { message = errorMessage ?? "Error al crear la devolución" });

            var devolucion = await _devolucionVentaService.ObtenerPorIdAsync(devolucionVentaId);

            if (devolucion == null)
                return BadRequest(new { message = "La devolución se creó pero no se pudo recuperar" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = devolucionVentaId }, devolucion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear devolución de venta");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una devolución de venta por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DevolucionVentaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var devolucion = await _devolucionVentaService.ObtenerPorIdAsync(id);

            if (devolucion == null)
                return NotFound(new { message = "Devolución no encontrada" });

            return Ok(devolucion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener devolución de venta ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener devoluciones de venta por rango de fechas
    /// </summary>
    [HttpGet("rango-fechas")]
    [ProducesResponseType(typeof(List<DevolucionVentaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MostrarPorRangoFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int top = 100)
    {
        try
        {
            var devoluciones = await _devolucionVentaService.MostrarPorRangoFechasAsync(fechaInicio, fechaFin, top);
            return Ok(devoluciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener devoluciones por rango de fechas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

