using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Empleados (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _empleadoService;
    private readonly ILogger<EmpleadosController> _logger;

    public EmpleadosController(IEmpleadoService empleadoService, ILogger<EmpleadosController> logger)
    {
        _empleadoService = empleadoService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo empleado
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _empleadoService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un empleado con ese código" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empleado");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un empleado existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarEmpleadoDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _empleadoService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Empleado no encontrado" });

            return Ok(new { message = "Empleado actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar empleado ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un empleado
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _empleadoService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Empleado no encontrado" });

            return Ok(new { message = "Empleado activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar empleado ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un empleado
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _empleadoService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Empleado no encontrado" });

            return Ok(new { message = "Empleado desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar empleado ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los empleados activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var empleados = await _empleadoService.MostrarActivosAsync(top);
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un empleado activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var empleado = await _empleadoService.MostrarActivosPorIdAsync(id);

            if (empleado == null)
                return NotFound(new { message = "Empleado no encontrado" });

            return Ok(empleado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleado ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar empleados activos por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var empleados = await _empleadoService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empleados por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los empleados inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var empleados = await _empleadoService.MostrarInactivosAsync(top);
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar empleados inactivos por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var empleados = await _empleadoService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empleados inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un empleado inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var empleado = await _empleadoService.MostrarInactivosPorIdAsync(id);

            if (empleado == null)
                return NotFound(new { message = "Empleado inactivo no encontrado" });

            return Ok(empleado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleado inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

