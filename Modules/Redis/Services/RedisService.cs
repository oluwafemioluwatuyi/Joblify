using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Joblify.Modules.Redis.Configurations;
using Joblify.Modules.Redis.Interfaces;

namespace Joblify.Modules.Redis.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;
    private readonly RedisConfiguration _config;
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis, IOptions<RedisConfiguration> config)
    {
        _redis = redis;
        _config = config.Value;
        // Connect to the specific database defined in your config (0-15)
        _database = _redis.GetDatabase(_config.Database);
    }
    private string GetPrefixedKey(string key) => $"{_config.InstancePrefix}{key}";

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        await _database.StringSetAsync(GetPrefixedKey(key), value, expiration ?? TimeSpan.Zero);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _database.StringGetAsync(GetPrefixedKey(key));
        return value.IsNull ? null : value.ToString();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(GetPrefixedKey(key), json, expiration ?? TimeSpan.Zero);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(GetPrefixedKey(key));

        if (value.IsNull || !value.HasValue) return default;

        try
        {
            return JsonSerializer.Deserialize<T>((string)value!, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            // value exists but can't be deserialized into T
            return default;
        }
    }

    public async Task<long> IncrementAttemptsAsync(string key, TimeSpan window)
    {
        var prefixedKey = GetPrefixedKey(key);

        var newValue = await _database.StringIncrementAsync(prefixedKey);

        // If this is the first attempt, set the sliding expiration window (e.g., 15 mins)
        if (newValue == 1)
        {
            await _database.KeyExpireAsync(prefixedKey, window);
        }

        return newValue;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(GetPrefixedKey(key));
    }

    public async Task RemoveAsync(string key)
    {
        await _database.KeyDeleteAsync(GetPrefixedKey(key));
    }

    public async Task ClearAllAsync()
    {
        // Note: In production, we usually only clear keys with our prefix 
        // to avoid wiping other apps' data on the same Redis server.
        var endpoints = _redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _redis.GetServer(endpoint);
            // This retrieves keys matching your prefix specifically
            var keys = server.Keys(_config.Database, pattern: $"{_config.InstancePrefix}*");
            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
    }
}