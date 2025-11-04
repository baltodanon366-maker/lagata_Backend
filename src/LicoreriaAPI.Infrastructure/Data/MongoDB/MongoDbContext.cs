using MongoDB.Driver;
using LicoreriaAPI.Infrastructure.Configuration;

namespace LicoreriaAPI.Infrastructure.Data.MongoDB;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoClient mongoClient, MongoDBSettings settings)
    {
        _database = mongoClient.GetDatabase(settings.DatabaseName);
    }

    // Aquí se agregarán las colecciones de MongoDB
    // Ejemplo: public IMongoCollection<T> GetCollection<T>(string collectionName)
    // {
    //     return _database.GetCollection<T>(collectionName);
    // }

    public IMongoDatabase Database => _database;
}

