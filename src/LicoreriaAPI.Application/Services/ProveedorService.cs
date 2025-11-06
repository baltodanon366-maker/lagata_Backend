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
/// Servicio de Proveedores (SQL Server) - Usa Stored Procedures
/// </summary>
public class ProveedorService : IProveedorService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<ProveedorService> _logger;

    public ProveedorService(LicoreriaDbContext context, ILogger<ProveedorService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProveedorDto?> CrearAsync(CrearProveedorDto crearDto)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var codigoProveedorParam = new SqlParameter("@CodigoProveedor", crearDto.CodigoProveedor);
            var nombreParam = new SqlParameter("@Nombre", crearDto.Nombre);
            var razonSocialParam = new SqlParameter("@RazonSocial", (object?)crearDto.RazonSocial ?? DBNull.Value);
            var rfcParam = new SqlParameter("@RFC", (object?)crearDto.RFC ?? DBNull.Value);
            var direccionParam = new SqlParameter("@Direccion", (object?)crearDto.Direccion ?? DBNull.Value);
            var telefonoParam = new SqlParameter("@Telefono", (object?)crearDto.Telefono ?? DBNull.Value);
            var emailParam = new SqlParameter("@Email", (object?)crearDto.Email ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Proveedor_Crear @CodigoProveedor, @Nombre, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, @ProveedorId OUTPUT",
                codigoProveedorParam, nombreParam, razonSocialParam, rfcParam, direccionParam, telefonoParam, emailParam, proveedorIdParam);

            var proveedorId = (int)proveedorIdParam.Value!;

            if (proveedorId == -1)
            {
                return null; // Ya existe un proveedor con ese código
            }

            return await MostrarActivosPorIdAsync(proveedorId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor: {Nombre}", crearDto.Nombre);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarProveedorDto editarDto)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", id);
            var nombreParam = new SqlParameter("@Nombre", editarDto.Nombre);
            var razonSocialParam = new SqlParameter("@RazonSocial", (object?)editarDto.RazonSocial ?? DBNull.Value);
            var rfcParam = new SqlParameter("@RFC", (object?)editarDto.RFC ?? DBNull.Value);
            var direccionParam = new SqlParameter("@Direccion", (object?)editarDto.Direccion ?? DBNull.Value);
            var telefonoParam = new SqlParameter("@Telefono", (object?)editarDto.Telefono ?? DBNull.Value);
            var emailParam = new SqlParameter("@Email", (object?)editarDto.Email ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Proveedor_Editar @ProveedorId, @Nombre, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, @Resultado OUTPUT",
                proveedorIdParam, nombreParam, razonSocialParam, rfcParam, direccionParam, telefonoParam, emailParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar proveedor ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Proveedor_Activar @ProveedorId, @Resultado OUTPUT",
                proveedorIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar proveedor ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Proveedor_Desactivar @ProveedorId, @Resultado OUTPUT",
                proveedorIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar proveedor ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<ProveedorDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var proveedores = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarActivos @Top", topParam)
                .ToListAsync();

            return proveedores.Select(p => new ProveedorDto
            {
                Id = p.Id,
                CodigoProveedor = p.CodigoProveedor,
                Nombre = p.Nombre,
                RazonSocial = p.RazonSocial,
                RFC = p.RFC,
                Direccion = p.Direccion,
                Telefono = p.Telefono,
                Email = p.Email,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores activos");
            return new List<ProveedorDto>();
        }
    }

    public async Task<ProveedorDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", id);

            var proveedor = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarActivosPorId @ProveedorId", proveedorIdParam)
                .FirstOrDefaultAsync();

            if (proveedor == null) return null;

            return new ProveedorDto
            {
                Id = proveedor.Id,
                CodigoProveedor = proveedor.CodigoProveedor,
                Nombre = proveedor.Nombre,
                RazonSocial = proveedor.RazonSocial,
                RFC = proveedor.RFC,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Activo = proveedor.Activo,
                FechaCreacion = proveedor.FechaCreacion,
                FechaModificacion = proveedor.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<ProveedorDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var proveedores = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return proveedores.Select(p => new ProveedorDto
            {
                Id = p.Id,
                CodigoProveedor = p.CodigoProveedor,
                Nombre = p.Nombre,
                RazonSocial = p.RazonSocial,
                RFC = p.RFC,
                Direccion = p.Direccion,
                Telefono = p.Telefono,
                Email = p.Email,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedores por nombre: {Nombre}", nombre);
            return new List<ProveedorDto>();
        }
    }

    public async Task<List<ProveedorDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var proveedores = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return proveedores.Select(p => new ProveedorDto
            {
                Id = p.Id,
                CodigoProveedor = p.CodigoProveedor,
                Nombre = p.Nombre,
                RazonSocial = p.RazonSocial,
                RFC = p.RFC,
                Direccion = p.Direccion,
                Telefono = p.Telefono,
                Email = p.Email,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedores inactivos");
            return new List<ProveedorDto>();
        }
    }

    public async Task<List<ProveedorDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var proveedores = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return proveedores.Select(p => new ProveedorDto
            {
                Id = p.Id,
                CodigoProveedor = p.CodigoProveedor,
                Nombre = p.Nombre,
                RazonSocial = p.RazonSocial,
                RFC = p.RFC,
                Direccion = p.Direccion,
                Telefono = p.Telefono,
                Email = p.Email,
                Activo = p.Activo,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedores inactivos por nombre: {Nombre}", nombre);
            return new List<ProveedorDto>();
        }
    }

    public async Task<ProveedorDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var proveedorIdParam = new SqlParameter("@ProveedorId", id);

            var proveedor = await _context.Proveedores
                .FromSqlRaw("EXEC sp_Proveedor_MostrarInactivosPorId @ProveedorId", proveedorIdParam)
                .FirstOrDefaultAsync();

            if (proveedor == null) return null;

            return new ProveedorDto
            {
                Id = proveedor.Id,
                CodigoProveedor = proveedor.CodigoProveedor,
                Nombre = proveedor.Nombre,
                RazonSocial = proveedor.RazonSocial,
                RFC = proveedor.RFC,
                Direccion = proveedor.Direccion,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                Activo = proveedor.Activo,
                FechaCreacion = proveedor.FechaCreacion,
                FechaModificacion = proveedor.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor inactivo ID: {Id}", id);
            return null;
        }
    }
}

