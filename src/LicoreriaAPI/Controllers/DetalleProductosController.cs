using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de DetalleProducto (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class DetalleProductosController : ControllerBase
{
    private readonly IDetalleProductoService _detalleProductoService;
    private readonly ILogger<DetalleProductosController> _logger;

    public DetalleProductosController(IDetalleProductoService detalleProductoService, ILogger<DetalleProductosController> logger)
    {
        _detalleProductoService = detalleProductoService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo detalle de producto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DetalleProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearDetalleProductoDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _detalleProductoService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un detalle producto con ese código" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un detalle de producto existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarDetalleProductoDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _detalleProductoService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Detalle producto no encontrado" });

            return Ok(new { message = "Detalle producto actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar detalle producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un detalle de producto
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _detalleProductoService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Detalle producto no encontrado" });

            return Ok(new { message = "Detalle producto activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar detalle producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un detalle de producto
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _detalleProductoService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Detalle producto no encontrado" });

            return Ok(new { message = "Detalle producto desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar detalle producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los detalles de producto activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DetalleProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var detallesProducto = await _detalleProductoService.MostrarActivosAsync(top);
            return Ok(detallesProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles producto activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un detalle de producto activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DetalleProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var detalleProducto = await _detalleProductoService.MostrarActivosPorIdAsync(id);

            if (detalleProducto == null)
                return NotFound(new { message = "Detalle producto no encontrado" });

            return Ok(detalleProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar detalles de producto activos por nombre del producto
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<DetalleProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var detallesProducto = await _detalleProductoService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(detallesProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar detalles producto por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los detalles de producto inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<DetalleProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var detallesProducto = await _detalleProductoService.MostrarInactivosAsync(top);
            return Ok(detallesProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles producto inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar detalles de producto inactivos por nombre del producto
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<DetalleProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var detallesProducto = await _detalleProductoService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(detallesProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar detalles producto inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un detalle de producto inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(DetalleProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var detalleProducto = await _detalleProductoService.MostrarInactivosPorIdAsync(id);

            if (detalleProducto == null)
                return NotFound(new { message = "Detalle producto inactivo no encontrado" });

            return Ok(detalleProducto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle producto inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

