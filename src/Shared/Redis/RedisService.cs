using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Shared.Redis;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<RedisService> _logger;

    public RedisService(IOptions<RedisSettings> settings, ILogger<RedisService> logger)
    {
        _logger = logger;
        _logger.LogInformation($"Connecting to Redis with connection string: {settings.Value.ConnectionString}");
        try
        {
            _redis = ConnectionMultiplexer.Connect(settings.Value.ConnectionString);
            _db = _redis.GetDatabase();
            _logger.LogInformation("Successfully connected to Redis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis");
            throw;
        }
    }

    public async Task<T> GetAsync<T>(string key)
    {
        try
        {
            _logger.LogInformation($"Getting value from Redis with key: {key}");
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogInformation($"No value found in Redis for key: {key}");
                return default;
            }
            _logger.LogInformation($"Successfully retrieved value from Redis for key: {key}");
            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting value from Redis for key: {key}");
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, int expirationMinutes = 60)
    {
        try
        {
            _logger.LogInformation($"Setting value in Redis with key: {key}");
            var serializedValue = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, serializedValue, TimeSpan.FromMinutes(expirationMinutes));
            _logger.LogInformation($"Successfully set value in Redis for key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting value in Redis for key: {key}");
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _logger.LogInformation($"Removing value from Redis with key: {key}");
            await _db.KeyDeleteAsync(key);
            _logger.LogInformation($"Successfully removed value from Redis for key: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing value from Redis for key: {key}");
            throw;
        }
    }
} 