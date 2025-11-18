using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Transacciones;
using LicoreriaAPI.Domain.Models;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de Ventas (SQL Server) - Usa Stored Procedures
/// </summary>
public class VentaService : IVentaService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<VentaService> _logger;

    public VentaService(LicoreriaDbContext context, ILogger<VentaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(int VentaId, string? ErrorMessage)> CrearAsync(CrearVentaDto crearDto, int usuarioId)
    {
        try
        {
            // Convertir detalles a JSON
            var detallesJson = JsonSerializer.Serialize(crearDto.Detalles.Select(d => new
            {
                DetalleProductoId = d.DetalleProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Descuento = d.Descuento
            }));

            var folioParam = new SqlParameter("@Folio", crearDto.Folio);
            var clienteIdParam = new SqlParameter("@ClienteId", (object?)crearDto.ClienteId ?? DBNull.Value);
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);
            var empleadoIdParam = new SqlParameter("@EmpleadoId", (object?)crearDto.EmpleadoId ?? DBNull.Value);
            var fechaVentaParam = new SqlParameter("@FechaVenta", (object?)crearDto.FechaVenta ?? DBNull.Value);
            var metodoPagoParam = new SqlParameter("@MetodoPago", (object?)crearDto.MetodoPago ?? DBNull.Value);
            var observacionesParam = new SqlParameter("@Observaciones", (object?)crearDto.Observaciones ?? DBNull.Value);
            var detallesVentaParam = new SqlParameter("@DetallesVenta", detallesJson);
            var ventaIdParam = new SqlParameter("@VentaId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var mensajeErrorParam = new SqlParameter("@MensajeError", SqlDbType.NVarChar, 500)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Venta_Crear @Folio, @ClienteId, @UsuarioId, @EmpleadoId, @FechaVenta, @MetodoPago, @Observaciones, @DetallesVenta, @VentaId OUTPUT, @MensajeError OUTPUT",
                folioParam, clienteIdParam, usuarioIdParam, empleadoIdParam, fechaVentaParam, metodoPagoParam, observacionesParam, detallesVentaParam, ventaIdParam, mensajeErrorParam);

            var ventaId = (int)ventaIdParam.Value!;
            var mensajeError = mensajeErrorParam.Value?.ToString();

            if (ventaId == -1)
            {
                return (-1, mensajeError ?? "Error al crear la venta");
            }

            return (ventaId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear venta con folio: {Folio}", crearDto.Folio);
            return (-1, "Error interno al crear la venta");
        }
    }

    public async Task<VentaDto?> ObtenerPorIdAsync(int ventaId)
    {
        try
        {
            _logger.LogInformation("Obteniendo venta con ID: {VentaId}", ventaId);

            // Usar consulta directa con Include para evitar problemas de mapeo
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Empleado)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Producto)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Marca)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Categoria)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta == null)
            {
                _logger.LogWarning("Venta con ID {VentaId} no encontrada", ventaId);
                return null;
            }

            _logger.LogInformation("Venta encontrada: ID={Id}, Folio={Folio}", venta.Id, venta.Folio);

            return new VentaDto
            {
                Id = venta.Id,
                Folio = venta.Folio,
                ClienteId = venta.ClienteId,
                ClienteNombre = venta.Cliente?.NombreCompleto,
                UsuarioId = venta.UsuarioId,
                UsuarioNombre = venta.Usuario?.NombreUsuario,
                EmpleadoId = venta.EmpleadoId,
                EmpleadoNombre = venta.Empleado?.NombreCompleto,
                FechaVenta = venta.FechaVenta,
                Subtotal = venta.Subtotal,
                Impuestos = venta.Impuestos,
                Descuento = venta.Descuento,
                Total = venta.Total,
                MetodoPago = venta.MetodoPago,
                Estado = venta.Estado,
                Observaciones = venta.Observaciones,
                FechaCreacion = venta.FechaCreacion,
                FechaModificacion = venta.FechaModificacion,
                Detalles = venta.VentasDetalle.Select(vd => new VentaDetalleDto
                {
                    Id = vd.Id,
                    VentaId = vd.VentaId,
                    DetalleProductoId = vd.DetalleProductoId,
                    ProductoCodigo = vd.DetalleProducto?.Codigo,
                    ProductoNombre = vd.DetalleProducto?.Producto?.Nombre,
                    MarcaNombre = vd.DetalleProducto?.Marca?.Nombre,
                    CategoriaNombre = vd.DetalleProducto?.Categoria?.Nombre,
                    Cantidad = vd.Cantidad,
                    PrecioUnitario = vd.PrecioUnitario,
                    Descuento = vd.Descuento,
                    Subtotal = vd.Subtotal
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener venta ID: {VentaId}", ventaId);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return null;
        }
    }

    public async Task<List<VentaDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", fechaInicio);
            var fechaFinParam = new SqlParameter("@FechaFin", fechaFin);
            var topParam = new SqlParameter("@Top", top);

            var ventas = await _context.Ventas
                .FromSqlRaw("EXEC sp_Venta_MostrarPorRangoFechas @FechaInicio, @FechaFin, @Top", fechaInicioParam, fechaFinParam, topParam)
                .ToListAsync();

            // Obtener relaciones
            var ventasIds = ventas.Select(v => v.Id).ToList();
            var ventasConRelaciones = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Empleado)
                .Where(v => ventasIds.Contains(v.Id))
                .ToListAsync();

            return ventasConRelaciones.Select(v => new VentaDto
            {
                Id = v.Id,
                Folio = v.Folio,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente?.NombreCompleto,
                UsuarioId = v.UsuarioId,
                UsuarioNombre = v.Usuario.NombreUsuario,
                EmpleadoId = v.EmpleadoId,
                EmpleadoNombre = v.Empleado?.NombreCompleto,
                FechaVenta = v.FechaVenta,
                Subtotal = v.Subtotal,
                Impuestos = v.Impuestos,
                Descuento = v.Descuento,
                Total = v.Total,
                MetodoPago = v.MetodoPago,
                Estado = v.Estado,
                Observaciones = v.Observaciones,
                FechaCreacion = v.FechaCreacion,
                FechaModificacion = v.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por rango de fechas");
            return new List<VentaDto>();
        }
    }

    public async Task<List<VentaDto>> MostrarActivasAsync(int top = 100)
    {
        try
        {
            _logger.LogInformation("Ejecutando MostrarActivasAsync con Top={Top}", top);

            // Verificar si hay ventas con estado 'Completada'
            var hayCompletadas = await _context.Ventas.AnyAsync(v => v.Estado == "Completada");
            _logger.LogInformation("Hay ventas completadas: {HayCompletadas}", hayCompletadas);

            // Usar consulta directa con Include para evitar problemas de mapeo
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Empleado)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Producto)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Marca)
                .Include(v => v.VentasDetalle)
                    .ThenInclude(vd => vd.DetalleProducto)
                        .ThenInclude(dp => dp.Categoria)
                .Where(v => hayCompletadas ? v.Estado == "Completada" : true)
                .OrderByDescending(v => v.FechaVenta)
                .Take(top)
                .ToListAsync();

            _logger.LogInformation("Consulta directa devolvió {Count} ventas", ventas.Count);

            _logger.LogInformation("Mapeando {Count} ventas a DTOs", ventas.Count);

            var ventasDto = ventas.Select(v => new VentaDto
            {
                Id = v.Id,
                Folio = v.Folio,
                ClienteId = v.ClienteId,
                ClienteNombre = v.Cliente?.NombreCompleto,
                UsuarioId = v.UsuarioId,
                UsuarioNombre = v.Usuario?.NombreUsuario,
                EmpleadoId = v.EmpleadoId,
                EmpleadoNombre = v.Empleado?.NombreCompleto,
                FechaVenta = v.FechaVenta,
                Subtotal = v.Subtotal,
                Impuestos = v.Impuestos,
                Descuento = v.Descuento,
                Total = v.Total,
                MetodoPago = v.MetodoPago,
                Estado = v.Estado,
                Observaciones = v.Observaciones,
                FechaCreacion = v.FechaCreacion,
                FechaModificacion = v.FechaModificacion,
                Detalles = v.VentasDetalle.Select(vd => new VentaDetalleDto
                {
                    Id = vd.Id,
                    VentaId = vd.VentaId,
                    DetalleProductoId = vd.DetalleProductoId,
                    ProductoCodigo = vd.DetalleProducto?.Codigo,
                    ProductoNombre = vd.DetalleProducto?.Producto?.Nombre,
                    MarcaNombre = vd.DetalleProducto?.Marca?.Nombre,
                    CategoriaNombre = vd.DetalleProducto?.Categoria?.Nombre,
                    Cantidad = vd.Cantidad,
                    PrecioUnitario = vd.PrecioUnitario,
                    Descuento = vd.Descuento,
                    Subtotal = vd.Subtotal
                }).ToList()
            }).ToList();

            _logger.LogInformation("Se obtuvieron {Count} ventas activas con detalles", ventasDto.Count);
            return ventasDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas activas: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return new List<VentaDto>();
        }
    }
}

