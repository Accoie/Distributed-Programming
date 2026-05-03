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
        string? configuration = Environment.GetEnvironmentVariable("DB_MAIN") ?? "localhost:6000";
        return _redisConnectionFactory.GetDatabase(configuration);
    }
    
    public IDatabase GetDatabaseForRegion(string regionCode)
    {
        string connectionString = regionCode switch
        {
            "RU" => Environment.GetEnvironmentVariable("DB_RU") ?? "localhost:6001",
            "EU" => Environment.GetEnvironmentVariable("DB_EU") ?? "localhost:6002",
            "ASIA" => Environment.GetEnvironmentVariable("DB_ASIA") ?? "localhost:6003",
            _ => Environment.GetEnvironmentVariable("DB_EU") ?? "localhost:6002"
        };
        return _redisConnectionFactory.GetDatabase(connectionString);
    }
}