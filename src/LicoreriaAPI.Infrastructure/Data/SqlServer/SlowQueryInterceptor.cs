using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;

namespace LicoreriaAPI.Infrastructure.Data.SqlServer;

/// <summary>
/// Interceptor de EF Core para rastrear consultas lentas (>100ms)
/// </summary>
public class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor> _logger;
    private readonly ISlowQueryMetricRepository? _repository;
    private const long SlowQueryThresholdMs = 100;

    public SlowQueryInterceptor(
        ILogger<SlowQueryInterceptor> logger,
        ISlowQueryMetricRepository? repository = null)
    {
        _logger = logger;
        _repository = repository;
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var originalResult = await base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowQueryThresholdMs && _repository != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var metric = new SlowQueryMetric
                    {
                        QueryType = GetQueryType(command.CommandText),
                        TableName = ExtractTableName(command.CommandText),
                        QueryText = command.CommandText.Length > 500 
                            ? command.CommandText.Substring(0, 500) + "..." 
                            : command.CommandText,
                        DurationMs = stopwatch.ElapsedMilliseconds,
                        ThresholdMs = SlowQueryThresholdMs,
                        Timestamp = DateTime.UtcNow
                    };

                    await _repository.AddAsync(metric);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar consulta lenta");
                }
            });
        }

        return originalResult;
    }

    private string GetQueryType(string commandText)
    {
        var upperText = commandText.ToUpper().Trim();
        if (upperText.StartsWith("SELECT"))
            return "SELECT";
        if (upperText.StartsWith("INSERT"))
            return "INSERT";
        if (upperText.StartsWith("UPDATE"))
            return "UPDATE";
        if (upperText.StartsWith("DELETE"))
            return "DELETE";
        if (upperText.StartsWith("EXEC") || upperText.StartsWith("EXECUTE"))
            return "StoredProcedure";
        return "Unknown";
    }

    private string? ExtractTableName(string commandText)
    {
        // Intentar extraer el nombre de la tabla de la consulta SQL
        var upperText = commandText.ToUpper();
        
        // Para SELECT: FROM tabla
        var fromIndex = upperText.IndexOf("FROM");
        if (fromIndex >= 0)
        {
            var afterFrom = commandText.Substring(fromIndex + 4).Trim();
            var spaceIndex = afterFrom.IndexOfAny(new[] { ' ', '\t', '\n', '\r', ';' });
            if (spaceIndex > 0)
                return afterFrom.Substring(0, spaceIndex).Trim();
            return afterFrom.Trim();
        }

        // Para INSERT: INTO tabla
        var intoIndex = upperText.IndexOf("INTO");
        if (intoIndex >= 0)
        {
            var afterInto = commandText.Substring(intoIndex + 4).Trim();
            var spaceIndex = afterInto.IndexOfAny(new[] { ' ', '\t', '\n', '\r', '(', ';' });
            if (spaceIndex > 0)
                return afterInto.Substring(0, spaceIndex).Trim();
            return afterInto.Trim();
        }

        // Para UPDATE: UPDATE tabla
        var updateIndex = upperText.IndexOf("UPDATE");
        if (updateIndex >= 0)
        {
            var afterUpdate = commandText.Substring(updateIndex + 6).Trim();
            var spaceIndex = afterUpdate.IndexOfAny(new[] { ' ', '\t', '\n', '\r', ';' });
            if (spaceIndex > 0)
                return afterUpdate.Substring(0, spaceIndex).Trim();
            return afterUpdate.Trim();
        }

        return null;
    }
}

