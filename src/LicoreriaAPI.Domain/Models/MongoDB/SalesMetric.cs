using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Modelo de métrica de ventas para MongoDB (usado en dashboard)
/// </summary>
public class SalesMetric
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("totalSales")]
    public decimal TotalSales { get; set; }

    [BsonElement("totalTransactions")]
    public int TotalTransactions { get; set; }

    [BsonElement("averageTicket")]
    public decimal AverageTicket { get; set; }

    [BsonElement("salesByCategory")]
    public Dictionary<string, decimal>? SalesByCategory { get; set; }

    [BsonElement("topProducts")]
    public List<TopProduct>? TopProducts { get; set; }

    [BsonElement("calculatedAt")]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class TopProduct
{
    [BsonElement("productId")]
    public int ProductId { get; set; }

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }

    [BsonElement("total")]
    public decimal Total { get; set; }
}

