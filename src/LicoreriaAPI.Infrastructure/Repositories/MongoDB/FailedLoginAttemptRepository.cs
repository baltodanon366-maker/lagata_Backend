using LicoreriaAPI.Domain.Interfaces;
using LicoreriaAPI.Domain.Models.MongoDB;
using LicoreriaAPI.Infrastructure.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;

namespace LicoreriaAPI.Infrastructure.Repositories.MongoDB;

public class FailedLoginAttemptRepository : IFailedLoginAttemptRepository
{
    private readonly IMongoCollection<FailedLoginAttemptMetric> _collection;
    private const string CollectionName = "failedLoginAttempts";

    private static bool _indexesCreated = false;
    private static readonly object _indexLock = new object();

    public FailedLoginAttemptRepository(IMongoClient mongoClient, IOptions<MongoDBSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<FailedLoginAttemptMetric>(CollectionName);
        
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
                            var timestampIndex = Builders<FailedLoginAttemptMetric>.IndexKeys.Descending(x => x.Timestamp);
                            var ipIndex = Builders<FailedLoginAttemptMetric>.IndexKeys.Ascending(x => x.IpAddress);
                            var usernameIndex = Builders<FailedLoginAttemptMetric>.IndexKeys.Ascending(x => x.Username);
                            
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<FailedLoginAttemptMetric>(timestampIndex, new CreateIndexOptions { Name = "timestamp_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<FailedLoginAttemptMetric>(ipIndex, new CreateIndexOptions { Name = "ipAddress_idx" }));
                            await _collection.Indexes.CreateOneAsync(new CreateIndexModel<FailedLoginAttemptMetric>(usernameIndex, new CreateIndexOptions { Name = "username_idx" }));
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

    public async Task AddAsync(FailedLoginAttemptMetric metric)
    {
        await _collection.InsertOneAsync(metric);
    }

    public async Task<IEnumerable<FailedLoginAttemptMetric>> GetAttemptsAsync(string? ipAddress = null, string? username = null, DateTime? startDate = null, DateTime? endDate = null, int limit = 100)
    {
        var query = _collection.AsQueryable();

        if (!string.IsNullOrEmpty(ipAddress))
            query = query.Where(x => x.IpAddress == ipAddress);

        if (!string.IsNullOrEmpty(username))
            query = query.Where(x => x.Username == username);

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(x => x.Timestamp <= endDate.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetAttemptCountByIpAsync(string ipAddress, DateTime? since = null)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.IpAddress == ipAddress);

        if (since.HasValue)
            query = query.Where(x => x.Timestamp >= since.Value);

        return await query.CountAsync();
    }

    public async Task<IEnumerable<FailedLoginAttemptMetric>> GetSuspiciousAttemptsAsync(DateTime? startDate = null, int limit = 100)
    {
        var query = _collection.AsQueryable()
            .Where(x => x.IsSuspicious);

        if (startDate.HasValue)
            query = query.Where(x => x.Timestamp >= startDate.Value);

        return await query
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}

