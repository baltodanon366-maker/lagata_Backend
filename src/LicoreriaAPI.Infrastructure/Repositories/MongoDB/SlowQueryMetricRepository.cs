using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace LicoreriaAPI.Infrastructure.Repositories.MongoDB;

public class SlowQueryMetricRepository : ISlowQueryMetricRepository
{
    private readonly IMongoCollection<SlowQueryMetric> _collection;
    private const string CollectionName = "slowQueries";

    private static bool _indexesCreated = false;
    private static readonly object _indexLock = new object();

    public SlowQueryMetricRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<SlowQueryMetric>(CollectionName);
        
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
                            var timestampIndex = Builders<SlowQueryMetric>.IndexKeys.Descending(x => x.Timestamp);
                            var durationIndex = Builders<SlowQueryMetric>.IndexKeys.Descending(x => x.DurationMs);
                            var tableIndex = Builders<SlowQueryMetric>.IndexKeys.Ascending(x => x.TableName);
                            
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<SlowQueryMetric>(timestampIndex, new CreateIndexOptions { Name = "timestamp_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<SlowQueryMetric>(durationIndex, new CreateIndexOptions { Name = "duration_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<SlowQueryMetric>(tableIndex, new CreateIndexOptions { Name = "tableName_idx" }));
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

    public async Task AddAsync(SlowQueryMetric metric)
    {
        await _collection.InsertOneAsync(metric);
    }

    public async Task<IEnumerable<SlowQueryMetric>> GetSlowQueriesAsync(string? tableName = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100)
    {
        var query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(tableName))
            query = query.Where(x => x.TableName == tableName);

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(x => x.DurationMs)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<SlowQueryMetric>> GetSlowestQueriesAsync(int limit = 10)
    {
        return await _collection
            .AsQueryable()
            .OrderByDescending(x => x.DurationMs)
            .Take(limit)
            .ToListAsync();
    }
}

