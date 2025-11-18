using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Analytics;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para Analytics y Dashboard (Data Warehouse)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("📊 Analytics (Data Warehouse)")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    // ============================================
    // ENDPOINTS DE VENTAS
    // ============================================

    /// <summary>
    /// Obtener ventas agregadas por rango de fechas
    /// </summary>
    /// <remarks>
    /// Retorna ventas agregadas según un período de tiempo específico. Ideal para gráficos de líneas o barras.
    /// 
    /// **Parámetros de agrupación:**
    /// - `agruparPor`: Determina cómo se agrupan los datos
    ///   - `"Dia"`: Agrupa por día (útil para gráficos diarios)
    ///   - `"Semana"`: Agrupa por semana (útil para análisis semanales)
    ///   - `"Mes"`: Agrupa por mes (útil para análisis mensuales)
    ///   - `"Año"`: Agrupa por año (útil para análisis anuales)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/analytics/ventas/rango-fechas?fechaInicio=2025-01-01&fechaFin=2025-01-31&agruparPor=Mes
    /// ```
    /// 
    /// **Respuesta incluye:**
    /// - Fecha o período de agrupación
    /// - Total de cantidad vendida
    /// - Total de ventas (monto)
    /// - Total de descuentos aplicados
    /// - Total de impuestos
    /// - Número de ventas realizadas
    /// - Promedio de ticket
    /// 
    /// **Uso recomendado:**
    /// - Dashboard: Gráfico de líneas mostrando tendencia de ventas
    /// - Reportes: Análisis de ventas por período
    /// - Mobile: Gráfico de barras con ventas mensuales
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del rango (requerido, formato: YYYY-MM-DD)</param>
    /// <param name="fechaFin">Fecha de fin del rango (requerido, formato: YYYY-MM-DD)</param>
    /// <param name="agruparPor">Tipo de agrupación: "Dia", "Semana", "Mes" o "Año" (por defecto: "Dia")</param>
    /// <returns>Lista de ventas agregadas según el período especificado</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna ventas agregadas por el período especificado.</response>
    /// <response code="400">❌ Parámetros inválidos (fechas mal formateadas o valor de agrupación no válido).</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("ventas/rango-fechas")]
    [ProducesResponseType(typeof(List<VentasPorRangoFechasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VentasPorRangoFechas(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] string agruparPor = "Dia")
    {
        try
        {
            var resultados = await _analyticsService.VentasPorRangoFechasAsync(fechaInicio, fechaFin, agruparPor);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por rango de fechas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas agregadas por producto
    /// </summary>
    /// <remarks>
    /// Retorna ventas agregadas por producto. Si se proporciona `productoId`, filtra por ese producto específico.
    /// Si no se proporciona, retorna todos los productos ordenados por total de ventas.
    /// </remarks>
    /// <param name="productoId">ID del producto específico (opcional). Si no se proporciona, retorna todos los productos.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    /// <param name="top">Número máximo de resultados (por defecto: 20)</param>
    [HttpGet("ventas/por-producto")]
    [ProducesResponseType(typeof(List<VentasPorProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorProducto(
        [FromQuery] int? productoId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 20)
    {
        try
        {
            var resultados = await _analyticsService.VentasPorProductoAsync(productoId, fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas agregadas por categoría
    /// </summary>
    /// <remarks>
    /// Retorna ventas agregadas por categoría. Si se proporciona `categoriaId`, filtra por esa categoría específica.
    /// Si no se proporciona, retorna todas las categorías ordenadas por total de ventas.
    /// </remarks>
    /// <param name="categoriaId">ID de la categoría específica (opcional). Si no se proporciona, retorna todas las categorías.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    [HttpGet("ventas/por-categoria")]
    [ProducesResponseType(typeof(List<VentasPorCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorCategoria(
        [FromQuery] int? categoriaId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var resultados = await _analyticsService.VentasPorCategoriaAsync(categoriaId, fechaInicio, fechaFin);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por categoría");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas agregadas por cliente
    /// </summary>
    /// <remarks>
    /// Retorna ventas agregadas por cliente. Si se proporciona `clienteId`, filtra por ese cliente específico.
    /// Si no se proporciona, retorna todos los clientes ordenados por total de ventas.
    /// </remarks>
    /// <param name="clienteId">ID del cliente específico (opcional). Si no se proporciona, retorna todos los clientes.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    /// <param name="top">Número máximo de resultados (por defecto: 20)</param>
    [HttpGet("ventas/por-cliente")]
    [ProducesResponseType(typeof(List<VentasPorClienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorCliente(
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 20)
    {
        try
        {
            var resultados = await _analyticsService.VentasPorClienteAsync(clienteId, fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por cliente");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas agregadas por empleado
    /// </summary>
    /// <remarks>
    /// Retorna ventas agregadas por empleado. Si se proporciona `empleadoId`, filtra por ese empleado específico.
    /// Si no se proporciona, retorna todos los empleados ordenados por total de ventas.
    /// </remarks>
    /// <param name="empleadoId">ID del empleado específico (opcional). Si no se proporciona, retorna todos los empleados.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    [HttpGet("ventas/por-empleado")]
    [ProducesResponseType(typeof(List<VentasPorEmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorEmpleado(
        [FromQuery] int? empleadoId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var resultados = await _analyticsService.VentasPorEmpleadoAsync(empleadoId, fechaInicio, fechaFin);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por empleado");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener ventas por método de pago
    /// </summary>
    [HttpGet("ventas/metodo-pago")]
    [ProducesResponseType(typeof(List<VentasPorMetodoPagoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VentasPorMetodoPago(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var resultados = await _analyticsService.VentasPorMetodoPagoAsync(fechaInicio, fechaFin);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por método de pago");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // ENDPOINTS DE COMPRAS
    // ============================================

    /// <summary>
    /// Obtener compras agregadas por rango de fechas
    /// </summary>
    [HttpGet("compras/rango-fechas")]
    [ProducesResponseType(typeof(List<ComprasPorRangoFechasDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComprasPorRangoFechas(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] string agruparPor = "Dia")
    {
        try
        {
            var resultados = await _analyticsService.ComprasPorRangoFechasAsync(fechaInicio, fechaFin, agruparPor);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener compras agregadas por proveedor
    /// </summary>
    /// <remarks>
    /// Retorna compras agregadas por proveedor. Si se proporciona `proveedorId`, filtra por ese proveedor específico.
    /// Si no se proporciona, retorna todos los proveedores ordenados por total de compras.
    /// </remarks>
    /// <param name="proveedorId">ID del proveedor específico (opcional). Si no se proporciona, retorna todos los proveedores.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    /// <param name="top">Número máximo de resultados (por defecto: 20)</param>
    [HttpGet("compras/por-proveedor")]
    [ProducesResponseType(typeof(List<ComprasPorProveedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComprasPorProveedor(
        [FromQuery] int? proveedorId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 20)
    {
        try
        {
            var resultados = await _analyticsService.ComprasPorProveedorAsync(proveedorId, fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por proveedor");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener compras agregadas por producto
    /// </summary>
    /// <remarks>
    /// Retorna compras agregadas por producto. Si se proporciona `productoId`, filtra por ese producto específico.
    /// Si no se proporciona, retorna todos los productos ordenados por total de compras.
    /// </remarks>
    /// <param name="productoId">ID del producto específico (opcional). Si no se proporciona, retorna todos los productos.</param>
    /// <param name="fechaInicio">Fecha de inicio del rango (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del rango (opcional)</param>
    /// <param name="top">Número máximo de resultados (por defecto: 20)</param>
    [HttpGet("compras/por-producto")]
    [ProducesResponseType(typeof(List<ComprasPorProductoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComprasPorProducto(
        [FromQuery] int? productoId = null,
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 20)
    {
        try
        {
            var resultados = await _analyticsService.ComprasPorProductoAsync(productoId, fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por producto");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // ENDPOINTS DE INVENTARIO
    // ============================================

    /// <summary>
    /// Obtener stock actual de productos
    /// </summary>
    [HttpGet("inventario/stock-actual")]
    [ProducesResponseType(typeof(List<StockActualDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> InventarioStockActual(
        [FromQuery] bool incluirInactivos = false,
        [FromQuery] int? top = null)
    {
        try
        {
            var resultados = await _analyticsService.InventarioStockActualAsync(incluirInactivos, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener stock actual");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener productos con stock bajo
    /// </summary>
    /// <remarks>
    /// Retorna una lista de productos que tienen stock igual o menor al stock mínimo configurado.
    /// **Este es un endpoint crítico para alertas de inventario.**
    /// 
    /// **Criterio de stock bajo:**
    /// Un producto se considera con stock bajo cuando: `StockActual <= StockMinimo`
    /// 
    /// **Ordenamiento:**
    /// Los productos se ordenan por cantidad faltante (mayor a menor), es decir, los que más necesitan reposición aparecen primero.
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/analytics/inventario/stock-bajo
    /// ```
    /// 
    /// **Respuesta incluye:**
    /// - Información del producto (nombre, código, categoría, marca)
    /// - `stockActual`: Cantidad actual en inventario
    /// - `stockMinimo`: Cantidad mínima configurada
    /// - `cantidadFaltante`: Diferencia entre mínimo y actual (cuánto falta para alcanzar el mínimo)
    /// - Precios de compra y venta
    /// - Fecha de última actualización
    /// 
    /// **Uso recomendado:**
    /// - Dashboard móvil: Notificación o alerta visual de productos con stock bajo
    /// - Reportes: Lista para generar órdenes de compra
    /// - Alertas: Integración con sistema de notificaciones para alertar automáticamente
    /// 
    /// **Nota:** Solo se incluyen productos activos. Los productos inactivos no aparecen en esta lista.
    /// </remarks>
    /// <returns>Lista de productos que requieren reposición urgente</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de productos con stock bajo (puede estar vacía si no hay productos con stock bajo).</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("inventario/stock-bajo")]
    [ProducesResponseType(typeof(List<ProductoStockBajoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InventarioProductosStockBajo()
    {
        try
        {
            var resultados = await _analyticsService.InventarioProductosStockBajoAsync();
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos con stock bajo");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener valor del inventario
    /// </summary>
    [HttpGet("inventario/valor")]
    [ProducesResponseType(typeof(List<ValorInventarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> InventarioValorInventario([FromQuery] bool porCategoria = false)
    {
        try
        {
            var resultados = await _analyticsService.InventarioValorInventarioAsync(porCategoria);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valor del inventario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // ENDPOINTS DE MÉTRICAS/KPIs
    // ============================================

    /// <summary>
    /// Obtener métricas principales del dashboard
    /// </summary>
    /// <remarks>
    /// Retorna las métricas principales para el dashboard móvil, incluyendo:
    /// - **Ventas**: Total de ventas, número de transacciones, promedio de ticket
    /// - **Compras**: Total de compras, número de transacciones, promedio de compra
    /// - **Inventario**: Valor total del inventario, cantidad de productos
    /// 
    /// **Parámetros de fecha:**
    /// - Si no se especifican fechas, usa el último mes por defecto
    /// - `fechaInicio`: Fecha de inicio (formato: YYYY-MM-DD)
    /// - `fechaFin`: Fecha de fin (formato: YYYY-MM-DD, por defecto: hoy)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/analytics/metricas/dashboard
    /// GET /api/analytics/metricas/dashboard?fechaInicio=2025-01-01&fechaFin=2025-01-31
    /// ```
    /// 
    /// **Respuesta:**
    /// Retorna un array con 3 objetos (Ventas, Compras, Inventario), cada uno con:
    /// - `tipo`: "Ventas", "Compras" o "Inventario"
    /// - `total`: Valor total en la moneda configurada
    /// - `numeroTransacciones`: Cantidad de transacciones (solo para Ventas y Compras)
    /// - `promedio`: Promedio de ticket/compra
    /// - `cantidad`: Cantidad de productos/items (solo para Ventas y Compras)
    /// 
    /// **Uso en dashboard móvil:**
    /// Este endpoint está optimizado para mostrar tarjetas principales (KPIs) en la pantalla inicial del dashboard.
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del período (opcional, por defecto: último mes)</param>
    /// <param name="fechaFin">Fecha de fin del período (opcional, por defecto: hoy)</param>
    /// <returns>Métricas principales agrupadas por tipo (Ventas, Compras, Inventario)</returns>
    /// <response code="200">✅ Métricas obtenidas exitosamente. Retorna array con métricas de Ventas, Compras e Inventario.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("metricas/dashboard")]
    [ProducesResponseType(typeof(List<MetricasDashboardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MetricasDashboard(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null)
    {
        try
        {
            var resultados = await _analyticsService.MetricasDashboardAsync(fechaInicio, fechaFin);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas del dashboard");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener comparación de tendencias entre períodos
    /// </summary>
    [HttpGet("metricas/tendencias")]
    [ProducesResponseType(typeof(List<TendenciasDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MetricasTendencias(
        [FromQuery] DateTime periodoActualInicio,
        [FromQuery] DateTime periodoActualFin,
        [FromQuery] DateTime periodoAnteriorInicio,
        [FromQuery] DateTime periodoAnteriorFin)
    {
        try
        {
            var resultados = await _analyticsService.MetricasTendenciasAsync(
                periodoActualInicio, periodoActualFin, periodoAnteriorInicio, periodoAnteriorFin);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencias");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener productos más vendidos con ranking
    /// </summary>
    /// <remarks>
    /// Retorna el top de productos más vendidos ordenados por cantidad vendida. Ideal para gráficos de barras horizontales o tablas de ranking.
    /// 
    /// **Características:**
    /// - Ordenados por cantidad total vendida (descendente)
    /// - Incluye ranking automático (1, 2, 3, ...)
    /// - Muestra información completa del producto (nombre, código, marca, categoría)
    /// - Calcula total de ventas y número de transacciones
    /// 
    /// **Parámetros:**
    /// - `fechaInicio`: Fecha de inicio (opcional, por defecto: último mes)
    /// - `fechaFin`: Fecha de fin (opcional, por defecto: hoy)
    /// - `top`: Número de productos a retornar (por defecto: 10, máximo recomendado: 50)
    /// 
    /// **Ejemplo de uso:**
    /// ```
    /// GET /api/analytics/metricas/productos-mas-vendidos?top=20
    /// GET /api/analytics/metricas/productos-mas-vendidos?fechaInicio=2025-01-01&fechaFin=2025-01-31&top=15
    /// ```
    /// 
    /// **Respuesta incluye:**
    /// - `ranking`: Posición en el ranking (1 = más vendido)
    /// - `productoNombre`: Nombre del producto
    /// - `productoCodigo`: Código único del producto
    /// - `categoriaNombre`: Categoría a la que pertenece
    /// - `marcaNombre`: Marca del producto
    /// - `totalVendido`: Cantidad total de unidades vendidas
    /// - `totalVentas`: Monto total en ventas
    /// - `numeroVentas`: Cantidad de transacciones que incluyeron este producto
    /// 
    /// **Uso recomendado:**
    /// - Dashboard móvil: Gráfico de barras horizontales con top 10
    /// - Reportes: Tabla completa con todos los productos
    /// - Análisis: Identificar productos estrella y oportunidades
    /// </remarks>
    /// <param name="fechaInicio">Fecha de inicio del período (opcional)</param>
    /// <param name="fechaFin">Fecha de fin del período (opcional)</param>
    /// <param name="top">Número de productos a retornar (por defecto: 10)</param>
    /// <returns>Lista de productos más vendidos ordenados por cantidad, con ranking</returns>
    /// <response code="200">✅ Consulta exitosa. Retorna lista de productos ordenados por cantidad vendida.</response>
    /// <response code="401">❌ No autenticado. Se requiere token JWT válido.</response>
    /// <response code="500">❌ Error interno del servidor.</response>
    [HttpGet("metricas/productos-mas-vendidos")]
    [ProducesResponseType(typeof(List<ProductosMasVendidosDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MetricasProductosMasVendidos(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 10)
    {
        try
        {
            var resultados = await _analyticsService.MetricasProductosMasVendidosAsync(fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos más vendidos");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener clientes más frecuentes con ranking
    /// </summary>
    [HttpGet("metricas/clientes-mas-frecuentes")]
    [ProducesResponseType(typeof(List<ClientesMasFrecuentesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MetricasClientesMasFrecuentes(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 10)
    {
        try
        {
            var resultados = await _analyticsService.MetricasClientesMasFrecuentesAsync(fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes más frecuentes");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    // ============================================
    // ENDPOINTS DE REPORTES AVANZADOS
    // ============================================

    /// <summary>
    /// Obtener comparación ventas vs compras (ganancia bruta)
    /// </summary>
    [HttpGet("reportes/ventas-vs-compras")]
    [ProducesResponseType(typeof(List<VentasVsComprasDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReporteVentasVsCompras(
        [FromQuery] DateTime fechaInicio,
        [FromQuery] DateTime fechaFin,
        [FromQuery] string agruparPor = "Mes")
    {
        try
        {
            var resultados = await _analyticsService.ReporteVentasVsComprasAsync(fechaInicio, fechaFin, agruparPor);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte ventas vs compras");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtener rotación de inventario por producto
    /// </summary>
    [HttpGet("reportes/rotacion-inventario")]
    [ProducesResponseType(typeof(List<RotacionInventarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReporteRotacionInventario(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] int top = 20)
    {
        try
        {
            var resultados = await _analyticsService.ReporteRotacionInventarioAsync(fechaInicio, fechaFin, top);
            return Ok(resultados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rotación de inventario");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

