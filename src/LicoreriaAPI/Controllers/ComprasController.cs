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
}

