using LicoreriaAPI.Domain.Models.MongoDB;

namespace LicoreriaAPI.Domain.Interfaces;

/// <summary>
/// Repositorio para métricas de uso de red
/// </summary>
public interface INetworkUsageMetricRepository
{
    Task AddAsync(NetworkUsageMetric metric);
    Task<IEnumerable<NetworkUsageMetric>> GetMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 100);
    Task<NetworkUsageMetric?> GetTotalBytesByPeriodAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Repositorio para intentos fallidos de login
/// </summary>
public interface IFailedLoginAttemptRepository
{
    Task AddAsync(FailedLoginAttemptMetric metric);
    Task<IEnumerable<FailedLoginAttemptMetric>> GetAttemptsAsync(string? ipAddress = null, string? username = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100);
    Task<int> GetAttemptCountByIpAsync(string ipAddress, DateTime? since = null);
    Task<IEnumerable<FailedLoginAttemptMetric>> GetSuspiciousAttemptsAsync(DateTime? startDate = null, int limit = 100);
}

/// <summary>
/// Repositorio para consultas lentas
/// </summary>
public interface ISlowQueryMetricRepository
{
    Task AddAsync(SlowQueryMetric metric);
    Task<IEnumerable<SlowQueryMetric>> GetSlowQueriesAsync(string? tableName = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100);
    Task<IEnumerable<SlowQueryMetric>> GetSlowestQueriesAsync(int limit = 10);
}

/// <summary>
/// Repositorio para usuarios activos
/// </summary>
public interface IActiveUserMetricRepository
{
    Task AddOrUpdateAsync(ActiveUserMetric metric);
    Task<IEnumerable<ActiveUserMetric>> GetActiveUsersAsync(DateTime? since = null);
    Task<int> GetActiveUserCountAsync(DateTime? since = null);
    Task UpdateLastActivityAsync(int userId);
    Task DeactivateInactiveUsersAsync(TimeSpan inactivityThreshold);
}

/// <summary>
/// Repositorio para métricas de transacciones
/// </summary>
public interface ITransactionMetricRepository
{
    Task AddAsync(TransactionMetric metric);
    Task<IEnumerable<TransactionMetric>> GetTransactionsAsync(string? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100);
    Task<Dictionary<string, int>> GetTransactionCountByTypeAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalAmountByTypeAsync(string transactionType, DateTime? startDate = null, DateTime? endDate = null);
}

