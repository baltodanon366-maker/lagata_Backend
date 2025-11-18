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
/// Servicio de Productos (SQL Server) - Usa Stored Procedures
/// </summary>
public class ProductoService : IProductoService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<ProductoService> _logger;

    public ProductoService(LicoreriaDbContext context, ILogger<ProductoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProductoDto?> CrearAsync(CrearProductoDto crearDto)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var nombreParam = new SqlParameter("@Nombre", crearDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)crearDto.Descripcion ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Crear @Nombre, @Descripcion, @ProductoId OUTPUT",
                nombreParam, descripcionParam, productoIdParam);

            var productoId = (int)productoIdParam.Value!;

            if (productoId == -1)
            {
                return null; // Ya existe un producto con ese nombre
            }

            return await MostrarActivosPorIdAsync(productoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto: {Nombre}", crearDto.Nombre);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarProductoDto editarDto)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", id);
            var nombreParam = new SqlParameter("@Nombre", editarDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)editarDto.Descripcion ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Editar @ProductoId, @Nombre, @Descripcion, @Resultado OUTPUT",
                productoIdParam, nombreParam, descripcionParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Activar @ProductoId, @Resultado OUTPUT",
                productoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Producto_Desactivar @ProductoId, @Resultado OUTPUT",
                productoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar producto ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<ProductoDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarActivos @Top", topParam)
                .ToListAsync();

            return productos.Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos activos");
            return new List<ProductoDto>();
        }
    }

    public async Task<ProductoDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            // Primero intentar buscar activo
            var productoIdParam = new SqlParameter("@ProductoId", id);
            var producto = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarActivosPorId @ProductoId", productoIdParam)
                .FirstOrDefaultAsync();

            // Si no está activo, buscar sin importar el estado
            if (producto == null)
            {
                producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            if (producto == null) return null;

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Activo = producto.Activo,
                FechaCreacion = producto.FechaCreacion,
                FechaModificacion = producto.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<ProductoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return productos.Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos por nombre: {Nombre}", nombre);
            return new List<ProductoDto>();
        }
    }

    public async Task<List<ProductoDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return productos.Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos inactivos");
            return new List<ProductoDto>();
        }
    }

    public async Task<List<ProductoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var productos = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return productos.Select(p => new ProductoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar productos inactivos por nombre: {Nombre}", nombre);
            return new List<ProductoDto>();
        }
    }

    public async Task<ProductoDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var productoIdParam = new SqlParameter("@ProductoId", id);

            var producto = await _context.Productos
                .FromSqlRaw("EXEC sp_Producto_MostrarInactivosPorId @ProductoId", productoIdParam)
                .FirstOrDefaultAsync();

            if (producto == null) return null;

            return new ProductoDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Activo = producto.Activo,
                FechaCreacion = producto.FechaCreacion,
                FechaModificacion = producto.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto inactivo ID: {Id}", id);
            return null;
        }
    }
}

