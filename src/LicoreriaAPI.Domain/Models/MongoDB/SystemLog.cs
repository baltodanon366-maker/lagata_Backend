using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Modelo de log del sistema en MongoDB
/// </summary>
public class SystemLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("level")]
    public string Level { get; set; } = string.Empty; // Error, Warning, Info

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("exception")]
    public string? Exception { get; set; }

    [BsonElement("source")]
    public string? Source { get; set; }

    [BsonElement("stackTrace")]
    public string? StackTrace { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

