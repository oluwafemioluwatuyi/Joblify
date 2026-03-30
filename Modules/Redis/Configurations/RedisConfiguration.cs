namespace Joblify.Modules.Redis.Configurations;

public class RedisConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Ssl { get; set; } = false;
    public string InstancePrefix { get; set; } = "Joblify:";
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public int KeepAlive { get; set; } = 60;
    public int Database { get; set; } = 0;
    public RetryPolicyOptions RetryPolicy { get; set; } = new();
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();
    public int DefaultExpirationSeconds { get; set; } = 3600;
}


public class RetryPolicyOptions
{
    public int MaxRetries { get; set; } = 3;
    public int BackoffMilliseconds { get; set; } = 1000;
    public bool UseExponentialBackoff { get; set; } = true;
    public bool UseJitter { get; set; } = true;
}

public class CircuitBreakerOptions
{
    public int FailureThreshold { get; set; } = 5;
    public int SamplingDurationSeconds { get; set; } = 30;
    public int MinimumThroughput { get; set; } = 10;
    public int BreakDurationSeconds { get; set; } = 30;
}