using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Modelo de log de usuario en MongoDB
/// </summary>
public class UserLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public int UserId { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = string.Empty; // CreateVenta, UpdateProducto, etc.

    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [BsonElement("entityId")]
    public int? EntityId { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("details")]
    public BsonDocument? Details { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
