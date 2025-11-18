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
/// Servicio de Compras (SQL Server) - Usa Stored Procedures
/// </summary>
public class CompraService : ICompraService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<CompraService> _logger;

    public CompraService(LicoreriaDbContext context, ILogger<CompraService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(int CompraId, string? ErrorMessage)> CrearAsync(CrearCompraDto crearDto, int usuarioId)
    {
        try
        {
            // Convertir detalles a JSON
            var detallesJson = JsonSerializer.Serialize(crearDto.Detalles.Select(d => new
            {
                DetalleProductoId = d.DetalleProductoId,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario
            }));

            var folioParam = new SqlParameter("@Folio", crearDto.Folio);
            var proveedorIdParam = new SqlParameter("@ProveedorId", crearDto.ProveedorId);
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);
            var fechaCompraParam = new SqlParameter("@FechaCompra", (object?)crearDto.FechaCompra ?? DBNull.Value);
            var observacionesParam = new SqlParameter("@Observaciones", (object?)crearDto.Observaciones ?? DBNull.Value);
            var detallesCompraParam = new SqlParameter("@DetallesCompra", detallesJson);
            var compraIdParam = new SqlParameter("@CompraId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var mensajeErrorParam = new SqlParameter("@MensajeError", SqlDbType.NVarChar, 500)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Compra_Crear @Folio, @ProveedorId, @UsuarioId, @FechaCompra, @Observaciones, @DetallesCompra, @CompraId OUTPUT, @MensajeError OUTPUT",
                folioParam, proveedorIdParam, usuarioIdParam, fechaCompraParam, observacionesParam, detallesCompraParam, compraIdParam, mensajeErrorParam);

            var compraId = (int)compraIdParam.Value!;
            var mensajeError = mensajeErrorParam.Value?.ToString();

            if (compraId == -1)
            {
                return (-1, mensajeError ?? "Error al crear la compra");
            }

            return (compraId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear compra con folio: {Folio}", crearDto.Folio);
            return (-1, "Error interno al crear la compra");
        }
    }

    public async Task<CompraDto?> ObtenerPorIdAsync(int compraId)
    {
        try
        {
            _logger.LogInformation("Obteniendo compra con ID: {CompraId}", compraId);

            // Usar consulta directa con Include para evitar problemas de mapeo
            var compra = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Producto)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Marca)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Categoria)
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null)
            {
                _logger.LogWarning("Compra con ID {CompraId} no encontrada", compraId);
                return null;
            }

            _logger.LogInformation("Compra encontrada: ID={Id}, Folio={Folio}", compra.Id, compra.Folio);

            return new CompraDto
            {
                Id = compra.Id,
                Folio = compra.Folio,
                ProveedorId = compra.ProveedorId,
                ProveedorNombre = compra.Proveedor?.Nombre,
                UsuarioId = compra.UsuarioId,
                UsuarioNombre = compra.Usuario?.NombreUsuario,
                FechaCompra = compra.FechaCompra,
                Subtotal = compra.Subtotal,
                Impuestos = compra.Impuestos,
                Total = compra.Total,
                Estado = compra.Estado,
                Observaciones = compra.Observaciones,
                FechaCreacion = compra.FechaCreacion,
                FechaModificacion = compra.FechaModificacion,
                Detalles = compra.ComprasDetalle.Select(cd => new CompraDetalleDto
                {
                    Id = cd.Id,
                    CompraId = cd.CompraId,
                    DetalleProductoId = cd.DetalleProductoId,
                    ProductoCodigo = cd.DetalleProducto?.Codigo,
                    ProductoNombre = cd.DetalleProducto?.Producto?.Nombre,
                    MarcaNombre = cd.DetalleProducto?.Marca?.Nombre,
                    CategoriaNombre = cd.DetalleProducto?.Categoria?.Nombre,
                    Cantidad = cd.Cantidad,
                    PrecioUnitario = cd.PrecioUnitario,
                    Subtotal = cd.Subtotal
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compra ID: {CompraId}", compraId);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return null;
        }
    }

    public async Task<List<CompraDto>> MostrarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, int top = 100)
    {
        try
        {
            var fechaInicioParam = new SqlParameter("@FechaInicio", fechaInicio);
            var fechaFinParam = new SqlParameter("@FechaFin", fechaFin);
            var topParam = new SqlParameter("@Top", top);

            var compras = await _context.Compras
                .FromSqlRaw("EXEC sp_Compra_MostrarPorRangoFechas @FechaInicio, @FechaFin, @Top", fechaInicioParam, fechaFinParam, topParam)
                .ToListAsync();

            // Obtener relaciones
            var comprasIds = compras.Select(c => c.Id).ToList();
            var comprasConRelaciones = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Where(c => comprasIds.Contains(c.Id))
                .ToListAsync();

            return comprasConRelaciones.Select(c => new CompraDto
            {
                Id = c.Id,
                Folio = c.Folio,
                ProveedorId = c.ProveedorId,
                ProveedorNombre = c.Proveedor.Nombre,
                UsuarioId = c.UsuarioId,
                UsuarioNombre = c.Usuario.NombreUsuario,
                FechaCompra = c.FechaCompra,
                Subtotal = c.Subtotal,
                Impuestos = c.Impuestos,
                Total = c.Total,
                Estado = c.Estado,
                Observaciones = c.Observaciones,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return new List<CompraDto>();
        }
    }

    public async Task<List<CompraDto>> MostrarActivasAsync(int top = 100)
    {
        try
        {
            _logger.LogInformation("Ejecutando MostrarActivasAsync con Top={Top}", top);

            // Verificar si hay compras con estado 'Completada'
            var hayCompletadas = await _context.Compras.AnyAsync(c => c.Estado == "Completada");
            _logger.LogInformation("Hay compras completadas: {HayCompletadas}", hayCompletadas);

            // Usar consulta directa con Include para evitar problemas de mapeo
            var compras = await _context.Compras
                .Include(c => c.Proveedor)
                .Include(c => c.Usuario)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Producto)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Marca)
                .Include(c => c.ComprasDetalle)
                    .ThenInclude(cd => cd.DetalleProducto)
                        .ThenInclude(dp => dp.Categoria)
                .Where(c => hayCompletadas ? c.Estado == "Completada" : true)
                .OrderByDescending(c => c.FechaCompra)
                .Take(top)
                .ToListAsync();

            _logger.LogInformation("Consulta directa devolvió {Count} compras", compras.Count);

            _logger.LogInformation("Mapeando {Count} compras a DTOs", compras.Count);

            var comprasDto = compras.Select(c => new CompraDto
            {
                Id = c.Id,
                Folio = c.Folio,
                ProveedorId = c.ProveedorId,
                ProveedorNombre = c.Proveedor?.Nombre,
                UsuarioId = c.UsuarioId,
                UsuarioNombre = c.Usuario?.NombreUsuario,
                FechaCompra = c.FechaCompra,
                Subtotal = c.Subtotal,
                Impuestos = c.Impuestos,
                Total = c.Total,
                Estado = c.Estado,
                Observaciones = c.Observaciones,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion,
                Detalles = c.ComprasDetalle.Select(cd => new CompraDetalleDto
                {
                    Id = cd.Id,
                    CompraId = cd.CompraId,
                    DetalleProductoId = cd.DetalleProductoId,
                    ProductoCodigo = cd.DetalleProducto?.Codigo,
                    ProductoNombre = cd.DetalleProducto?.Producto?.Nombre,
                    MarcaNombre = cd.DetalleProducto?.Marca?.Nombre,
                    CategoriaNombre = cd.DetalleProducto?.Categoria?.Nombre,
                    Cantidad = cd.Cantidad,
                    PrecioUnitario = cd.PrecioUnitario,
                    Subtotal = cd.Subtotal
                }).ToList()
            }).ToList();

            _logger.LogInformation("Se obtuvieron {Count} compras activas con detalles", comprasDto.Count);
            return comprasDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras activas: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return new List<CompraDto>();
        }
    }
}

