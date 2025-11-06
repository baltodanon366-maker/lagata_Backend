using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Modelos (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class ModelosController : ControllerBase
{
    private readonly IModeloService _modeloService;
    private readonly ILogger<ModelosController> _logger;

    public ModelosController(IModeloService modeloService, ILogger<ModelosController> logger)
    {
        _modeloService = modeloService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo modelo
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ModeloDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearModeloDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _modeloService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un modelo con ese nombre" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear modelo");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un modelo existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarModeloDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _modeloService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Modelo no encontrado" });

            return Ok(new { message = "Modelo actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar modelo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un modelo
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _modeloService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Modelo no encontrado" });

            return Ok(new { message = "Modelo activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar modelo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un modelo
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _modeloService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Modelo no encontrado" });

            return Ok(new { message = "Modelo desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar modelo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los modelos activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ModeloDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var modelos = await _modeloService.MostrarActivosAsync(top);
            return Ok(modelos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelos activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un modelo activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ModeloDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var modelo = await _modeloService.MostrarActivosPorIdAsync(id);

            if (modelo == null)
                return NotFound(new { message = "Modelo no encontrado" });

            return Ok(modelo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar modelos activos por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ModeloDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var modelos = await _modeloService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(modelos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar modelos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los modelos inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<ModeloDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var modelos = await _modeloService.MostrarInactivosAsync(top);
            return Ok(modelos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelos inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar modelos inactivos por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ModeloDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var modelos = await _modeloService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(modelos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar modelos inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un modelo inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(ModeloDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var modelo = await _modeloService.MostrarInactivosPorIdAsync(id);

            if (modelo == null)
                return NotFound(new { message = "Modelo inactivo no encontrado" });

            return Ok(modelo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelo inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

