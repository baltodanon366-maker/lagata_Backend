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
/// Servicio de Categorías (SQL Server) - Usa Stored Procedures
/// </summary>
public class CategoriaService : ICategoriaService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<CategoriaService> _logger;

    public CategoriaService(LicoreriaDbContext context, ILogger<CategoriaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CategoriaDto?> CrearAsync(CrearCategoriaDto crearDto)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var nombreParam = new SqlParameter("@Nombre", crearDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)crearDto.Descripcion ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Categoria_Crear @Nombre, @Descripcion, @CategoriaId OUTPUT",
                nombreParam, descripcionParam, categoriaIdParam);

            var categoriaId = (int)categoriaIdParam.Value!;

            if (categoriaId == -1)
            {
                return null; // Ya existe una categoría con ese nombre
            }

            return await MostrarActivosPorIdAsync(categoriaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría: {Nombre}", crearDto.Nombre);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarCategoriaDto editarDto)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", id);
            var nombreParam = new SqlParameter("@Nombre", editarDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)editarDto.Descripcion ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Categoria_Editar @CategoriaId, @Nombre, @Descripcion, @Resultado OUTPUT",
                categoriaIdParam, nombreParam, descripcionParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar categoría ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Categoria_Activar @CategoriaId, @Resultado OUTPUT",
                categoriaIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar categoría ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Categoria_Desactivar @CategoriaId, @Resultado OUTPUT",
                categoriaIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar categoría ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<CategoriaDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarActivos @Top", topParam)
                .ToListAsync();

            return categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías activas");
            return new List<CategoriaDto>();
        }
    }

    public async Task<CategoriaDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", id);

            var categoria = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarActivosPorId @CategoriaId", categoriaIdParam)
                .FirstOrDefaultAsync();

            if (categoria == null) return null;

            return new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaCreacion = categoria.FechaCreacion,
                FechaModificacion = categoria.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<CategoriaDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar categorías por nombre: {Nombre}", nombre);
            return new List<CategoriaDto>();
        }
    }

    public async Task<List<CategoriaDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías inactivas");
            return new List<CategoriaDto>();
        }
    }

    public async Task<List<CategoriaDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var categorias = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return categorias.Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar categorías inactivas por nombre: {Nombre}", nombre);
            return new List<CategoriaDto>();
        }
    }

    public async Task<CategoriaDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var categoriaIdParam = new SqlParameter("@CategoriaId", id);

            var categoria = await _context.Categorias
                .FromSqlRaw("EXEC sp_Categoria_MostrarInactivosPorId @CategoriaId", categoriaIdParam)
                .FirstOrDefaultAsync();

            if (categoria == null) return null;

            return new CategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Activo = categoria.Activo,
                FechaCreacion = categoria.FechaCreacion,
                FechaModificacion = categoria.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categoría inactiva ID: {Id}", id);
            return null;
        }
    }
}

