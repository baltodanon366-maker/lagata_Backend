using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Clientes (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📦 Catálogos - SQL Server")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo cliente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody] CrearClienteDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _clienteService.CrearAsync(crearDto);

            if (resultado == null)
                return BadRequest(new { message = "Ya existe un cliente con ese código" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Editar un cliente existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Editar(int id, [FromBody] EditarClienteDto editarDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var resultado = await _clienteService.EditarAsync(id, editarDto);

            if (!resultado)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(new { message = "Cliente actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar cliente ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Activar un cliente
    /// </summary>
    [HttpPatch("{id}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(int id)
    {
        try
        {
            var resultado = await _clienteService.ActivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(new { message = "Cliente activado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar cliente ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Desactivar un cliente
    /// </summary>
    [HttpPatch("{id}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desactivar(int id)
    {
        try
        {
            var resultado = await _clienteService.DesactivarAsync(id);

            if (!resultado)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(new { message = "Cliente desactivado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar cliente ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los clientes activos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerActivos([FromQuery] int top = 100)
    {
        try
        {
            var clientes = await _clienteService.MostrarActivosAsync(top);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes activos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un cliente activo por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var cliente = await _clienteService.MostrarActivosPorIdAsync(id);

            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar clientes activos por nombre
    /// </summary>
    [HttpGet("buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarActivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var clientes = await _clienteService.MostrarActivosPorNombreAsync(nombre, top);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener todos los clientes inactivos
    /// </summary>
    [HttpGet("inactivos")]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerInactivos([FromQuery] int top = 100)
    {
        try
        {
            var clientes = await _clienteService.MostrarInactivosAsync(top);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes inactivos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Buscar clientes inactivos por nombre
    /// </summary>
    [HttpGet("inactivos/buscar/{nombre}")]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarInactivosPorNombre(string nombre, [FromQuery] int top = 100)
    {
        try
        {
            var clientes = await _clienteService.MostrarInactivosPorNombreAsync(nombre, top);
            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes inactivos por nombre: {Nombre}", nombre);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener un cliente inactivo por ID
    /// </summary>
    [HttpGet("inactivos/{id}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerInactivoPorId(int id)
    {
        try
        {
            var cliente = await _clienteService.MostrarInactivosPorIdAsync(id);

            if (cliente == null)
                return NotFound(new { message = "Cliente inactivo no encontrado" });

            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente inactivo ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

