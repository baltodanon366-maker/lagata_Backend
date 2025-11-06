using System.ComponentModel.DataAnnotations;

namespace LicoreriaAPI.DTOs.MongoDB;

/// <summary>
/// DTO para metadatos de documento
/// </summary>
public class DocumentMetadataDto
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public long Size { get; set; }
    public string MimeType { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear/actualizar metadatos de documento
/// </summary>
public class CrearDocumentMetadataDto
{
    [Required(ErrorMessage = "El tipo de entidad es requerido")]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID de la entidad es requerido")]
    public int EntityId { get; set; }

    [Required(ErrorMessage = "El tipo de documento es requerido")]
    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del archivo es requerido")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ruta de almacenamiento es requerida")]
    [MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ID del usuario que sube es requerido")]
    public int UploadedBy { get; set; }

    [Required(ErrorMessage = "El tamaño es requerido")]
    public long Size { get; set; }

    [Required(ErrorMessage = "El tipo MIME es requerido")]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;
}

