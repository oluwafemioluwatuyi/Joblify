using Microsoft.Extensions.Options;
using Joblify.Modules.Redis.Configurations;
using Joblify.Modules.Redis.Services;
using Joblify.Modules.Redis.Interfaces;
using static Joblify.Infrastructure.Extensions.ModuleExtensions;
using StackExchange.Redis;

namespace Joblify.Modules.Redis;

public class RedisModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {

        static string getEnv(string key, string defaultValue) => Environment.GetEnvironmentVariable(key) ?? defaultValue;
        static int getEnvInt(string key, int defaultValue) => int.TryParse(Environment.GetEnvironmentVariable(key), out var result) ? result : defaultValue;
        static bool getEnvBool(string key, bool defaultValue) => bool.TryParse(Environment.GetEnvironmentVariable(key), out var result) ? result : defaultValue;

        //Map variables to the strongly-typed class
        services.Configure<RedisConfiguration>(options =>
        {
            options.ConnectionString = getEnv("REDIS_CONNECTION_STRING", "localhost:6379");
            options.InstanceName = getEnv("REDIS_INSTANCE_NAME", "");
            options.Password = getEnv("REDIS_PASSWORD", "");
            options.Ssl = getEnvBool("REDIS_SSL", false);
            options.Database = getEnvInt("REDIS_DATABASE", 0);
            options.ConnectTimeout = getEnvInt("REDIS_CONNECT_TIMEOUT", 5000);
            options.SyncTimeout = getEnvInt("REDIS_SYNC_TIMEOUT", 5000);
            options.KeepAlive = getEnvInt("REDIS_KEEP_ALIVE", 60);

            // 
            options.RetryPolicy = new RetryPolicyOptions
            {
                MaxRetries = getEnvInt("REDIS_RETRY_MAX_RETRIES", 3),
                BackoffMilliseconds = getEnvInt("REDIS_RETRY_BACKOFF_MS", 1000),
                UseExponentialBackoff = getEnvBool("REDIS_RETRY_EXP_BACKOFF", true),
                UseJitter = getEnvBool("REDIS_RETRY_JITTER", true)
            };

            // Circuit Breaker settings
            options.CircuitBreaker = new CircuitBreakerOptions
            {
                FailureThreshold = getEnvInt("REDIS_CB_FAILURE_THRESHOLD", 5),
                SamplingDurationSeconds = getEnvInt("REDIS_CB_SAMPLING_DURATION", 30),
                MinimumThroughput = getEnvInt("REDIS_CB_MIN_THROUGHPUT", 10),
                BreakDurationSeconds = getEnvInt("REDIS_CB_BREAK_DURATION", 30)
            };
            options.DefaultExpirationSeconds = getEnvInt("REDIS_DEFAULT_EXPIRATION", 3600);
        });

        services.AddStackExchangeRedisCache(opts =>
        {
            var rc = services.BuildServiceProvider().GetRequiredService<IOptions<RedisConfiguration>>().Value;
            opts.Configuration = rc.ConnectionString;
            opts.InstanceName = rc.InstancePrefix;
        });

        // Register IConnectionMultiplexer (required by RedisService and RedisStartupService)
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var rc = sp.GetRequiredService<IOptions<RedisConfiguration>>().Value;

            var configOptions = new ConfigurationOptions
            {
                Password = rc.Password,
                Ssl = rc.Ssl,
                ConnectTimeout = rc.ConnectTimeout,
                SyncTimeout = rc.SyncTimeout,
                KeepAlive = rc.KeepAlive,
                AbortOnConnectFail = false, // Let startup service handle failures gracefully
            };

            foreach (var endpoint in rc.ConnectionString.Split(','))
                configOptions.EndPoints.Add(endpoint.Trim());

            return ConnectionMultiplexer.Connect(configOptions);
        });

        // Uncomment this now
        services.AddHostedService<RedisStartupService>();
        services.AddScoped<IRedisStartupService, RedisStartupService>();

        services.AddScoped<IRedisService, RedisService>();
        //services.AddScoped<IRedisStartupService, RedisStartupService>();
        return services;
    }
}
