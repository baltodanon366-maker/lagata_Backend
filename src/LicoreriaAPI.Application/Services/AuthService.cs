using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.Auth;
using LicoreriaAPI.Infrastructure.Configuration;
using LicoreriaAPI.Infrastructure.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using LicoreriaAPI.Domain.Models;
using System.Security.Cryptography;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de autenticación (SQL Server)
/// </summary>
public class AuthService : IAuthService
{
    private readonly LicoreriaDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(LicoreriaDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
    {
        // Aquí se implementará la lógica de login cuando tengamos la tabla Usuario
        // Por ahora, retornamos null como placeholder
        
        // var usuario = await _context.Usuarios
        //     .FirstOrDefaultAsync(u => u.NombreUsuario == loginRequest.NombreUsuario);
        
        // if (usuario == null || !VerifyPassword(loginRequest.Password, usuario.PasswordHash))
        // {
        //     return null;
        // }

        // var token = await GenerateTokenAsync(usuario.NombreUsuario, usuario.Rol ?? "Usuario");
        
        // return new LoginResponseDto
        // {
        //     Token = token,
        //     Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
        //     NombreUsuario = usuario.NombreUsuario,
        //     Rol = usuario.Rol
        // };

        return null;
    }

    public Task<string> GenerateTokenAsync(string nombreUsuario, string rol)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, nombreUsuario),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

    private bool VerifyPassword(string password, string passwordHash)
    {
        // Implementar verificación de contraseña (usar BCrypt o similar)
        // Por ahora es un placeholder
        return false;
    }
}

