using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Catalogos;
using LicoreriaAPI.Domain.Models;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de DetalleProducto (SQL Server) - Usa Stored Procedures
/// </summary>
public class DetalleProductoService : IDetalleProductoService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<DetalleProductoService> _logger;

    public DetalleProductoService(LicoreriaDbContext context, ILogger<DetalleProductoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DetalleProductoDto?> CrearAsync(CrearDetalleProductoDto crearDto)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var productoIdParam = new SqlParameter("@ProductoId", crearDto.ProductoId);
            var categoriaIdParam = new SqlParameter("@CategoriaId", crearDto.CategoriaId);
            var marcaIdParam = new SqlParameter("@MarcaId", crearDto.MarcaId);
            var modeloIdParam = new SqlParameter("@ModeloId", crearDto.ModeloId);
            var codigoParam = new SqlParameter("@Codigo", crearDto.Codigo);
            var skuParam = new SqlParameter("@SKU", (object?)crearDto.SKU ?? DBNull.Value);
            var observacionesParam = new SqlParameter("@Observaciones", (object?)crearDto.Observaciones ?? DBNull.Value);
            var precioCompraParam = new SqlParameter("@PrecioCompra", crearDto.PrecioCompra);
            var precioVentaParam = new SqlParameter("@PrecioVenta", crearDto.PrecioVenta);
            var stockMinimoParam = new SqlParameter("@StockMinimo", crearDto.StockMinimo);
            var unidadMedidaParam = new SqlParameter("@UnidadMedida", (object?)crearDto.UnidadMedida ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_DetalleProducto_Crear @ProductoId, @CategoriaId, @MarcaId, @ModeloId, @Codigo, @SKU, @Observaciones, @PrecioCompra, @PrecioVenta, @StockMinimo, @UnidadMedida, @DetalleProductoId OUTPUT",
                productoIdParam, categoriaIdParam, marcaIdParam, modeloIdParam, codigoParam, skuParam, observacionesParam, precioCompraParam, precioVentaParam, stockMinimoParam, unidadMedidaParam, detalleProductoIdParam);

            var detalleProductoId = (int)detalleProductoIdParam.Value!;

            if (detalleProductoId == -1)
            {
                return null; // Ya existe un detalle producto con ese código
            }

            return await MostrarActivosPorIdAsync(detalleProductoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear detalle producto: {Codigo}", crearDto.Codigo);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarDetalleProductoDto editarDto)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", id);
            var categoriaIdParam = new SqlParameter("@CategoriaId", editarDto.CategoriaId);
            var marcaIdParam = new SqlParameter("@MarcaId", editarDto.MarcaId);
            var modeloIdParam = new SqlParameter("@ModeloId", editarDto.ModeloId);
            var skuParam = new SqlParameter("@SKU", (object?)editarDto.SKU ?? DBNull.Value);
            var observacionesParam = new SqlParameter("@Observaciones", (object?)editarDto.Observaciones ?? DBNull.Value);
            var precioCompraParam = new SqlParameter("@PrecioCompra", editarDto.PrecioCompra);
            var precioVentaParam = new SqlParameter("@PrecioVenta", editarDto.PrecioVenta);
            var stockMinimoParam = new SqlParameter("@StockMinimo", editarDto.StockMinimo);
            var unidadMedidaParam = new SqlParameter("@UnidadMedida", (object?)editarDto.UnidadMedida ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_DetalleProducto_Editar @DetalleProductoId, @CategoriaId, @MarcaId, @ModeloId, @SKU, @Observaciones, @PrecioCompra, @PrecioVenta, @StockMinimo, @UnidadMedida, @Resultado OUTPUT",
                detalleProductoIdParam, categoriaIdParam, marcaIdParam, modeloIdParam, skuParam, observacionesParam, precioCompraParam, precioVentaParam, stockMinimoParam, unidadMedidaParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar detalle producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_DetalleProducto_Activar @DetalleProductoId, @Resultado OUTPUT",
                detalleProductoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar detalle producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_DetalleProducto_Desactivar @DetalleProductoId, @Resultado OUTPUT",
                detalleProductoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar detalle producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<DetalleProductoDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var detallesProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .Where(dp => dp.Activo)
                .OrderByDescending(dp => dp.FechaCreacion)
                .Take(top)
                .ToListAsync();

            return detallesProducto.Select(dp => new DetalleProductoDto
            {
                Id = dp.Id,
                ProductoId = dp.ProductoId,
                ProductoNombre = dp.Producto.Nombre,
                CategoriaId = dp.CategoriaId,
                CategoriaNombre = dp.Categoria.Nombre,
                MarcaId = dp.MarcaId,
                MarcaNombre = dp.Marca.Nombre,
                ModeloId = dp.ModeloId,
                ModeloNombre = dp.Modelo.Nombre,
                Codigo = dp.Codigo,
                SKU = dp.SKU,
                Observaciones = dp.Observaciones,
                PrecioCompra = dp.PrecioCompra,
                PrecioVenta = dp.PrecioVenta,
                Stock = dp.Stock,
                StockMinimo = dp.StockMinimo,
                UnidadMedida = dp.UnidadMedida,
                FechaUltimoMovimiento = dp.FechaUltimoMovimiento,
                Activo = dp.Activo,
                FechaCreacion = dp.FechaCreacion,
                FechaModificacion = dp.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles producto activos");
            return new List<DetalleProductoDto>();
        }
    }

    public async Task<DetalleProductoDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", id);

            var detalleProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .FirstOrDefaultAsync(dp => dp.Id == id && dp.Activo);

            if (detalleProducto == null) return null;

            return new DetalleProductoDto
            {
                Id = detalleProducto.Id,
                ProductoId = detalleProducto.ProductoId,
                ProductoNombre = detalleProducto.Producto.Nombre,
                CategoriaId = detalleProducto.CategoriaId,
                CategoriaNombre = detalleProducto.Categoria.Nombre,
                MarcaId = detalleProducto.MarcaId,
                MarcaNombre = detalleProducto.Marca.Nombre,
                ModeloId = detalleProducto.ModeloId,
                ModeloNombre = detalleProducto.Modelo.Nombre,
                Codigo = detalleProducto.Codigo,
                SKU = detalleProducto.SKU,
                Observaciones = detalleProducto.Observaciones,
                PrecioCompra = detalleProducto.PrecioCompra,
                PrecioVenta = detalleProducto.PrecioVenta,
                Stock = detalleProducto.Stock,
                StockMinimo = detalleProducto.StockMinimo,
                UnidadMedida = detalleProducto.UnidadMedida,
                FechaUltimoMovimiento = detalleProducto.FechaUltimoMovimiento,
                Activo = detalleProducto.Activo,
                FechaCreacion = detalleProducto.FechaCreacion,
                FechaModificacion = detalleProducto.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle producto ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<DetalleProductoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var detallesProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .Where(dp => dp.Activo && dp.Producto.Nombre.Contains(nombre))
                .OrderBy(dp => dp.Producto.Nombre)
                .Take(top)
                .ToListAsync();

            return detallesProducto.Select(dp => new DetalleProductoDto
            {
                Id = dp.Id,
                ProductoId = dp.ProductoId,
                ProductoNombre = dp.Producto.Nombre,
                CategoriaId = dp.CategoriaId,
                CategoriaNombre = dp.Categoria.Nombre,
                MarcaId = dp.MarcaId,
                MarcaNombre = dp.Marca.Nombre,
                ModeloId = dp.ModeloId,
                ModeloNombre = dp.Modelo.Nombre,
                Codigo = dp.Codigo,
                SKU = dp.SKU,
                Observaciones = dp.Observaciones,
                PrecioCompra = dp.PrecioCompra,
                PrecioVenta = dp.PrecioVenta,
                Stock = dp.Stock,
                StockMinimo = dp.StockMinimo,
                UnidadMedida = dp.UnidadMedida,
                FechaUltimoMovimiento = dp.FechaUltimoMovimiento,
                Activo = dp.Activo,
                FechaCreacion = dp.FechaCreacion,
                FechaModificacion = dp.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar detalles producto por nombre: {Nombre}", nombre);
            return new List<DetalleProductoDto>();
        }
    }

    public async Task<List<DetalleProductoDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var detallesProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .Where(dp => !dp.Activo)
                .OrderByDescending(dp => dp.FechaCreacion)
                .Take(top)
                .ToListAsync();

            return detallesProducto.Select(dp => new DetalleProductoDto
            {
                Id = dp.Id,
                ProductoId = dp.ProductoId,
                ProductoNombre = dp.Producto.Nombre,
                CategoriaId = dp.CategoriaId,
                CategoriaNombre = dp.Categoria.Nombre,
                MarcaId = dp.MarcaId,
                MarcaNombre = dp.Marca.Nombre,
                ModeloId = dp.ModeloId,
                ModeloNombre = dp.Modelo.Nombre,
                Codigo = dp.Codigo,
                SKU = dp.SKU,
                Observaciones = dp.Observaciones,
                PrecioCompra = dp.PrecioCompra,
                PrecioVenta = dp.PrecioVenta,
                Stock = dp.Stock,
                StockMinimo = dp.StockMinimo,
                UnidadMedida = dp.UnidadMedida,
                FechaUltimoMovimiento = dp.FechaUltimoMovimiento,
                Activo = dp.Activo,
                FechaCreacion = dp.FechaCreacion,
                FechaModificacion = dp.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles producto inactivos");
            return new List<DetalleProductoDto>();
        }
    }

    public async Task<List<DetalleProductoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var detallesProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .Where(dp => !dp.Activo && dp.Producto.Nombre.Contains(nombre))
                .OrderBy(dp => dp.Producto.Nombre)
                .Take(top)
                .ToListAsync();

            return detallesProducto.Select(dp => new DetalleProductoDto
            {
                Id = dp.Id,
                ProductoId = dp.ProductoId,
                ProductoNombre = dp.Producto.Nombre,
                CategoriaId = dp.CategoriaId,
                CategoriaNombre = dp.Categoria.Nombre,
                MarcaId = dp.MarcaId,
                MarcaNombre = dp.Marca.Nombre,
                ModeloId = dp.ModeloId,
                ModeloNombre = dp.Modelo.Nombre,
                Codigo = dp.Codigo,
                SKU = dp.SKU,
                Observaciones = dp.Observaciones,
                PrecioCompra = dp.PrecioCompra,
                PrecioVenta = dp.PrecioVenta,
                Stock = dp.Stock,
                StockMinimo = dp.StockMinimo,
                UnidadMedida = dp.UnidadMedida,
                FechaUltimoMovimiento = dp.FechaUltimoMovimiento,
                Activo = dp.Activo,
                FechaCreacion = dp.FechaCreacion,
                FechaModificacion = dp.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar detalles producto inactivos por nombre: {Nombre}", nombre);
            return new List<DetalleProductoDto>();
        }
    }

    public async Task<DetalleProductoDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var detalleProductoIdParam = new SqlParameter("@DetalleProductoId", id);

            var detalleProducto = await _context.DetallesProducto
                .Include(dp => dp.Producto)
                .Include(dp => dp.Categoria)
                .Include(dp => dp.Marca)
                .Include(dp => dp.Modelo)
                .FirstOrDefaultAsync(dp => dp.Id == id && !dp.Activo);

            if (detalleProducto == null) return null;

            return new DetalleProductoDto
            {
                Id = detalleProducto.Id,
                ProductoId = detalleProducto.ProductoId,
                ProductoNombre = detalleProducto.Producto.Nombre,
                CategoriaId = detalleProducto.CategoriaId,
                CategoriaNombre = detalleProducto.Categoria.Nombre,
                MarcaId = detalleProducto.MarcaId,
                MarcaNombre = detalleProducto.Marca.Nombre,
                ModeloId = detalleProducto.ModeloId,
                ModeloNombre = detalleProducto.Modelo.Nombre,
                Codigo = detalleProducto.Codigo,
                SKU = detalleProducto.SKU,
                Observaciones = detalleProducto.Observaciones,
                PrecioCompra = detalleProducto.PrecioCompra,
                PrecioVenta = detalleProducto.PrecioVenta,
                Stock = detalleProducto.Stock,
                StockMinimo = detalleProducto.StockMinimo,
                UnidadMedida = detalleProducto.UnidadMedida,
                FechaUltimoMovimiento = detalleProducto.FechaUltimoMovimiento,
                Activo = detalleProducto.Activo,
                FechaCreacion = detalleProducto.FechaCreacion,
                FechaModificacion = detalleProducto.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle producto inactivo ID: {Id}", id);
            return null;
        }
    }
}

