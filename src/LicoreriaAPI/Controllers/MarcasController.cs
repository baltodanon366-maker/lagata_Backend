using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Marcas (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class MarcasController : ControllerBase
{
    private readonly IMarcaService _marcaService;
    private readonly ILogger<MarcasController> _logger;

    public MarcasController(IMarcaService marcaService, ILogger<MarcasController> logger)
    {
        _marcaService = marcaService;
        _logger = logger;
    }

    /// <summary>
    /// Crear una nueva marca
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MarcaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearMarcaDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _marcaService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe una marca con ese nombre" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear marca");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar una marca existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarMarcaDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _marcaService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Marca no encontrada" });

            return Ok(new { message = "Marca actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar marca ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar una marca
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _marcaService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Marca no encontrada" });

            return Ok(new { message = "Marca activada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar marca ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar una marca
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _marcaService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Marca no encontrada" });

            return Ok(new { message = "Marca desactivada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar marca ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todas las marcas activas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MarcaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var marcas = await _marcaService.MostrarActivosAsync(top);
            return Ok(marcas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marcas activas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una marca activa por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MarcaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var marca = await _marcaService.MostrarActivosPorIdAsync(id);

            if (marca == null)
                return NotFound(new { message = "Marca no encontrada" });

            return Ok(marca);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marca ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar marcas activas por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<MarcaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var marcas = await _marcaService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(marcas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar marcas por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todas las marcas inactivas
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<MarcaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var marcas = await _marcaService.MostrarInactivosAsync(top);
            return Ok(marcas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marcas inactivas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar marcas inactivas por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<MarcaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var marcas = await _marcaService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(marcas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar marcas inactivas por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una marca inactiva por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(MarcaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var marca = await _marcaService.MostrarInactivosPorIdAsync(id);

            if (marca == null)
                return NotFound(new { message = "Marca inactiva no encontrada" });

            return Ok(marca);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marca inactiva ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

