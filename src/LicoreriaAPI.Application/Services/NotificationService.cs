using MongoDB.Driver;
using MongoDB.Bson;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.MongoDB;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Data.MongoDB;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de Notificaciones (MongoDB)
/// </summary>
public class NotificationService : INotificationService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(MongoDbContext mongoContext, ILogger<NotificationService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    private IMongoCollection<Notification> GetCollection()
    {
        return _mongoContext.GetCollection<Notification>("notifications");
    }

    public async Task<List<NotificationDto>> ListarAsync(int userId, bool soloNoLeidas = false, int top = 50)
    {
        try
        {
            var collection = GetCollection();
            var filterBuilder = Builders<Notification>.Filter;
            var filter = filterBuilder.Eq(n => n.UserId, userId);

            if (soloNoLeidas)
            {
                filter &= filterBuilder.Eq(n => n.Read, false);
            }

            var notifications = await collection
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .Limit(top)
                .ToListAsync();

            return notifications.Select(n => MapToDto(n)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar notificaciones para usuario: {UserId}", userId);
            return new List<NotificationDto>();
        }
    }

    public async Task<NotificationDto?> ObtenerPorIdAsync(string id)
    {
        try
        {
            var collection = GetCollection();
            var notification = await collection.Find(n => n.Id == id).FirstOrDefaultAsync();
            
            if (notification == null) return null;

            return MapToDto(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificación ID: {Id}", id);
            return null;
        }
    }

    public async Task<NotificationDto> CrearAsync(CrearNotificationDto crearDto)
    {
        try
        {
            var collection = GetCollection();
            var notification = new Notification
            {
                UserId = crearDto.UserId,
                Type = crearDto.Type,
                Title = crearDto.Title,
                Message = crearDto.Message,
                Read = false,
                CreatedAt = DateTime.UtcNow,
                Data = crearDto.Data != null ? BsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(crearDto.Data)) : null
            };

            await collection.InsertOneAsync(notification);
            return MapToDto(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear notificación");
            throw;
        }
    }

    public async Task<bool> MarcarComoLeidaAsync(string id)
    {
        try
        {
            var collection = GetCollection();
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notification>.Update
                .Set(n => n.Read, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar notificación como leída ID: {Id}", id);
            return false;
        }
    }

    private NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Data = notification.Data != null 
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data.ToJson())
                : null
        };
    }
}

