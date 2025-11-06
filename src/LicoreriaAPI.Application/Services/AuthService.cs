using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Auth;
using LicoreriaAPI.Domain.Models;
using LicoreriaAPI.Infrastructure.Configuration;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BCrypt.Net;
using System.Data;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de autenticación (SQL Server) - Usa Stored Procedures
/// </summary>
public class AuthService : IAuthService
{
    private readonly LicoreriaDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        LicoreriaDbContext context, 
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            // Obtener el usuario para verificar el hash
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == loginRequest.NombreUsuario && u.Activo);

            if (usuario == null)
            {
                return null;
            }

            // Verificar contraseña usando BCrypt
            if (!VerifyPassword(loginRequest.Password, usuario.PasswordHash))
            {
                return null;
            }

            // Llamar al stored procedure para actualizar último acceso
            // El stored procedure espera el password hash para validación, pero ya verificamos arriba
            var nombreUsuarioParam = new SqlParameter("@NombreUsuario", loginRequest.NombreUsuario);
            var passwordHashParam = new SqlParameter("@PasswordHash", usuario.PasswordHash);

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Usuario_Login @NombreUsuario, @PasswordHash",
                nombreUsuarioParam, passwordHashParam);

            // Generar token JWT con el ID del usuario en los claims
            var token = await GenerateTokenAsync(usuario.Id, usuario.NombreUsuario, usuario.Rol ?? "Usuario");
            
            return new LoginResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                NombreUsuario = usuario.NombreUsuario,
                Rol = usuario.Rol
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoginAsync para usuario: {NombreUsuario}", loginRequest.NombreUsuario);
            return null;
        }
    }

    public async Task<RegistroResponseDto?> RegistrarAsync(RegistroRequestDto registroRequest)
    {
        try
        {
            // Hash de la contraseña
            var passwordHash = HashPassword(registroRequest.Password);

            // Parámetros para el stored procedure
            var nombreUsuarioParam = new SqlParameter("@NombreUsuario", registroRequest.NombreUsuario);
            var emailParam = new SqlParameter("@Email", registroRequest.Email);
            var passwordHashParam = new SqlParameter("@PasswordHash", passwordHash);
            var nombreCompletoParam = new SqlParameter("@NombreCompleto", (object?)registroRequest.NombreCompleto ?? DBNull.Value);
            var rolParam = new SqlParameter("@Rol", registroRequest.Rol ?? "Vendedor");
            var usuarioIdParam = new SqlParameter("@UsuarioId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            // Ejecutar stored procedure
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Usuario_Registrar @NombreUsuario, @Email, @PasswordHash, @NombreCompleto, @Rol, @UsuarioId OUTPUT",
                nombreUsuarioParam, emailParam, passwordHashParam, nombreCompletoParam, rolParam, usuarioIdParam);

            var usuarioId = (int)usuarioIdParam.Value!;

            // Si el usuarioId es -1, significa que el usuario ya existe
            if (usuarioId == -1)
            {
                return null;
            }

            return new RegistroResponseDto
            {
                UsuarioId = usuarioId,
                NombreUsuario = registroRequest.NombreUsuario,
                Email = registroRequest.Email,
                NombreCompleto = registroRequest.NombreCompleto,
                Rol = registroRequest.Rol
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en RegistrarAsync para usuario: {NombreUsuario}", registroRequest.NombreUsuario);
            return null;
        }
    }

    public async Task<bool> ActualizarPasswordAsync(int usuarioId, ActualizarPasswordDto actualizarPasswordDto)
    {
        try
        {
            // Obtener el usuario actual para verificar la contraseña anterior
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return false;
            }

            // Verificar contraseña actual
            if (!VerifyPassword(actualizarPasswordDto.PasswordActual, usuario.PasswordHash))
            {
                return false;
            }

            // Hash de la nueva contraseña
            var passwordHashNuevo = HashPassword(actualizarPasswordDto.PasswordNuevo);

            // Parámetros para el stored procedure
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);
            var passwordHashAnteriorParam = new SqlParameter("@PasswordHashAnterior", usuario.PasswordHash);
            var passwordHashNuevoParam = new SqlParameter("@PasswordHashNuevo", passwordHashNuevo);
            var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit)
            {
                Direction = ParameterDirection.Output
            };

            // Ejecutar stored procedure
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_Usuario_ActualizarPassword @UsuarioId, @PasswordHashAnterior, @PasswordHashNuevo, @Resultado OUTPUT",
                usuarioIdParam, passwordHashAnteriorParam, passwordHashNuevoParam, resultadoParam);

            return (bool)resultadoParam.Value!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarPasswordAsync para usuarioId: {UsuarioId}", usuarioId);
            return false;
        }
    }

    public async Task<List<string>> ObtenerPermisosAsync(int usuarioId)
    {
        try
        {
            var usuarioIdParam = new SqlParameter("@UsuarioId", usuarioId);

            // Usar FromSqlRaw con Permisos - el stored procedure devuelve Id, Nombre, Descripcion, Modulo
            var permisos = await _context.Permisos
                .FromSqlRaw("EXEC sp_Usuario_ObtenerPermisos @UsuarioId", usuarioIdParam)
                .ToListAsync();

            return permisos.Select(p => p.Nombre).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ObtenerPermisosAsync para usuarioId: {UsuarioId}", usuarioId);
            return new List<string>();
        }
    }

    public async Task<Usuario?> GetUsuarioByNombreAsync(string nombreUsuario)
    {
        try
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && u.Activo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetUsuarioByNombreAsync para usuario: {NombreUsuario}", nombreUsuario);
            return null;
        }
    }

    public Task<string> GenerateTokenAsync(int usuarioId, string nombreUsuario, string rol)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Name, nombreUsuario),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, nombreUsuario)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    // Método sobrecargado para compatibilidad
    public Task<string> GenerateTokenAsync(string nombreUsuario, string rol)
    {
        // Este método no debería usarse directamente, pero lo mantenemos por compatibilidad
        // Si no tenemos el ID, usamos 0 temporalmente
        return GenerateTokenAsync(0, nombreUsuario, rol);
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}

