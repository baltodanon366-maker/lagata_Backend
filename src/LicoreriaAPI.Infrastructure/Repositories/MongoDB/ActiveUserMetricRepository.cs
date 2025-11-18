using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace LicoreriaAPI.Infrastructure.Repositories.MongoDB;

public class ActiveUserMetricRepository : IActiveUserMetricRepository
{
    private readonly IMongoCollection<ActiveUserMetric> _collection;
    private const string CollectionName = "activeUsers";

    private static bool _indexesCreated = false;
    private static readonly object _indexLock = new object();

    public ActiveUserMetricRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<ActiveUserMetric>(CollectionName);
        
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
                            var userIdIndex = Builders<ActiveUserMetric>.IndexKeys.Ascending(x => x.UserId);
                            var timestampIndex = Builders<ActiveUserMetric>.IndexKeys.Descending(x => x.LastActivity);
                            var isActiveIndex = Builders<ActiveUserMetric>.IndexKeys.Ascending(x => x.IsActive);
                            
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<ActiveUserMetric>(userIdIndex, new CreateIndexOptions { Name = "userId_idx", Unique = true }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<ActiveUserMetric>(timestampIndex, new CreateIndexOptions { Name = "lastActivity_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<ActiveUserMetric>(isActiveIndex, new CreateIndexOptions { Name = "isActive_idx" }));
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

    public async Task AddOrUpdateAsync(ActiveUserMetric metric)
    {
        var filter = Builders<ActiveUserMetric>.Filter.Eq(x => x.UserId, metric.UserId);
        var existing = await _collection.Find(filter).FirstOrDefaultAsync();

        if (existing != null)
        {
            var update = Builders<ActiveUserMetric>.Update
                .Set(x => x.LastActivity, metric.LastActivity)
                .Set(x => x.RequestCount, existing.RequestCount + 1)
                .Set(x => x.IsActive, true)
                .Set(x => x.IpAddress, metric.IpAddress);

            await _collection.UpdateOneAsync(filter, update);
        }
        else
        {
            await _collection.InsertOneAsync(metric);
        }
    }

    public async Task<IEnumerable<ActiveUserMetric>> GetActiveUsersAsync(DateTime? since = null)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.IsActive);

        if (since.HasValue)
            query = query.Where(x => x.LastActivity >= since.Value);

        return await query
            .OrderByDescending(x => x.LastActivity)
            .ToListAsync();
    }

    public async Task<int> GetActiveUserCountAsync(DateTime? since = null)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.IsActive);

        if (since.HasValue)
            query = query.Where(x => x.LastActivity >= since.Value);

        return await query.CountAsync();
    }

    public async Task UpdateLastActivityAsync(int userId)
    {
        var filter = Builders<ActiveUserMetric>.Filter.Eq(x => x.UserId, userId);
        var update = Builders<ActiveUserMetric>.Update
            .Set(x => x.LastActivity, DateTime.UtcNow)
            .Inc(x => x.RequestCount, 1);

        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task DeactivateInactiveUsersAsync(TimeSpan inactivityThreshold)
    {
        var threshold = DateTime.UtcNow - inactivityThreshold;
        var filter = Builders<ActiveUserMetric>.Filter
            .And(
                Builders<ActiveUserMetric>.Filter.Eq(x => x.IsActive, true),
                Builders<ActiveUserMetric>.Filter.Lt(x => x.LastActivity, threshold)
            );

        var update = Builders<ActiveUserMetric>.Update
            .Set(x => x.IsActive, false);

        await _collection.UpdateManyAsync(filter, update);
    }
}

