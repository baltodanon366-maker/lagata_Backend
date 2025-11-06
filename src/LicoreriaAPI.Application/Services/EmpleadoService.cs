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
/// Servicio de Empleados (SQL Server) - Usa Stored Procedures
/// </summary>
public class EmpleadoService : IEmpleadoService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<EmpleadoService> _logger;

    public EmpleadoService(LicoreriaDbContext context, ILogger<EmpleadoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmpleadoDto?> CrearAsync(CrearEmpleadoDto crearDto)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var usuarioIdParam = new SqlParameter("@UsuarioId", (object?)crearDto.UsuarioId ?? DBNull.Value);
            var codigoEmpleadoParam = new SqlParameter("@CodigoEmpleado", crearDto.CodigoEmpleado);
            var nombreCompletoParam = new SqlParameter("@NombreCompleto", crearDto.NombreCompleto);
            var telefonoParam = new SqlParameter("@Telefono", (object?)crearDto.Telefono ?? DBNull.Value);
            var emailParam = new SqlParameter("@Email", (object?)crearDto.Email ?? DBNull.Value);
            var direccionParam = new SqlParameter("@Direccion", (object?)crearDto.Direccion ?? DBNull.Value);
            var fechaNacimientoParam = new SqlParameter("@FechaNacimiento", (object?)crearDto.FechaNacimiento ?? DBNull.Value);
            var fechaIngresoParam = new SqlParameter("@FechaIngreso", crearDto.FechaIngreso);
            var salarioParam = new SqlParameter("@Salario", (object?)crearDto.Salario ?? DBNull.Value);
            var departamentoParam = new SqlParameter("@Departamento", (object?)crearDto.Departamento ?? DBNull.Value);
            var puestoParam = new SqlParameter("@Puesto", (object?)crearDto.Puesto ?? DBNull.Value);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Empleado_Crear @UsuarioId, @CodigoEmpleado, @NombreCompleto, @Telefono, @Email, @Direccion, @FechaNacimiento, @FechaIngreso, @Salario, @Departamento, @Puesto, @EmpleadoId OUTPUT",
                usuarioIdParam, codigoEmpleadoParam, nombreCompletoParam, telefonoParam, emailParam, direccionParam, fechaNacimientoParam, fechaIngresoParam, salarioParam, departamentoParam, puestoParam, empleadoIdParam);

            var empleadoId = (int)empleadoIdParam.Value!;

            if (empleadoId == -1)
            {
                return null; // Ya existe un empleado con ese código
            }

            return await MostrarActivosPorIdAsync(empleadoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empleado: {NombreCompleto}", crearDto.NombreCompleto);
            return null;
        }
    }

    public async Task<bool> EditarAsync(int id, EditarEmpleadoDto editarDto)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", id);
            var nombreCompletoParam = new SqlParameter("@NombreCompleto", editarDto.NombreCompleto);
            var telefonoParam = new SqlParameter("@Telefono", (object?)editarDto.Telefono ?? DBNull.Value);
            var emailParam = new SqlParameter("@Email", (object?)editarDto.Email ?? DBNull.Value);
            var direccionParam = new SqlParameter("@Direccion", (object?)editarDto.Direccion ?? DBNull.Value);
            var fechaNacimientoParam = new SqlParameter("@FechaNacimiento", (object?)editarDto.FechaNacimiento ?? DBNull.Value);
            var salarioParam = new SqlParameter("@Salario", (object?)editarDto.Salario ?? DBNull.Value);
            var departamentoParam = new SqlParameter("@Departamento", (object?)editarDto.Departamento ?? DBNull.Value);
            var puestoParam = new SqlParameter("@Puesto", (object?)editarDto.Puesto ?? DBNull.Value);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Empleado_Editar @EmpleadoId, @NombreCompleto, @Telefono, @Email, @Direccion, @FechaNacimiento, @Salario, @Departamento, @Puesto, @Resultado OUTPUT",
                empleadoIdParam, nombreCompletoParam, telefonoParam, emailParam, direccionParam, fechaNacimientoParam, salarioParam, departamentoParam, puestoParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al editar empleado ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivarAsync(int id)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Empleado_Activar @EmpleadoId, @Resultado OUTPUT",
                empleadoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar empleado ID: {Id}", id);
            return false;
        }
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", id);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Empleado_Desactivar @EmpleadoId, @Resultado OUTPUT",
                empleadoIdParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar empleado ID: {Id}", id);
            return false;
        }
    }

    public async Task<List<EmpleadoDto>> MostrarActivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var empleados = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarActivos @Top", topParam)
                .ToListAsync();

            return empleados.Select(e => new EmpleadoDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = e.NombreCompleto,
                Telefono = e.Telefono,
                Email = e.Email,
                Direccion = e.Direccion,
                FechaNacimiento = e.FechaNacimiento,
                FechaIngreso = e.FechaIngreso,
                Salario = e.Salario,
                Departamento = e.Departamento,
                Puesto = e.Puesto,
                Activo = e.Activo,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados activos");
            return new List<EmpleadoDto>();
        }
    }

    public async Task<EmpleadoDto?> MostrarActivosPorIdAsync(int id)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", id);

            var empleado = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarActivosPorId @EmpleadoId", empleadoIdParam)
                .FirstOrDefaultAsync();

            if (empleado == null) return null;

            return new EmpleadoDto
            {
                Id = empleado.Id,
                UsuarioId = empleado.UsuarioId,
                CodigoEmpleado = empleado.CodigoEmpleado,
                NombreCompleto = empleado.NombreCompleto,
                Telefono = empleado.Telefono,
                Email = empleado.Email,
                Direccion = empleado.Direccion,
                FechaNacimiento = empleado.FechaNacimiento,
                FechaIngreso = empleado.FechaIngreso,
                Salario = empleado.Salario,
                Departamento = empleado.Departamento,
                Puesto = empleado.Puesto,
                Activo = empleado.Activo,
                FechaCreacion = empleado.FechaCreacion,
                FechaModificacion = empleado.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleado ID: {Id}", id);
            return null;
        }
    }

    public async Task<List<EmpleadoDto>> MostrarActivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var empleados = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarActivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return empleados.Select(e => new EmpleadoDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = e.NombreCompleto,
                Telefono = e.Telefono,
                Email = e.Email,
                Direccion = e.Direccion,
                FechaNacimiento = e.FechaNacimiento,
                FechaIngreso = e.FechaIngreso,
                Salario = e.Salario,
                Departamento = e.Departamento,
                Puesto = e.Puesto,
                Activo = e.Activo,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empleados por nombre: {Nombre}", nombre);
            return new List<EmpleadoDto>();
        }
    }

    public async Task<List<EmpleadoDto>> MostrarInactivosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);

            var empleados = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarInactivos @Top", topParam)
                .ToListAsync();

            return empleados.Select(e => new EmpleadoDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = e.NombreCompleto,
                Telefono = e.Telefono,
                Email = e.Email,
                Direccion = e.Direccion,
                FechaNacimiento = e.FechaNacimiento,
                FechaIngreso = e.FechaIngreso,
                Salario = e.Salario,
                Departamento = e.Departamento,
                Puesto = e.Puesto,
                Activo = e.Activo,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados inactivos");
            return new List<EmpleadoDto>();
        }
    }

    public async Task<List<EmpleadoDto>> MostrarInactivosPorNombreAsync(string nombre, int top = 100)
    {
        try
        {
            var nombreParam = new SqlParameter("@Nombre", nombre);
            var topParam = new SqlParameter("@Top", top);

            var empleados = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarInactivosPorNombre @Nombre, @Top", nombreParam, topParam)
                .ToListAsync();

            return empleados.Select(e => new EmpleadoDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                CodigoEmpleado = e.CodigoEmpleado,
                NombreCompleto = e.NombreCompleto,
                Telefono = e.Telefono,
                Email = e.Email,
                Direccion = e.Direccion,
                FechaNacimiento = e.FechaNacimiento,
                FechaIngreso = e.FechaIngreso,
                Salario = e.Salario,
                Departamento = e.Departamento,
                Puesto = e.Puesto,
                Activo = e.Activo,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar empleados inactivos por nombre: {Nombre}", nombre);
            return new List<EmpleadoDto>();
        }
    }

    public async Task<EmpleadoDto?> MostrarInactivosPorIdAsync(int id)
    {
        try
        {
            var empleadoIdParam = new SqlParameter("@EmpleadoId", id);

            var empleado = await _context.Empleados
                .FromSqlRaw("EXEC sp_Empleado_MostrarInactivosPorId @EmpleadoId", empleadoIdParam)
                .FirstOrDefaultAsync();

            if (empleado == null) return null;

            return new EmpleadoDto
            {
                Id = empleado.Id,
                UsuarioId = empleado.UsuarioId,
                CodigoEmpleado = empleado.CodigoEmpleado,
                NombreCompleto = empleado.NombreCompleto,
                Telefono = empleado.Telefono,
                Email = empleado.Email,
                Direccion = empleado.Direccion,
                FechaNacimiento = empleado.FechaNacimiento,
                FechaIngreso = empleado.FechaIngreso,
                Salario = empleado.Salario,
                Departamento = empleado.Departamento,
                Puesto = empleado.Puesto,
                Activo = empleado.Activo,
                FechaCreacion = empleado.FechaCreacion,
                FechaModificacion = empleado.FechaModificacion
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleado inactivo ID: {Id}", id);
            return null;
        }
    }
}

