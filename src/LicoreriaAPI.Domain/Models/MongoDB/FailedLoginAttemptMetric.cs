using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Métrica de intentos fallidos de inicio de sesión
/// </summary>
public class FailedLoginAttemptMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("failureReason")]
    public string FailureReason { get; set; } = string.Empty; // InvalidPassword, UserNotFound, AccountLocked, etc.

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("isSuspicious")]
    public bool IsSuspicious { get; set; } // Múltiples intentos desde la misma IP

    [BsonElement("attemptCount")]
    public int AttemptCount { get; set; } = 1; // Contador de intentos consecutivos
}

