using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LicoreriaAPI.Domain.Interfaces;

namespace LicoreriaAPI.Controllers;

/// <summary>
/// Controlador para consultar las 5 métricas almacenadas en MongoDB Atlas
/// </summary>
/// <remarks>
/// Este controlador expone las 5 métricas implementadas según los requisitos de la tarea:
/// 1. Uso de red (bytes enviados/recibidos)
/// 2. Intentos fallidos de inicio de sesión
/// 3. Consultas lentas (>100ms)
/// 4. Usuarios activos
/// 5. Transacciones por tipo
/// 
/// Todas las métricas se almacenan automáticamente en MongoDB Atlas y se pueden consultar mediante estos endpoints.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Tags("📊 Métricas MongoDB")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly INetworkUsageMetricRepository _networkRepository;
    private readonly IFailedLoginAttemptRepository _failedLoginRepository;
    private readonly ISlowQueryMetricRepository _slowQueryRepository;
    private readonly IActiveUserMetricRepository _activeUserRepository;
    private readonly ITransactionMetricRepository _transactionRepository;

    public MetricsController(
        INetworkUsageMetricRepository networkRepository,
        IFailedLoginAttemptRepository failedLoginRepository,
        ISlowQueryMetricRepository slowQueryRepository,
        IActiveUserMetricRepository activeUserRepository,
        ITransactionMetricRepository transactionRepository)
    {
        _networkRepository = networkRepository;
        _failedLoginRepository = failedLoginRepository;
        _slowQueryRepository = slowQueryRepository;
        _activeUserRepository = activeUserRepository;
        _transactionRepository = transactionRepository;
    }

    /// <summary>
    /// Obtiene métricas de uso de red (bytes enviados/recibidos)
    /// </summary>
    /// <remarks>
    /// Retorna las métricas de uso de red capturadas automáticamente por el middleware.
    /// Cada petición HTTP registra los bytes enviados y recibidos.
    /// </remarks>
    [HttpGet("network")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNetworkMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int limit = 100)
    {
        var metrics = await _networkRepository.GetMetricsAsync(startDate, endDate, limit);
        return Ok(metrics);
    }

    /// <summary>
    /// Obtiene el total de bytes por período
    /// </summary>
    [HttpGet("network/total")]
    public async Task<IActionResult> GetNetworkTotal([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var total = await _networkRepository.GetTotalBytesByPeriodAsync(startDate, endDate);
        return Ok(total);
    }

    /// <summary>
    /// Obtiene intentos fallidos de inicio de sesión
    /// </summary>
    /// <remarks>
    /// Retorna los intentos fallidos de login capturados automáticamente por el AuthService.
    /// Incluye información sobre IP, usuario, razón del fallo y si es sospechoso.
    /// </remarks>
    [HttpGet("failed-logins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFailedLogins(
        [FromQuery] string? ipAddress,
        [FromQuery] string? username,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int limit = 100)
    {
        var attempts = await _failedLoginRepository.GetAttemptsAsync(ipAddress, username, startDate, endDate, limit);
        return Ok(attempts);
    }

    /// <summary>
    /// Obtiene intentos sospechosos de login
    /// </summary>
    [HttpGet("failed-logins/suspicious")]
    public async Task<IActionResult> GetSuspiciousLogins([FromQuery] DateTime? startDate, [FromQuery] int limit = 100)
    {
        var attempts = await _failedLoginRepository.GetSuspiciousAttemptsAsync(startDate, limit);
        return Ok(attempts);
    }

    /// <summary>
    /// Obtiene consultas lentas (>100ms)
    /// </summary>
    /// <remarks>
    /// Retorna las consultas SQL que tardaron más de 100ms, capturadas automáticamente por el interceptor de EF Core.
    /// Útil para identificar problemas de rendimiento en la base de datos.
    /// </remarks>
    [HttpGet("slow-queries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSlowQueries(
        [FromQuery] string? tableName,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int limit = 100)
    {
        var queries = await _slowQueryRepository.GetSlowQueriesAsync(tableName, startDate, endDate, limit);
        return Ok(queries);
    }

    /// <summary>
    /// Obtiene las consultas más lentas
    /// </summary>
    [HttpGet("slow-queries/slowest")]
    public async Task<IActionResult> GetSlowestQueries([FromQuery] int limit = 10)
    {
        var queries = await _slowQueryRepository.GetSlowestQueriesAsync(limit);
        return Ok(queries);
    }

    /// <summary>
    /// Obtiene usuarios activos
    /// </summary>
    /// <remarks>
    /// Retorna los usuarios que han realizado peticiones recientemente, capturados automáticamente por el middleware.
    /// Útil para análisis de uso y comportamiento de usuarios.
    /// </remarks>
    [HttpGet("active-users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveUsers([FromQuery] DateTime? since)
    {
        var users = await _activeUserRepository.GetActiveUsersAsync(since);
        return Ok(users);
    }

    /// <summary>
    /// Obtiene el conteo de usuarios activos
    /// </summary>
    [HttpGet("active-users/count")]
    public async Task<IActionResult> GetActiveUserCount([FromQuery] DateTime? since)
    {
        var count = await _activeUserRepository.GetActiveUserCountAsync(since);
        return Ok(new { count });
    }

    /// <summary>
    /// Obtiene métricas de transacciones por tipo
    /// </summary>
    /// <remarks>
    /// Retorna las métricas de transacciones (Venta, Compra, Devolución) registradas en el sistema.
    /// Incluye información sobre monto, tipo, estado y duración de cada transacción.
    /// </remarks>
    [HttpGet("transactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? transactionType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int limit = 100)
    {
        var transactions = await _transactionRepository.GetTransactionsAsync(transactionType, startDate, endDate, limit);
        return Ok(transactions);
    }

    /// <summary>
    /// Obtiene conteo de transacciones por tipo
    /// </summary>
    [HttpGet("transactions/count-by-type")]
    public async Task<IActionResult> GetTransactionCountByType(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var counts = await _transactionRepository.GetTransactionCountByTypeAsync(startDate, endDate);
        return Ok(counts);
    }

    /// <summary>
    /// Obtiene el total de monto por tipo de transacción
    /// </summary>
    [HttpGet("transactions/total-by-type")]
    public async Task<IActionResult> GetTotalByType(
        [FromQuery] string transactionType,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var total = await _transactionRepository.GetTotalAmountByTypeAsync(transactionType, startDate, endDate);
        return Ok(new { transactionType, total });
    }
}

