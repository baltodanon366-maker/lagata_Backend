using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Métrica de uso de red (bytes enviados/recibidos)
/// </summary>
public class NetworkUsageMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("path")]
    public string Path { get; set; } = string.Empty;

    [BsonElement("method")]
    public string Method { get; set; } = string.Empty;

    [BsonElement("bytesSent")]
    public long BytesSent { get; set; }

    [BsonElement("bytesReceived")]
    public long BytesReceived { get; set; }

    [BsonElement("totalBytes")]
    public long TotalBytes { get; set; }

    [BsonElement("clientIp")]
    public string? ClientIp { get; set; }

    [BsonElement("userAgent")]
    public string? UserAgent { get; set; }

    [BsonElement("statusCode")]
    public int StatusCode { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("durationMs")]
    public long DurationMs { get; set; }
}

