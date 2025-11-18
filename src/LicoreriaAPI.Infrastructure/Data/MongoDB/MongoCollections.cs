using MongoDB.Driver;

namespace LicoreriaAPI.Infrastructure.Data.MongoDB;

/// <summary>
/// Helper para acceder a las colecciones de MongoDB (solo métricas)
/// </summary>
public static class MongoCollections
{
    // Colecciones de métricas
    public const string NetworkUsageMetrics = "networkUsageMetrics";
    public const string FailedLoginAttempts = "failedLoginAttempts";
    public const string SlowQueries = "slowQueries";
    public const string ActiveUsers = "activeUsers";
    public const string TransactionMetrics = "transactionMetrics";

    /// <summary>
    /// Obtiene una colección tipada de MongoDB
    /// </summary>
    public static IMongoCollection<T> GetCollection<T>(MongoDbContext context, string collectionName)
    {
        return context.Database.GetCollection<T>(collectionName);
    }
}
