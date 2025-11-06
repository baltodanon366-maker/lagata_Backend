using LicoreriaAPI.DTOs.Analytics;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Analytics (Data Warehouse)
/// </summary>
public interface IAnalyticsService
{
    // Ventas
    Task<List<VentasPorRangoFechasDto>> VentasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Dia");
    Task<List<VentasPorProductoDto>> VentasPorProductoAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20);
    Task<List<VentasPorCategoriaDto>> VentasPorCategoriaAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<List<VentasPorClienteDto>> VentasPorClienteAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20);
    Task<List<VentasPorEmpleadoDto>> VentasPorEmpleadoAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<List<VentasPorMetodoPagoDto>> VentasPorMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin);

    // Compras
    Task<List<ComprasPorRangoFechasDto>> ComprasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Dia");
    Task<List<ComprasPorProveedorDto>> ComprasPorProveedorAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20);
    Task<List<ComprasPorProductoDto>> ComprasPorProductoAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20);

    // Inventario
    Task<List<StockActualDto>> InventarioStockActualAsync(bool incluirInactivos = false, int? top = null);
    Task<List<ProductoStockBajoDto>> InventarioProductosStockBajoAsync();
    Task<List<ValorInventarioDto>> InventarioValorInventarioAsync(bool porCategoria = false);

    // Métricas/KPIs
    Task<List<MetricasDashboardDto>> MetricasDashboardAsync(DateTime? fechaInicio, DateTime? fechaFin);
    Task<List<TendenciasDto>> MetricasTendenciasAsync(DateTime periodoActualInicio, DateTime periodoActualFin, DateTime periodoAnteriorInicio, DateTime periodoAnteriorFin);
    Task<List<ProductosMasVendidosDto>> MetricasProductosMasVendidosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);
    Task<List<ClientesMasFrecuentesDto>> MetricasClientesMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10);

    // Reportes Avanzados
    Task<List<VentasVsComprasDto>> ReporteVentasVsComprasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Mes");
    Task<List<RotacionInventarioDto>> ReporteRotacionInventarioAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20);
}

