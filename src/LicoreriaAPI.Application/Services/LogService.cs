using MongoDB.Driver;
using MongoDB.Bson;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.MongoDB;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Data.MongoDB;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de Logs (MongoDB)
/// </summary>
public class LogService : ILogService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<LogService> _logger;

    public LogService(MongoDbContext mongoContext, ILogger<LogService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    private IMongoCollection<UserLog> GetUserLogsCollection()
    {
        return _mongoContext.GetCollection<UserLog>("user_logs");
    }

    private IMongoCollection<SystemLog> GetSystemLogsCollection()
    {
        return _mongoContext.GetCollection<SystemLog>("system_logs");
    }

    public async Task<List<UserLogDto>> ListarUserLogsAsync(int? userId = null, int top = 100)
    {
        try
        {
            var collection = GetUserLogsCollection();
            FilterDefinition<UserLog> filter = Builders<UserLog>.Filter.Empty;

            if (userId.HasValue)
            {
                filter = Builders<UserLog>.Filter.Eq(log => log.UserId, userId.Value);
            }

            var logs = await collection
                .Find(filter)
                .SortByDescending(log => log.Timestamp)
                .Limit(top)
                .ToListAsync();

            return logs.Select(log => MapUserLogToDto(log)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar logs de usuario");
            return new List<UserLogDto>();
        }
    }

    public async Task<UserLogDto?> ObtenerUserLogPorIdAsync(string id)
    {
        try
        {
            var collection = GetUserLogsCollection();
            var log = await collection.Find(l => l.Id == id).FirstOrDefaultAsync();
            
            if (log == null) return null;

            return MapUserLogToDto(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log de usuario ID: {Id}", id);
            return null;
        }
    }

    public async Task<UserLogDto> CrearUserLogAsync(CrearUserLogDto crearDto)
    {
        try
        {
            var collection = GetUserLogsCollection();
            var log = new UserLog
            {
                UserId = crearDto.UserId,
                Action = crearDto.Action,
                EntityType = crearDto.EntityType,
                EntityId = crearDto.EntityId,
                IpAddress = crearDto.IpAddress,
                UserAgent = crearDto.UserAgent,
                Details = crearDto.Details != null ? BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(crearDto.Details)) : null,
                Timestamp = DateTime.UtcNow
            };

            await collection.InsertOneAsync(log);
            return MapUserLogToDto(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear log de usuario");
            throw;
        }
    }

    public async Task<List<SystemLogDto>> ListarSystemLogsAsync(string? level = null, int top = 100)
    {
        try
        {
            var collection = GetSystemLogsCollection();
            FilterDefinition<SystemLog> filter = Builders<SystemLog>.Filter.Empty;

            if (!string.IsNullOrEmpty(level))
            {
                filter = Builders<SystemLog>.Filter.Eq(log => log.Level, level);
            }

            var logs = await collection
                .Find(filter)
                .SortByDescending(log => log.Timestamp)
                .Limit(top)
                .ToListAsync();

            return logs.Select(log => MapSystemLogToDto(log)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar logs del sistema");
            return new List<SystemLogDto>();
        }
    }

    public async Task<SystemLogDto?> ObtenerSystemLogPorIdAsync(string id)
    {
        try
        {
            var collection = GetSystemLogsCollection();
            var log = await collection.Find(l => l.Id == id).FirstOrDefaultAsync();
            
            if (log == null) return null;

            return MapSystemLogToDto(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log del sistema ID: {Id}", id);
            return null;
        }
    }

    public async Task<SystemLogDto> CrearSystemLogAsync(CrearSystemLogDto crearDto)
    {
        try
        {
            var collection = GetSystemLogsCollection();
            var log = new SystemLog
            {
                Level = crearDto.Level,
                Message = crearDto.Message,
                Exception = crearDto.Exception,
                Source = crearDto.Source,
                StackTrace = crearDto.StackTrace,
                Timestamp = DateTime.UtcNow
            };

            await collection.InsertOneAsync(log);
            return MapSystemLogToDto(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear log del sistema");
            throw;
        }
    }

    private UserLogDto MapUserLogToDto(UserLog log)
    {
        return new UserLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Details = log.Details != null 
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(log.Details.ToJson())
                : null,
            Timestamp = log.Timestamp
        };
    }

    private SystemLogDto MapSystemLogToDto(SystemLog log)
    {
        return new SystemLogDto
        {
            Id = log.Id,
            Level = log.Level,
            Message = log.Message,
            Exception = log.Exception,
            Source = log.Source,
            StackTrace = log.StackTrace,
            Timestamp = log.Timestamp
        };
    }
}

