using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Métrica de consultas lentas (>100ms)
/// </summary>
public class SlowQueryMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("queryType")]
    public string QueryType { get; set; } = string.Empty; // SELECT, INSERT, UPDATE, DELETE, StoredProcedure

    [BsonElement("tableName")]
    public string? TableName { get; set; }

    [BsonElement("queryText")]
    public string? QueryText { get; set; } // SQL query o nombre del SP

    [BsonElement("durationMs")]
    public long DurationMs { get; set; }

    [BsonElement("thresholdMs")]
    public long ThresholdMs { get; set; } = 100; // Umbral configurado

    [BsonElement("rowsAffected")]
    public int? RowsAffected { get; set; }

    [BsonElement("endpoint")]
    public string? Endpoint { get; set; } // Endpoint de la API que ejecutó la consulta

    [BsonElement("userId")]
    public int? UserId { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("executionPlan")]
    public string? ExecutionPlan { get; set; } // Plan de ejecución si está disponible
}

