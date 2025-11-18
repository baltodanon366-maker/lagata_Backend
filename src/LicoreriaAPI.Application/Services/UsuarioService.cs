using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Auth;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de gestión de usuarios (solo admin)
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly LicoreriaDbContext _context;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(LicoreriaDbContext context, ILogger<UsuarioService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UsuarioDto>> MostrarTodosAsync(int top = 100)
    {
        try
        {
            var topParam = new SqlParameter("@Top", top);
            
            var usuarios = await _context.Database
                .SqlQueryRaw<UsuarioDto>(
                    "EXEC sp_Usuario_MostrarTodos @Top",
                    topParam)
                .ToListAsync();

            _logger.LogInformation("Se obtuvieron {Count} usuarios", usuarios.Count);
            return usuarios;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return new List<UsuarioDto>();
        }
    }

    public async Task<bool> AsignarRolAsync(int usuarioId, int rolId)
    {
        try
        {
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);
            var rolIdParam = new SqlParameter("@RolId", rolId);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Usuario_AsignarRol @UsuarioId, @RolId, @Resultado OUTPUT",
                usuarioIdParam, rolIdParam, resultadoParam);

            var resultado = (bool)resultadoParam.Value!;
            
            if (resultado)
            {
                _logger.LogInformation("Rol {RolId} asignado exitosamente al usuario {UsuarioId}", rolId, usuarioId);
            }
            else
            {
                _logger.LogWarning("No se pudo asignar el rol {RolId} al usuario {UsuarioId}. Verifica que ambos existan.", rolId, usuarioId);
            }
            
            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar rol al usuario {UsuarioId}", usuarioId);
            return false;
        }
    }

    public async Task<List<RolDto>> MostrarRolesAsync()
    {
        try
        {
            var roles = await _context.Database
                .SqlQueryRaw<RolDto>("EXEC sp_Rol_MostrarTodos")
                .ToListAsync();

            _logger.LogInformation("Se obtuvieron {Count} roles", roles.Count);
            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles");
            return new List<RolDto>();
        }
    }
}

