using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace LicoreriaAPI.Infrastructure.Repositories.MongoDB;

public class NetworkUsageMetricRepository : INetworkUsageMetricRepository
{
    private readonly IMongoCollection<NetworkUsageMetric> _collection;
    private const string CollectionName = "networkUsageMetrics";

    private static bool _indexesCreated = false;
    private static readonly object _indexLock = new object();

    public NetworkUsageMetricRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<NetworkUsageMetric>(CollectionName);
        
        // Crear índices de forma lazy (solo una vez)
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
                            var indexKeys = Builders<NetworkUsageMetric>.IndexKeys.Descending(x => x.Timestamp);
                            var indexOptions = new CreateIndexOptions { Name = "timestamp_idx" };
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<NetworkUsageMetric>(indexKeys, indexOptions));
                        }
                        catch
                        {
                            // Ignorar errores de creación de índices (MongoDB puede no estar disponible)
                        }
                    });
                    _indexesCreated = true;
                }
            }
        }
    }

    public async Task AddAsync(NetworkUsageMetric metric)
    {
        await _collection.InsertOneAsync(metric);
    }

    public async Task<IEnumerable<NetworkUsageMetric>> GetMetricsAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 100)
    {
        var query = _collection.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<NetworkUsageMetric?> GetTotalBytesByPeriodAsync(DateTime startDate, DateTime endDate)
    {
        var metrics = await _collection
            .Find(x => x.Timestamp >= startDate && x.Timestamp <= endDate)
            .ToListAsync();

        if (!metrics.Any())
            return null;

        var totalSent = metrics.Sum(x => x.BytesSent);
        var totalReceived = metrics.Sum(x => x.BytesReceived);

        return new NetworkUsageMetric
        {
            BytesSent = totalSent,
            BytesReceived = totalReceived,
            TotalBytes = totalSent + totalReceived,
            Timestamp = DateTime.UtcNow
        };
    }
}

