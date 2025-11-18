using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Métrica de usuarios activos
/// </summary>
public class ActiveUserMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userId")]
    public int UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("role")]
    public string? Role { get; set; }

    [BsonElement("sessionStart")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime SessionStart { get; set; } = DateTime.UtcNow;

    [BsonElement("lastActivity")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("requestCount")]
    public int RequestCount { get; set; } = 1;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

