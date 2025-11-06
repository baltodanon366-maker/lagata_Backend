using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Proveedores (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _proveedorService;
    private readonly ILogger<ProveedoresController> _logger;

    public ProveedoresController(IProveedorService proveedorService, ILogger<ProveedoresController> logger)
    {
        _proveedorService = proveedorService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo proveedor
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearProveedorDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _proveedorService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un proveedor con ese código" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un proveedor existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarProveedorDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _proveedorService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Proveedor no encontrado" });

            return Ok(new { message = "Proveedor actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar proveedor ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un proveedor
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _proveedorService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Proveedor no encontrado" });

            return Ok(new { message = "Proveedor activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar proveedor ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un proveedor
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _proveedorService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Proveedor no encontrado" });

            return Ok(new { message = "Proveedor desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar proveedor ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los proveedores activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var proveedores = await _proveedorService.MostrarActivosAsync(top);
            return Ok(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un proveedor activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var proveedor = await _proveedorService.MostrarActivosPorIdAsync(id);

            if (proveedor == null)
                return NotFound(new { message = "Proveedor no encontrado" });

            return Ok(proveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar proveedores activos por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var proveedores = await _proveedorService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedores por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los proveedores inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var proveedores = await _proveedorService.MostrarInactivosAsync(top);
            return Ok(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar proveedores inactivos por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var proveedores = await _proveedorService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(proveedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedores inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un proveedor inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(ProveedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var proveedor = await _proveedorService.MostrarInactivosPorIdAsync(id);

            if (proveedor == null)
                return NotFound(new { message = "Proveedor inactivo no encontrado" });

            return Ok(proveedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

