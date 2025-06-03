using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore; // <-- Убедитесь, что using есть
using MongoDB.Driver;
using Neo4j.Driver;
using StackExchange.Redis;

namespace University_Schedule_Generator;

public static class ServiceDataBaseExtensions
{
    public static void AddPostgres(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        // Оставляем AddDbContext, если ApplicationContext используется где-то еще
        // напрямую в запросах (как Scoped). Если нет - можно убрать.
        services.AddDbContext<ApplicationContext>(options =>
            options.UseNpgsql(connectionString));

        // *** Добавляем регистрацию ФАБРИКИ DbContext ***
        // Это позволит создавать контекст с нужным lifetime вручную.
        services.AddDbContextFactory<ApplicationContext>(options =>
            options.UseNpgsql(connectionString),
            ServiceLifetime.Scoped); // Фабрика может быть Scoped или Singleton,
                                     // Scoped чаще всего подходит.
    }

    public static void AddRedis(this IServiceCollection services, IConfiguration config)
    {
        // Без изменений
        var redis = ConnectionMultiplexer.Connect(config["RedisOptions:Configuration"]);
        services.AddSingleton(redis.GetDatabase());
    }

    public static void AddMongoDb(this IServiceCollection services, IConfiguration config)
    {
        // Без изменений
        var mongoSettings = config.GetSection("MongoDbSettings");
        var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
        var mongoDatabase = mongoClient.GetDatabase(mongoSettings["Database"]);
        services.AddSingleton<IMongoDatabase>(mongoDatabase);
    }

    public static void AddNeo4j(this IServiceCollection services, IConfiguration config)
    {
        // Без изменений
        var neo4jOptions = config.GetSection("Neo4jOptions");
        var neo4jUri = neo4jOptions["Uri"];
        var neo4jUsername = neo4jOptions["Username"];
        var neo4jPassword = neo4jOptions["Password"];
        var neo4jDriver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUsername, neo4jPassword));
        services.AddSingleton<IDriver>(neo4jDriver);
    }

    public static void AddElastic(this IServiceCollection services, IConfiguration config)
    {
        // Без изменений
        string esUri = config["ElasticsearchOptions:Uri"];
        var esSettings = new ElasticsearchClientSettings(new Uri(esUri))
            .DefaultIndex("materials"); // optional
        services.AddSingleton(new ElasticsearchClient(esSettings));
    }
}