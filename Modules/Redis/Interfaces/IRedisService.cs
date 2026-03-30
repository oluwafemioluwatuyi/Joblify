namespace Joblify.Modules.Redis.Interfaces;

public interface IRedisService
{
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task<string?> GetStringAsync(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetAsync<T>(string key);
    Task<long> IncrementAttemptsAsync(string key, TimeSpan window);
    Task<bool> ExistsAsync(string key);
    Task RemoveAsync(string key);
    Task ClearAllAsync();

}
