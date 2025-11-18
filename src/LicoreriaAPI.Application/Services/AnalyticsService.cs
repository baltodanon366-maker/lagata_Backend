using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Analytics;
using LicoreriaAPI.Infrastructure.Data.DataWarehouse;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de Analytics (Data Warehouse) - Usa Stored Procedures
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly DataWarehouseContext _dwContext;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(DataWarehouseContext dwContext, ILogger<AnalyticsService> logger)
    {
        _dwContext = dwContext;
        _logger = logger;
    }

    // ============================================
    // VENTAS
    // ============================================

    public async Task<List<VentasPorRangoFechasDto>> VentasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Dia")
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", fechaInicio);
            var fechaFinParam = new SqlParameter("@FechaFin", fechaFin);
            var agruparPorParam = new SqlParameter("@AgruparPor", agruparPor);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorRangoFechasDto>(
                "EXEC sp_DW_Ventas_PorRangoFechas @FechaInicio, @FechaFin, @AgruparPor",
                fechaInicioParam, fechaFinParam, agruparPorParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por rango de fechas");
            return new List<VentasPorRangoFechasDto>();
        }
    }

    public async Task<List<VentasPorProductoDto>> VentasPorProductoAsync(int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 20)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", (object?)productoId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorProductoDto>(
                "EXEC sp_DW_Ventas_PorProducto @ProductoId, @FechaInicio, @FechaFin, @Top",
                productoIdParam, fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por producto");
            return new List<VentasPorProductoDto>();
        }
    }

    public async Task<List<VentasPorCategoriaDto>> VentasPorCategoriaAsync(int? categoriaId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", (object?)categoriaId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorCategoriaDto>(
                "EXEC sp_DW_Ventas_PorCategoria @CategoriaId, @FechaInicio, @FechaFin",
                categoriaIdParam, fechaInicioParam, fechaFinParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por categoría");
            return new List<VentasPorCategoriaDto>();
        }
    }

    public async Task<List<VentasPorClienteDto>> VentasPorClienteAsync(int? clienteId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 20)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", (object?)clienteId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorClienteDto>(
                "EXEC sp_DW_Ventas_PorCliente @ClienteId, @FechaInicio, @FechaFin, @Top",
                clienteIdParam, fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por cliente");
            return new List<VentasPorClienteDto>();
        }
    }

    public async Task<List<VentasPorEmpleadoDto>> VentasPorEmpleadoAsync(int? empleadoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", (object?)empleadoId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorEmpleadoDto>(
                "EXEC sp_DW_Ventas_PorEmpleado @EmpleadoId, @FechaInicio, @FechaFin",
                empleadoIdParam, fechaInicioParam, fechaFinParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por empleado");
            return new List<VentasPorEmpleadoDto>();
        }
    }

    public async Task<List<VentasPorMetodoPagoDto>> VentasPorMetodoPagoAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasPorMetodoPagoDto>(
                "EXEC sp_DW_Ventas_MetodoPago @FechaInicio, @FechaFin",
                fechaInicioParam, fechaFinParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por método de pago");
            return new List<VentasPorMetodoPagoDto>();
        }
    }

    // ============================================
    // COMPRAS
    // ============================================

    public async Task<List<ComprasPorRangoFechasDto>> ComprasPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Dia")
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", fechaInicio);
            var fechaFinParam = new SqlParameter("@FechaFin", fechaFin);
            var agruparPorParam = new SqlParameter("@AgruparPor", agruparPor);

            var resultados = await _dwContext.Database.SqlQueryRaw<ComprasPorRangoFechasDto>(
                "EXEC sp_DW_Compras_PorRangoFechas @FechaInicio, @FechaFin, @AgruparPor",
                fechaInicioParam, fechaFinParam, agruparPorParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return new List<ComprasPorRangoFechasDto>();
        }
    }

    public async Task<List<ComprasPorProveedorDto>> ComprasPorProveedorAsync(int? proveedorId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 20)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", (object?)proveedorId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<ComprasPorProveedorDto>(
                "EXEC sp_DW_Compras_PorProveedor @ProveedorId, @FechaInicio, @FechaFin, @Top",
                proveedorIdParam, fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por proveedor");
            return new List<ComprasPorProveedorDto>();
        }
    }

    public async Task<List<ComprasPorProductoDto>> ComprasPorProductoAsync(int? productoId = null, DateTime? fechaInicio = null, DateTime? fechaFin = null, int top = 20)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", (object?)productoId ?? DBNull.Value);
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<ComprasPorProductoDto>(
                "EXEC sp_DW_Compras_PorProducto @ProductoId, @FechaInicio, @FechaFin, @Top",
                productoIdParam, fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por producto");
            return new List<ComprasPorProductoDto>();
        }
    }

    // ============================================
    // INVENTARIO
    // ============================================

    public async Task<List<StockActualDto>> InventarioStockActualAsync(bool incluirInactivos = false, int? top = null)
    {
        try
        {
            var incluirInactivosParam = new SqlParameter("@IncluirInactivos", incluirInactivos ? 1 : 0);
            var topParam = new SqlParameter("@Top", (object?)top ?? DBNull.Value);

            var resultados = await _dwContext.Database.SqlQueryRaw<StockActualDto>(
                "EXEC sp_DW_Inventario_StockActual @IncluirInactivos, @Top",
                incluirInactivosParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener stock actual");
            return new List<StockActualDto>();
        }
    }

    public async Task<List<ProductoStockBajoDto>> InventarioProductosStockBajoAsync()
    {
        try
        {
            var resultados = await _dwContext.Database.SqlQueryRaw<ProductoStockBajoDto>(
                "EXEC sp_DW_Inventario_ProductosStockBajo").ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos con stock bajo");
            return new List<ProductoStockBajoDto>();
        }
    }

    public async Task<List<ValorInventarioDto>> InventarioValorInventarioAsync(bool porCategoria = false)
    {
        try
        {
            var porCategoriaParam = new SqlParameter("@PorCategoria", porCategoria ? 1 : 0);

            var resultados = await _dwContext.Database.SqlQueryRaw<ValorInventarioDto>(
                "EXEC sp_DW_Inventario_ValorInventario @PorCategoria",
                porCategoriaParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valor del inventario");
            return new List<ValorInventarioDto>();
        }
    }

    // ============================================
    // MÉTRICAS/KPIs
    // ============================================

    public async Task<List<MetricasDashboardDto>> MetricasDashboardAsync(DateTime? fechaInicio, DateTime? fechaFin)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);

            var resultados = await _dwContext.Database.SqlQueryRaw<MetricasDashboardDto>(
                "EXEC sp_DW_Metricas_Dashboard @FechaInicio, @FechaFin",
                fechaInicioParam, fechaFinParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas del dashboard");
            return new List<MetricasDashboardDto>();
        }
    }

    public async Task<List<TendenciasDto>> MetricasTendenciasAsync(DateTime periodoActualInicio, DateTime periodoActualFin, DateTime periodoAnteriorInicio, DateTime periodoAnteriorFin)
    {
        try
        {
            var periodoActualInicioParam = new SqlParameter("@PeriodoActualInicio", periodoActualInicio);
            var periodoActualFinParam = new SqlParameter("@PeriodoActualFin", periodoActualFin);
            var periodoAnteriorInicioParam = new SqlParameter("@PeriodoAnteriorInicio", periodoAnteriorInicio);
            var periodoAnteriorFinParam = new SqlParameter("@PeriodoAnteriorFin", periodoAnteriorFin);

            var resultados = await _dwContext.Database.SqlQueryRaw<TendenciasDto>(
                "EXEC sp_DW_Metricas_Tendencias @PeriodoActualInicio, @PeriodoActualFin, @PeriodoAnteriorInicio, @PeriodoAnteriorFin",
                periodoActualInicioParam, periodoActualFinParam, periodoAnteriorInicioParam, periodoAnteriorFinParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencias");
            return new List<TendenciasDto>();
        }
    }

    public async Task<List<ProductosMasVendidosDto>> MetricasProductosMasVendidosAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<ProductosMasVendidosDto>(
                "EXEC sp_DW_Metricas_ProductosMasVendidos @FechaInicio, @FechaFin, @Top",
                fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos más vendidos");
            return new List<ProductosMasVendidosDto>();
        }
    }

    public async Task<List<ClientesMasFrecuentesDto>> MetricasClientesMasFrecuentesAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 10)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<ClientesMasFrecuentesDto>(
                "EXEC sp_DW_Metricas_ClientesMasFrecuentes @FechaInicio, @FechaFin, @Top",
                fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes más frecuentes");
            return new List<ClientesMasFrecuentesDto>();
        }
    }

    // ============================================
    // REPORTES AVANZADOS
    // ============================================

    public async Task<List<VentasVsComprasDto>> ReporteVentasVsComprasAsync(DateTime fechaInicio, DateTime fechaFin, string agruparPor = "Mes")
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", fechaInicio);
            var fechaFinParam = new SqlParameter("@FechaFin", fechaFin);
            var agruparPorParam = new SqlParameter("@AgruparPor", agruparPor);

            var resultados = await _dwContext.Database.SqlQueryRaw<VentasVsComprasDto>(
                "EXEC sp_DW_Reporte_VentasVsCompras @FechaInicio, @FechaFin, @AgruparPor",
                fechaInicioParam, fechaFinParam, agruparPorParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte ventas vs compras");
            return new List<VentasVsComprasDto>();
        }
    }

    public async Task<List<RotacionInventarioDto>> ReporteRotacionInventarioAsync(DateTime? fechaInicio, DateTime? fechaFin, int top = 20)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", (object?)fechaInicio ?? DBNull.Value);
            var fechaFinParam = new SqlParameter("@FechaFin", (object?)fechaFin ?? DBNull.Value);
            var topParam = new SqlParameter("@Top", top);

            var resultados = await _dwContext.Database.SqlQueryRaw<RotacionInventarioDto>(
                "EXEC sp_DW_Reporte_RotacionInventario @FechaInicio, @FechaFin, @Top",
                fechaInicioParam, fechaFinParam, topParam).ToListAsync();

            return resultados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener rotación de inventario");
            return new List<RotacionInventarioDto>();
        }
    }
}

