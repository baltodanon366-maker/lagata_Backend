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
/// Servicio de Modelos (SQL Server) - Usa Stored Procedures
/// </summary>
public class ModeloService : IModeloService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<ModeloService> _logger;

    public ModeloService(LicoreriaDbContext context, ILogger<ModeloService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ModeloDto?> CrearAsync(CrearModeloDto crearDto)
    {
        try
        {
            var modeloIdParam = new SqlParameter("@ModeloId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var nombreParam = new SqlParameter("@Nombre", crearDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)crearDto.Descripcion ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Modelo_Crear @Nombre, @Descripcion, @ModeloId OUTPUT",
                nombreParam, descripcionParam, modeloIdParam);

            var modeloId = (int)modeloIdParam.Value!;

            if (modeloId == -1)
            {
                return null; // Ya existe un modelo con ese nombre
            }

            return await MostrarActivosPorIdAsync(modeloId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear modelo: {Nombre}", crearDto.Nombre);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarModeloDto editarDto)
    {
        try
        {
            var modeloIdParam = new SqlParameter("@ModeloId", id);
            var nombreParam = new SqlParameter("@Nombre", editarDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)editarDto.Descripcion ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Modelo_Editar @ModeloId, @Nombre, @Descripcion, @Resultado OUTPUT",
                modeloIdParam, nombreParam, descripcionParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar modelo ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var modeloIdParam = new SqlParameter("@ModeloId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Modelo_Activar @ModeloId, @Resultado OUTPUT",
                modeloIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar modelo ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var modeloIdParam = new SqlParameter("@ModeloId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Modelo_Desactivar @ModeloId, @Resultado OUTPUT",
                modeloIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar modelo ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<ModeloDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var modelos = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarActivos @Top", topParam)
                .ToListAsync();

            return modelos.Select(m => new ModeloDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Activo = m.Activo,
                FechaCreacion = m.FechaCreacion,
                FechaModificacion = m.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelos activos");
            return new List<ModeloDto>();
        }
    }

    public async Task<ModeloDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            // Primero intentar buscar activo
            var modeloIdParam = new SqlParameter("@ModeloId", id);
            var modelo = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarActivosPorId @ModeloId", modeloIdParam)
                .FirstOrDefaultAsync();

            // Si no está activo, buscar sin importar el estado
            if (modelo == null)
            {
                modelo = await _context.Modelos
                    .FirstOrDefaultAsync(m => m.Id == id);
            }

            if (modelo == null) return null;

            return new ModeloDto
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Activo = modelo.Activo,
                FechaCreacion = modelo.FechaCreacion,
                FechaModificacion = modelo.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelo ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<ModeloDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var modelos = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return modelos.Select(m => new ModeloDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Activo = m.Activo,
                FechaCreacion = m.FechaCreacion,
                FechaModificacion = m.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar modelos por nombre: {Nombre}", nombre);
            return new List<ModeloDto>();
        }
    }

    public async Task<List<ModeloDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var modelos = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return modelos.Select(m => new ModeloDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Activo = m.Activo,
                FechaCreacion = m.FechaCreacion,
                FechaModificacion = m.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelos inactivos");
            return new List<ModeloDto>();
        }
    }

    public async Task<List<ModeloDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var modelos = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return modelos.Select(m => new ModeloDto
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                Activo = m.Activo,
                FechaCreacion = m.FechaCreacion,
                FechaModificacion = m.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar modelos inactivos por nombre: {Nombre}", nombre);
            return new List<ModeloDto>();
        }
    }

    public async Task<ModeloDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var modeloIdParam = new SqlParameter("@ModeloId", id);

            var modelo = await _context.Modelos
                .FromSqlRaw("EXEC sp_Modelo_MostrarInactivosPorId @ModeloId", modeloIdParam)
                .FirstOrDefaultAsync();

            if (modelo == null) return null;

            return new ModeloDto
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                Descripcion = modelo.Descripcion,
                Activo = modelo.Activo,
                FechaCreacion = modelo.FechaCreacion,
                FechaModificacion = modelo.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener modelo inactivo ID: {Id}", id);
            return null;
        }
    }
}

