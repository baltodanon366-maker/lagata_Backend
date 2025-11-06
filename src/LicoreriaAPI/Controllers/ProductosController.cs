using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Productos (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductosController> _logger;

    public ProductosController(IProductoService productoService, ILogger<ProductosController> logger)
    {
        _productoService = productoService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo producto
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _productoService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un producto con ese nombre" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un producto existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarProductoDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _productoService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(new { message = "Producto actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un producto
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _productoService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(new { message = "Producto activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un producto
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _productoService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(new { message = "Producto desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los productos activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var productos = await _productoService.MostrarActivosAsync(top);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un producto activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var producto = await _productoService.MostrarActivosPorIdAsync(id);

            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar productos activos por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var productos = await _productoService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los productos inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var productos = await _productoService.MostrarInactivosAsync(top);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar productos inactivos por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var productos = await _productoService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un producto inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(ProductoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var producto = await _productoService.MostrarInactivosPorIdAsync(id);

            if (producto == null)
                return NotFound(new { message = "Producto inactivo no encontrado" });

            return Ok(producto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

