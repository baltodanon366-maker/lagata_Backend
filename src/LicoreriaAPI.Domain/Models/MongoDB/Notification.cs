using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Modelo de notificación en MongoDB
/// </summary>
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public int UserId { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // StockBajo, VentaCompletada, CompraRecibida, etc.

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("read")]
    public bool Read { get; set; } = false;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("data")]
    public BsonDocument? Data { get; set; } // Datos flexibles
}
