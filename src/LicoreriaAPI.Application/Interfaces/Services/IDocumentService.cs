using LicoreriaAPI.DTOs.MongoDB;

namespace LicoreriaAPI.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de Documentos (MongoDB)
/// </summary>
public interface IDocumentService
{
    Task<List<DocumentMetadataDto>> ListarAsync(string? entityType = null, int? entityId = null, int top = 100);
    Task<DocumentMetadataDto?> ObtenerPorIdAsync(string id);
    Task<DocumentMetadataDto> CrearAsync(CrearDocumentMetadataDto crearDto);
}

