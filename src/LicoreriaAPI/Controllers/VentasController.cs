using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Transacciones;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para gestión de Ventas (SQL Server)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("🛒 Transacciones - SQL Server")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaService _ventaService;
    private readonly ILogger<VentasController> _logger;

    public VentasController(IVentaService ventaService, ILogger<VentasController> logger)
    {
        _ventaService = ventaService;
        _logger = logger;
    }

    /// <summary>
    /// Crear una nueva venta
    /// </summary>
    /// <remarks>
    /// Permite registrar una nueva venta en el sistema. Este endpoint realiza varias operaciones automáticamente:
    /// 
    /// **Proceso automático:**
    /// 1. ✅ Valida que haya stock suficiente para todos los productos
    /// 2. ✅ Calcula subtotal, impuestos (15%) y total
    /// 3. ✅ Genera un folio único automáticamente (formato: VTA-YYYYMMDDHHMMSS-XXXX)
    /// 4. ✅ Crea los detalles de venta
    /// 5. ✅ Actualiza el stock de los productos (reduce cantidad)
    /// 6. ✅ Registra movimientos de stock automáticamente
    /// 
    /// **Validaciones:**
    /// - Debe haber al menos un detalle en la venta
    /// - El stock debe ser suficiente para cada producto
    /// - Las cantidades deben ser mayores a 0
    /// - Los precios deben ser positivos
    /// 
    /// **Ejemplo de solicitud:**
    /// ```json
    /// {
    ///   "folio": "VTA-20250115-0001",
    ///   "clienteId": 1,
    ///   "empleadoId": 2,
    ///   "metodoPago": "Efectivo",
    ///   "observaciones": "Venta en mostrador",
    ///   "detalles": [
    ///     {
    ///       "detalleProductoId": 1,
    ///       "cantidad": 5,
    ///       "precioUnitario": 150.00,
    ///       "descuento": 0
    ///     },
    ///     {
    ///       "detalleProductoId": 2,
    ///       "cantidad": 2,
    ///       "precioUnitario": 200.00,
    ///       "descuento": 10.00
    ///     }
    ///   ]
    /// }
    /// ```
    /// 
    /// **Nota:** El `usuarioId` se obtiene automáticamente del token JWT. No es necesario enviarlo.
    /// </remarks>
    /// <param name="crearDto">Datos de la venta incluyendo detalles de productos</param>
    /// <returns>Venta creada con todos sus detalles y totales calculados</returns>
    /// <response code="201">✅ Venta creada exitosamente. Retorna la venta completa con detalles.</response>
    /// <response code="400">❌ Error de validación: stock insuficiente, datos inválidos, o folio duplicado.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VentaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Crear([FromBody] CrearVentaDto crearDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var (ventaId, errorMessage) = await _ventaService.CrearAsync(crearDto, usuarioId);

            if (ventaId == -1)
                return BadRequest(new { message = errorMessage ?? "Error al crear la venta" });

            var venta = await _ventaService.ObtenerPorIdAsync(ventaId);

            if (venta == null)
                return BadRequest(new { message = "La venta se creó pero no se pudo recuperar" });

            return CreatedAtAction(nameof(ObtenerPorId), new { id = ventaId }, venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear venta");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener una venta por ID
    /// </summary>
    /// <remarks>
    /// Obtiene los detalles completos de una venta específica, incluyendo:
    /// - Información del encabezado (folio, cliente, empleado, totales)
    /// - Lista completa de detalles de productos vendidos
    /// - Información de cada producto (nombre, marca, categoría, precios)
    /// - Método de pago y observaciones
    /// 
    /// **Ejemplo de respuesta:**
    /// ```json
    /// {
    ///   "id": 1,
    ///   "folio": "VTA-20250115-0001",
    ///   "clienteId": 1,
    ///   "clienteNombre": "Juan Pérez",
    ///   "subtotal": 1150.00,
    ///   "impuestos": 172.50,
    ///   "descuento": 10.00,
    ///   "total": 1312.50,
    ///   "metodoPago": "Efectivo",
    ///   "detalles": [...]
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID único de la venta a consultar</param>
    /// <returns>Venta completa con todos sus detalles</returns>
    /// <response code="200">✅ Venta encontrada. Retorna datos completos.</response>
    /// <response code="404">❌ Venta no encontrada con el ID proporcionado.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VentaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        try
        {
            var venta = await _ventaService.ObtenerPorIdAsync(id);

            if (venta == null)
                return NotFound(new { message = "Venta no encontrada" });

            return Ok(venta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener venta ID: {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas por rango de fechas
    /// </summary>
    /// <remarks>
    /// Consulta ventas dentro de un rango de fechas específico. Útil para reportes y análisis de ventas.
    /// 
    /// **Parámetros:**
    /// - `fechaInicio`: Fecha de inicio del rango (formato: YYYY-MM-DD o YYYY-MM-DDTHH:mm:ss)
    /// - `fechaFin`: Fecha de fin del rango (formato: YYYY-MM-DD o YYYY-MM-DDTHH:mm:ss)
    /// - `top`: Número máximo de resultados (por defecto: 100, máximo recomendado: 500)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/ventas/rango-fechas?fechaInicio=2025-01-01&fechaFin=2025-01-31&top=50
    /// ```
    /// 
    /// **Ordenamiento:**
    /// Las ventas se retornan ordenadas por fecha descendente (más recientes primero).
    /// 
    /// **Nota:** Las fechas se interpretan en formato UTC. Si no se especifica hora, se usa 00:00:00.
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del rango (requerido)</param>
    /// <param name="fechaFin">Fecha de fin del rango (requerido)</param>
    /// <param name="top">Número máximo de resultados (por defecto: 100)</param>
    /// <returns>Lista de ventas en el rango de fechas especificado</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de ventas (puede estar vacía si no hay resultados).</response>
    /// <response code="400">❌ Parámetros inválidos (fechas mal formateadas o rango inválido).</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("rango-fechas")]
    [ProducesResponseType(typeof(List<VentaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MostrarPorRangoFechas([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin, [FromQuery] int top = 100)
    {
        try
        {
            var ventas = await _ventaService.MostrarPorRangoFechasAsync(fechaInicio, fechaFin, top);
            return Ok(ventas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por rango de fechas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

