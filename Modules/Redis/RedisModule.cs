using Microsoft.Extensions.DependencyInjection;
using Joblify.Modules.Redis.Services;

namespace Joblify.Modules.Redis;

public static class RedisModule
{
    public static IServiceCollection AddRedisModule(this IServiceCollection services)
    {
        services.AddScoped<IRedisService, RedisService>();

        return services;
    }
}
