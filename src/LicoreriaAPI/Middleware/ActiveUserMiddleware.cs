using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using System.Security.Claims;

namespace LicoreriaAPI.Middleware;

/// <summary>
/// Middleware para rastrear usuarios activos
/// </summary>
public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActiveUserMiddleware> _logger;
    private readonly IActiveUserMetricRepository _repository;

    public ActiveUserMiddleware(
        RequestDelegate next,
        ILogger<ActiveUserMiddleware> logger,
        IActiveUserMetricRepository repository)
    {
        _next = next;
        _logger = logger;
        _repository = repository;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Excluir rutas de salud y swagger
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/_framework") ||
            context.Request.Path.StartsWithSegments("/favicon.ico"))
        {
            await _next(context);
            return;
        }

        await _next(context);

        // Registrar usuario activo de forma asíncrona
        _ = Task.Run(async () =>
        {
            try
            {
                var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    var username = context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                    var role = context.User?.FindFirst(ClaimTypes.Role)?.Value;

                    var metric = new ActiveUserMetric
                    {
                        UserId = userId,
                        Username = username,
                        Role = role,
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        LastActivity = DateTime.UtcNow,
                        SessionStart = DateTime.UtcNow, // Se actualizará si ya existe
                        RequestCount = 1,
                        IsActive = true,
                        Timestamp = DateTime.UtcNow
                    };

                    await _repository.AddOrUpdateAsync(metric);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario activo");
            }
        });
    }
}

