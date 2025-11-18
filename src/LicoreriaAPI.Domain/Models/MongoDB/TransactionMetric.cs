using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Métrica de transacciones por tipo
/// </summary>
public class TransactionMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("transactionType")]
    public string TransactionType { get; set; } = string.Empty; // Venta, Compra, DevolucionVenta, DevolucionCompra

    [BsonElement("transactionId")]
    public int TransactionId { get; set; }

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("userId")]
    public int? UserId { get; set; }

    [BsonElement("clientId")]
    public int? ClientId { get; set; }

    [BsonElement("supplierId")]
    public int? SupplierId { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // Completed, Pending, Cancelled, Failed

    [BsonElement("itemCount")]
    public int ItemCount { get; set; }

    [BsonElement("paymentMethod")]
    public string? PaymentMethod { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("durationMs")]
    public long? DurationMs { get; set; } // Tiempo que tomó procesar la transacción

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }
}

