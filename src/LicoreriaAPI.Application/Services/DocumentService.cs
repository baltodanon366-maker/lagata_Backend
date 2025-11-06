using MongoDB.Driver;
using LicoreriaAPI.Application.Interfaces.Services;
using LicoreriaAPI.DTOs.MongoDB;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Data.MongoDB;
using Microsoft.Extensions.Logging;

namespace LicoreriaAPI.Application.Services;

/// <summary>
/// Servicio de Documentos (MongoDB)
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(MongoDbContext mongoContext, ILogger<DocumentService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    private IMongoCollection<DocumentMetadata> GetCollection()
    {
        return _mongoContext.GetCollection<DocumentMetadata>("document_metadata");
    }

    public async Task<List<DocumentMetadataDto>> ListarAsync(string? entityType = null, int? entityId = null, int top = 100)
    {
        try
        {
            var collection = GetCollection();
            var filterBuilder = Builders<DocumentMetadata>.Filter;
            FilterDefinition<DocumentMetadata> filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(entityType))
            {
                filter &= filterBuilder.Eq(d => d.EntityType, entityType);
            }

            if (entityId.HasValue)
            {
                filter &= filterBuilder.Eq(d => d.EntityId, entityId.Value);
            }

            var documents = await collection
                .Find(filter)
                .SortByDescending(d => d.UploadedAt)
                .Limit(top)
                .ToListAsync();

            return documents.Select(d => MapToDto(d)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar documentos");
            return new List<DocumentMetadataDto>();
        }
    }

    public async Task<DocumentMetadataDto?> ObtenerPorIdAsync(string id)
    {
        try
        {
            var collection = GetCollection();
            var document = await collection.Find(d => d.Id == id).FirstOrDefaultAsync();
            
            if (document == null) return null;

            return MapToDto(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documento ID: {Id}", id);
            return null;
        }
    }

    public async Task<DocumentMetadataDto> CrearAsync(CrearDocumentMetadataDto crearDto)
    {
        try
        {
            var collection = GetCollection();
            var document = new DocumentMetadata
            {
                EntityType = crearDto.EntityType,
                EntityId = crearDto.EntityId,
                DocumentType = crearDto.DocumentType,
                FileName = crearDto.FileName,
                StoragePath = crearDto.StoragePath,
                UploadedBy = crearDto.UploadedBy,
                UploadedAt = DateTime.UtcNow,
                Size = crearDto.Size,
                MimeType = crearDto.MimeType
            };

            await collection.InsertOneAsync(document);
            return MapToDto(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear documento");
            throw;
        }
    }

    private DocumentMetadataDto MapToDto(DocumentMetadata document)
    {
        return new DocumentMetadataDto
        {
            Id = document.Id,
            EntityType = document.EntityType,
            EntityId = document.EntityId,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            StoragePath = document.StoragePath,
            UploadedBy = document.UploadedBy,
            UploadedAt = document.UploadedAt,
            Size = document.Size,
            MimeType = document.MimeType
        };
    }
}

