using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Categorías (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
    {
        _categoriaService = categoriaService;
        _logger = logger;
    }

    /// <summary>
    /// Crear una nueva categoría
    /// </summary>
    /// <remarks>
    /// Permite registrar una nueva categoría en el sistema. Las categorías se usan para organizar productos.
    /// 
    /// **Validaciones:**
    /// - El nombre de la categoría debe ser único
    /// - El nombre es requerido y no puede exceder 100 caracteres
    /// - La descripción es opcional y puede tener hasta 500 caracteres
    /// 
    /// **Ejemplo de solicitud:**
    /// ```json
    /// {
    ///   "nombre": "Whisky",
    ///   "descripcion": "Bebidas alcohólicas destiladas de whisky"
    /// }
    /// ```
    /// 
    /// **Nota:** La categoría se crea como activa por defecto (`activo: true`).
    /// </remarks>
    /// <param name="crearDto">Datos de la nueva categoría (nombre y descripción opcional)</param>
    /// <returns>Categoría creada con su ID y datos</returns>
    /// <response code="201">✅ Categoría creada exitosamente. Retorna la categoría con su ID asignado.</response>
    /// <response code="400">❌ Error de validación: nombre requerido, nombre duplicado, o datos inválidos.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _categoriaService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe una categoría con ese nombre" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar una categoría existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarCategoriaDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _categoriaService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(new { message = "Categoría actualizada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar categoría ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar una categoría
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _categoriaService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(new { message = "Categoría activada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar categoría ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar una categoría
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _categoriaService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(new { message = "Categoría desactivada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar categoría ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todas las categorías activas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var categorias = await _categoriaService.MostrarActivosAsync(top);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías activas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una categoría activa por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var categoria = await _categoriaService.MostrarActivosPorIdAsync(id);

            if (categoria == null)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar categorías activas por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var categorias = await _categoriaService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar categorías por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todas las categorías inactivas
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var categorias = await _categoriaService.MostrarInactivosAsync(top);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías inactivas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar categorías inactivas por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<CategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var categorias = await _categoriaService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(categorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar categorías inactivas por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una categoría inactiva por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(CategoriaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var categoria = await _categoriaService.MostrarInactivosPorIdAsync(id);

            if (categoria == null)
                return NotFound(new { message = "Categoría inactiva no encontrada" });

            return Ok(categoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría inactiva ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

