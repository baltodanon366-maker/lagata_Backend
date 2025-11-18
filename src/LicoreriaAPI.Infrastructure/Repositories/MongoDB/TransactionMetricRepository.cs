using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace LicoreriaAPI.Infrastructure.Repositories.MongoDB;

public class TransactionMetricRepository : ITransactionMetricRepository
{
    private readonly IMongoCollection<TransactionMetric> _collection;
    private const string CollectionName = "transactionMetrics";

    private static bool _indexesCreated = false;
    private static readonly object _indexLock = new object();

    public TransactionMetricRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<TransactionMetric>(CollectionName);
        
        // Crear índices de forma lazy
        if (!_indexesCreated)
        {
            lock (_indexLock)
            {
                if (!_indexesCreated)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var timestampIndex = Builders<TransactionMetric>.IndexKeys.Descending(x => x.Timestamp);
                            var typeIndex = Builders<TransactionMetric>.IndexKeys.Ascending(x => x.TransactionType);
                            var statusIndex = Builders<TransactionMetric>.IndexKeys.Ascending(x => x.Status);
                            
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<TransactionMetric>(timestampIndex, new CreateIndexOptions { Name = "timestamp_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<TransactionMetric>(typeIndex, new CreateIndexOptions { Name = "transactionType_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<TransactionMetric>(statusIndex, new CreateIndexOptions { Name = "status_idx" }));
                        }
                        catch
                        {
                            // Ignorar errores de creación de índices
                        }
                    });
                    _indexesCreated = true;
                }
            }
        }
    }

    public async Task AddAsync(TransactionMetric metric)
    {
        await _collection.InsertOneAsync(metric);
    }

    public async Task<IEnumerable<TransactionMetric>> GetTransactionsAsync(string? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100)
    {
        var query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(transactionType))
            query = query.Where(x => x.TransactionType == transactionType);

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetTransactionCountByTypeAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _collection.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        var transactions = await query.ToListAsync();
        
        return transactions
            .GroupBy(x => x.TransactionType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<decimal> GetTotalAmountByTypeAsync(string transactionType, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.TransactionType == transactionType);

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        var transactions = await query.ToListAsync();
        return transactions.Sum(x => x.Amount);
    }
}

