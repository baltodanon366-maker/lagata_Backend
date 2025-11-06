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
/// Servicio de Devoluciones de Venta (SQL Server) - Usa Stored Procedures
/// </summary>
public class DevolucionVentaService : IDevolucionVentaService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<DevolucionVentaService> _logger;

    public DevolucionVentaService(LicoreriaDbContext context, ILogger<DevolucionVentaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(int DevolucionVentaId, string? ErrorMessage)> CrearAsync(CrearDevolucionVentaDto crearDto, int usuarioId)
    {
        try
        {
            // Convertir detalles a JSON
            var detallesJson = JsonSerializer.Serialize(crearDto.Detalles.Select(d => new
            {
                VentaDetalleId = d.VentaDetalleId,
                DetalleProductoId = d.DetalleProductoId,
                CantidadDevolver = d.CantidadDevolver,
                Motivo = d.Motivo
            }));

            var folioParam = new SqlParameter("@Folio", crearDto.Folio);
            var ventaIdParam = new SqlParameter("@VentaId", crearDto.VentaId);
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);
            var fechaDevolucionParam = new SqlParameter("@FechaDevolucion", (object?)crearDto.FechaDevolucion ?? DBNull.Value);
            var motivoParam = new SqlParameter("@Motivo", crearDto.Motivo);
            var observacionesParam = new SqlParameter("@Observaciones", (object?)crearDto.Observaciones ?? DBNull.Value);
            var detallesDevolucionParam = new SqlParameter("@DetallesDevolucion", detallesJson);
            var devolucionVentaIdParam = new SqlParameter("@DevolucionVentaId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var mensajeErrorParam = new SqlParameter("@MensajeError", SqlDbType.NVarChar, 500)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_DevolucionVenta_Crear @Folio, @VentaId, @UsuarioId, @FechaDevolucion, @Motivo, @Observaciones, @DetallesDevolucion, @DevolucionVentaId OUTPUT, @MensajeError OUTPUT",
                folioParam, ventaIdParam, usuarioIdParam, fechaDevolucionParam, motivoParam, observacionesParam, detallesDevolucionParam, devolucionVentaIdParam, mensajeErrorParam);

            var devolucionVentaId = (int)devolucionVentaIdParam.Value!;
            var mensajeError = mensajeErrorParam.Value?.ToString();

            if (devolucionVentaId == -1)
            {
                return (-1, mensajeError ?? "Error al crear la devolución");
            }

            return (devolucionVentaId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear devolución de venta con folio: {Folio}", crearDto.Folio);
            return (-1, "Error interno al crear la devolución");
        }
    }

    public async Task<DevolucionVentaDto?> ObtenerPorIdAsync(int devolucionVentaId)
    {
        try
        {
            var devolucion = await _context.DevolucionesVenta
                .Include(dv => dv.Venta)
                .Include(dv => dv.Usuario)
                .Include(dv => dv.DevolucionesVentaDetalle)
                    .ThenInclude(dvd => dvd.DetalleProducto)
                        .ThenInclude(dp => dp.Producto)
                .Include(dv => dv.DevolucionesVentaDetalle)
                    .ThenInclude(dvd => dvd.DetalleProducto)
                        .ThenInclude(dp => dp.Marca)
                .FirstOrDefaultAsync(dv => dv.Id == devolucionVentaId);

            if (devolucion == null) return null;

            return new DevolucionVentaDto
            {
                Id = devolucion.Id,
                Folio = devolucion.Folio,
                VentaId = devolucion.VentaId,
                VentaFolio = devolucion.Venta.Folio,
                UsuarioId = devolucion.UsuarioId,
                UsuarioNombre = devolucion.Usuario.NombreUsuario,
                FechaDevolucion = devolucion.FechaDevolucion,
                Motivo = devolucion.Motivo,
                TotalDevolucion = devolucion.TotalDevolucion,
                Estado = devolucion.Estado,
                Observaciones = devolucion.Observaciones,
                FechaCreacion = devolucion.FechaCreacion,
                FechaModificacion = devolucion.FechaModificacion,
                Detalles = devolucion.DevolucionesVentaDetalle.Select(dvd => new DevolucionVentaDetalleDto
                {
                    Id = dvd.Id,
                    DevolucionVentaId = dvd.DevolucionVentaId,
                    VentaDetalleId = dvd.VentaDetalleId,
                    DetalleProductoId = dvd.DetalleProductoId,
                    ProductoCodigo = dvd.DetalleProducto.Codigo,
                    ProductoNombre = dvd.DetalleProducto.Producto.Nombre,
                    MarcaNombre = dvd.DetalleProducto.Marca.Nombre,
                    CantidadDevolver = dvd.CantidadDevolver,
                    Motivo = dvd.Motivo,
                    Subtotal = dvd.Subtotal
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener devolución de venta ID: {DevolucionVentaId}", devolucionVentaId);
            return null;
        }
    }

    public async Task<List<DevolucionVentaDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100)
    {
        try
        {
            var devoluciones = await _context.DevolucionesVenta
                .Include(dv => dv.Venta)
                .Include(dv => dv.Usuario)
                .Where(dv => dv.FechaDevolucion >= fechaInicio && dv.FechaDevolucion <= fechaFin)
                .OrderByDescending(dv => dv.FechaDevolucion)
                .Take(top)
                .ToListAsync();

            return devoluciones.Select(dv => new DevolucionVentaDto
            {
                Id = dv.Id,
                Folio = dv.Folio,
                VentaId = dv.VentaId,
                VentaFolio = dv.Venta.Folio,
                UsuarioId = dv.UsuarioId,
                UsuarioNombre = dv.Usuario.NombreUsuario,
                FechaDevolucion = dv.FechaDevolucion,
                Motivo = dv.Motivo,
                TotalDevolucion = dv.TotalDevolucion,
                Estado = dv.Estado,
                Observaciones = dv.Observaciones,
                FechaCreacion = dv.FechaCreacion,
                FechaModificacion = dv.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener devoluciones por rango de fechas");
            return new List<DevolucionVentaDto>();
        }
    }
}

