using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Joblify.Modules.Redis.Configurations;
using Joblify.Modules.Redis.Interfaces;

namespace Joblify.Modules.Redis.Services;

public class RedisStartupService : IRedisStartupService, IHostedService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisConfiguration _config;
    private readonly ILogger<RedisStartupService> _logger;

    public RedisStartupService(
        IConnectionMultiplexer redis,
        IOptions<RedisConfiguration> config,
        ILogger<RedisStartupService> logger)
    {
        _redis = redis;
        _config = config.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Redis startup check beginning...");

        int attempt = 0;
        int maxRetries = _config.RetryPolicy.MaxRetries;

        while (attempt <= maxRetries)
        {
            try
            {
                var db = _redis.GetDatabase(_config.Database);
                var pingResult = await db.PingAsync();

                _logger.LogInformation("Redis connected successfully. Latency: {Latency}ms", pingResult.TotalMilliseconds);
                return;
            }
            catch (RedisConnectionException ex)
            {
                attempt++;

                if (attempt > maxRetries)
                {
                    _logger.LogCritical(ex, "Redis connection failed after {MaxRetries} retries. Startup aborted.", maxRetries);
                    throw; // Prevent app from starting if Redis is required
                }

                var delay = CalculateDelay(attempt);
                _logger.LogWarning(ex, "Redis connection attempt {Attempt}/{MaxRetries} failed. Retrying in {Delay}ms...",
                    attempt, maxRetries, delay);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Redis service stopping...");
        _redis.Close();
        return Task.CompletedTask;
    }

    private int CalculateDelay(int attempt)
    {
        var backoff = _config.RetryPolicy.BackoffMilliseconds;

        if (_config.RetryPolicy.UseExponentialBackoff)
            backoff = (int)(backoff * Math.Pow(2, attempt - 1));

        if (_config.RetryPolicy.UseJitter)
            backoff += Random.Shared.Next(0, 200); // add up to 200ms jitter

        return backoff;
    }
}