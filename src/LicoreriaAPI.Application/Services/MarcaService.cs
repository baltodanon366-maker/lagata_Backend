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
/// Servicio de Marcas (SQL Server) - Usa Stored Procedures
/// </summary>
public class MarcaService : IMarcaService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<MarcaService> _logger;

    public MarcaService(LicoreriaDbContext context, ILogger<MarcaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MarcaDto?> CrearAsync(CrearMarcaDto crearDto)
    {
        try
        {
            var marcaIdParam = new SqlParameter("@MarcaId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var nombreParam = new SqlParameter("@Nombre", crearDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)crearDto.Descripcion ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Marca_Crear @Nombre, @Descripcion, @MarcaId OUTPUT",
                nombreParam, descripcionParam, marcaIdParam);

            var marcaId = (int)marcaIdParam.Value!;

            if (marcaId == -1)
            {
                return null; // Ya existe una marca con ese nombre
            }

            return await MostrarActivosPorIdAsync(marcaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear marca: {Nombre}", crearDto.Nombre);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarMarcaDto editarDto)
    {
        try
        {
            var marcaIdParam = new SqlParameter("@MarcaId", id);
            var nombreParam = new SqlParameter("@Nombre", editarDto.Nombre);
            var descripcionParam = new SqlParameter("@Descripcion", (object?)editarDto.Descripcion ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Marca_Editar @MarcaId, @Nombre, @Descripcion, @Resultado OUTPUT",
                marcaIdParam, nombreParam, descripcionParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar marca ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var marcaIdParam = new SqlParameter("@MarcaId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Marca_Activar @MarcaId, @Resultado OUTPUT",
                marcaIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar marca ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var marcaIdParam = new SqlParameter("@MarcaId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Marca_Desactivar @MarcaId, @Resultado OUTPUT",
                marcaIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar marca ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<MarcaDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var marcas = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarActivos @Top", topParam)
                .ToListAsync();

            return marcas.Select(m => new MarcaDto
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
            _logger.LogError(ex, "Error al obtener marcas activas");
            return new List<MarcaDto>();
        }
    }

    public async Task<MarcaDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            // Primero intentar buscar activo
            var marcaIdParam = new SqlParameter("@MarcaId", id);
            var marca = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarActivosPorId @MarcaId", marcaIdParam)
                .FirstOrDefaultAsync();

            // Si no está activo, buscar sin importar el estado
            if (marca == null)
            {
                marca = await _context.Marcas
                    .FirstOrDefaultAsync(m => m.Id == id);
            }

            if (marca == null) return null;

            return new MarcaDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                Descripcion = marca.Descripcion,
                Activo = marca.Activo,
                FechaCreacion = marca.FechaCreacion,
                FechaModificacion = marca.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marca ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<MarcaDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var marcas = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return marcas.Select(m => new MarcaDto
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
            _logger.LogError(ex, "Error al buscar marcas por nombre: {Nombre}", nombre);
            return new List<MarcaDto>();
        }
    }

    public async Task<List<MarcaDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var marcas = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return marcas.Select(m => new MarcaDto
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
            _logger.LogError(ex, "Error al obtener marcas inactivas");
            return new List<MarcaDto>();
        }
    }

    public async Task<List<MarcaDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var marcas = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return marcas.Select(m => new MarcaDto
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
            _logger.LogError(ex, "Error al buscar marcas inactivas por nombre: {Nombre}", nombre);
            return new List<MarcaDto>();
        }
    }

    public async Task<MarcaDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var marcaIdParam = new SqlParameter("@MarcaId", id);

            var marca = await _context.Marcas
                .FromSqlRaw("EXEC sp_Marca_MostrarInactivosPorId @MarcaId", marcaIdParam)
                .FirstOrDefaultAsync();

            if (marca == null) return null;

            return new MarcaDto
            {
                Id = marca.Id,
                Nombre = marca.Nombre,
                Descripcion = marca.Descripcion,
                Activo = marca.Activo,
                FechaCreacion = marca.FechaCreacion,
                FechaModificacion = marca.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener marca inactiva ID: {Id}", id);
            return null;
        }
    }
}

