namespace Joblify.Modules.Redis.Interfaces;

public interface IRedisStartupService
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}