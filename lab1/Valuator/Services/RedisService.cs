using Shared;
using Shared.Enums;
using StackExchange.Redis;

namespace Valuator.Services;

public class RedisService
{
    private readonly RedisConnectionFactory _redisConnectionFactory;

    public RedisService(RedisConnectionFactory redisConnectionFactory)
    {
        _redisConnectionFactory = redisConnectionFactory;
    }

    public IDatabase GetMainDatabase()
    {
        string configuration = Environment.GetEnvironmentVariable("DB_MAIN")!;
        return _redisConnectionFactory.GetDatabase(configuration);
    }
    
    public IDatabase GetDatabaseForRegion(Region region)
    {
        string connectionString = region switch
        {
            Region.Ru => Environment.GetEnvironmentVariable("DB_RU") ?? "localhost:6001",
            Region.Eu => Environment.GetEnvironmentVariable("DB_EU") ?? "localhost:6002",
            Region.Asia => Environment.GetEnvironmentVariable("DB_ASIA") ?? "localhost:6003",
            _ => Environment.GetEnvironmentVariable("DB_EU") ?? "localhost:6002"
        };
        return _redisConnectionFactory.GetDatabase(connectionString);
    }
}