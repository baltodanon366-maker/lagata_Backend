using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LicoreriaAPI.Domain.Models.MongoDB;

/// <summary>
/// Modelo de metadatos de documento en MongoDB
/// </summary>
public class DocumentMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("entityType")]
    public string EntityType { get; set; } = string.Empty; // venta, compra, producto

    [BsonElement("entityId")]
    public int EntityId { get; set; }

    [BsonElement("documentType")]
    public string DocumentType { get; set; } = string.Empty; // invoice, receipt, image

    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("storagePath")]
    public string StoragePath { get; set; } = string.Empty;

    [BsonElement("uploadedBy")]
    public int UploadedBy { get; set; }

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("size")]
    public long Size { get; set; }

    [BsonElement("mimeType")]
    public string MimeType { get; set; } = string.Empty;
}

