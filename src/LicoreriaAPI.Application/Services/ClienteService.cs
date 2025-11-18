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
/// Servicio de Clientes (SQL Server) - Usa Stored Procedures
/// </summary>
public class ClienteService : IClienteService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(LicoreriaDbContext context, ILogger<ClienteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ClienteDto?> CrearAsync(CrearClienteDto crearDto)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var codigoClienteParam = new SqlParameter("@CodigoCliente", crearDto.CodigoCliente);
            var nombreCompletoParam = new SqlParameter("@NombreCompleto", crearDto.NombreCompleto);
            var razonSocialParam = new SqlParameter("@RazonSocial", (object?)crearDto.RazonSocial ?? DBNull.Value);
            var rfcParam = new SqlParameter("@RFC", (object?)crearDto.RFC ?? DBNull.Value);
            var direccionParam = new SqlParameter("@Direccion", (object?)crearDto.Direccion ?? DBNull.Value);
            var telefonoParam = new SqlParameter("@Telefono", (object?)crearDto.Telefono ?? DBNull.Value);
            var emailParam = new SqlParameter("@Email", (object?)crearDto.Email ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Cliente_Crear @CodigoCliente, @NombreCompleto, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, @ClienteId OUTPUT",
                codigoClienteParam, nombreCompletoParam, razonSocialParam, rfcParam, direccionParam, telefonoParam, emailParam, clienteIdParam);

            var clienteId = (int)clienteIdParam.Value!;

            if (clienteId == -1)
            {
                return null; // Ya existe un cliente con ese código
            }

            return await MostrarActivosPorIdAsync(clienteId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente: {NombreCompleto}", crearDto.NombreCompleto);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarClienteDto editarDto)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", id);
            var nombreCompletoParam = new SqlParameter("@NombreCompleto", editarDto.NombreCompleto);
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
                "EXEC sp_Cliente_Editar @ClienteId, @NombreCompleto, @RazonSocial, @RFC, @Direccion, @Telefono, @Email, @Resultado OUTPUT",
                clienteIdParam, nombreCompletoParam, razonSocialParam, rfcParam, direccionParam, telefonoParam, emailParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar cliente ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Cliente_Activar @ClienteId, @Resultado OUTPUT",
                clienteIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar cliente ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Cliente_Desactivar @ClienteId, @Resultado OUTPUT",
                clienteIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar cliente ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<ClienteDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var clientes = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarActivos @Top", topParam)
                .ToListAsync();

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                CodigoCliente = c.CodigoCliente,
                NombreCompleto = c.NombreCompleto,
                RazonSocial = c.RazonSocial,
                RFC = c.RFC,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes activos");
            return new List<ClienteDto>();
        }
    }

    public async Task<ClienteDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            // Primero intentar buscar activo
            var clienteIdParam = new SqlParameter("@ClienteId", id);
            var cliente = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarActivosPorId @ClienteId", clienteIdParam)
                .FirstOrDefaultAsync();

            // Si no está activo, buscar sin importar el estado
            if (cliente == null)
            {
                cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            if (cliente == null) return null;

            return new ClienteDto
            {
                Id = cliente.Id,
                CodigoCliente = cliente.CodigoCliente,
                NombreCompleto = cliente.NombreCompleto,
                RazonSocial = cliente.RazonSocial,
                RFC = cliente.RFC,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.Activo,
                FechaCreacion = cliente.FechaCreacion,
                FechaModificacion = cliente.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<ClienteDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var clientes = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                CodigoCliente = c.CodigoCliente,
                NombreCompleto = c.NombreCompleto,
                RazonSocial = c.RazonSocial,
                RFC = c.RFC,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes por nombre: {Nombre}", nombre);
            return new List<ClienteDto>();
        }
    }

    public async Task<List<ClienteDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var clientes = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                CodigoCliente = c.CodigoCliente,
                NombreCompleto = c.NombreCompleto,
                RazonSocial = c.RazonSocial,
                RFC = c.RFC,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener clientes inactivos");
            return new List<ClienteDto>();
        }
    }

    public async Task<List<ClienteDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var clientes = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return clientes.Select(c => new ClienteDto
            {
                Id = c.Id,
                CodigoCliente = c.CodigoCliente,
                NombreCompleto = c.NombreCompleto,
                RazonSocial = c.RazonSocial,
                RFC = c.RFC,
                Direccion = c.Direccion,
                Telefono = c.Telefono,
                Email = c.Email,
                Activo = c.Activo,
                FechaCreacion = c.FechaCreacion,
                FechaModificacion = c.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes inactivos por nombre: {Nombre}", nombre);
            return new List<ClienteDto>();
        }
    }

    public async Task<ClienteDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var clienteIdParam = new SqlParameter("@ClienteId", id);

            var cliente = await _context.Clientes
                .FromSqlRaw("EXEC sp_Cliente_MostrarInactivosPorId @ClienteId", clienteIdParam)
                .FirstOrDefaultAsync();

            if (cliente == null) return null;

            return new ClienteDto
            {
                Id = cliente.Id,
                CodigoCliente = cliente.CodigoCliente,
                NombreCompleto = cliente.NombreCompleto,
                RazonSocial = cliente.RazonSocial,
                RFC = cliente.RFC,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Activo = cliente.Activo,
                FechaCreacion = cliente.FechaCreacion,
                FechaModificacion = cliente.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente inactivo ID: {Id}", id);
            return null;
        }
    }
}

