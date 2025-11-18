using System.Diagnostics;
using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;

namespace LicoreriaAPI.Middleware;

/// <summary>
/// Middleware para capturar métricas de uso de red (bytes enviados/recibidos)
/// </summary>
public class NetworkUsageMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NetworkUsageMiddleware> _logger;
    private readonly INetworkUsageMetricRepository _repository;

    public NetworkUsageMiddleware(
        RequestDelegate next,
        ILogger<NetworkUsageMiddleware> logger,
        INetworkUsageMetricRepository repository)
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

        var stopwatch = Stopwatch.StartNew();
        var initialBytesReceived = context.Request.ContentLength ?? 0;

        // Interceptar la respuesta para medir bytes enviados
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Calcular bytes enviados
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBytes = responseBody.ToArray();
            var bytesSent = responseBytes.Length;

            // Copiar al stream original
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);

            // Obtener bytes recibidos del request
            var bytesReceived = initialBytesReceived;
            if (context.Request.HasFormContentType && context.Request.Form != null)
            {
                // Si hay form data, usar ContentLength o 0
                bytesReceived = context.Request.ContentLength ?? 0L;
            }

            // Registrar métrica de forma asíncrona para no bloquear la respuesta
            _ = Task.Run(async () =>
            {
                try
                {
                    var metric = new NetworkUsageMetric
                    {
                        Path = context.Request.Path.ToString(),
                        Method = context.Request.Method,
                        BytesSent = bytesSent,
                        BytesReceived = bytesReceived,
                        TotalBytes = bytesSent + bytesReceived,
                        ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.Request.Headers["User-Agent"].ToString(),
                        StatusCode = context.Response.StatusCode,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        Timestamp = DateTime.UtcNow
                    };

                    await _repository.AddAsync(metric);
                }
                catch (Exception ex)
                {
                    // Log error pero no fallar la aplicación
                    _logger.LogWarning(ex, "No se pudo registrar métrica de uso de red (MongoDB puede no estar disponible)");
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error en NetworkUsageMiddleware");
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

