using Microsoft.Extensions.DependencyInjection;
using Joblify.Modules.Users.Repositories;
using Joblify.Modules.Users.Services;

namespace Joblify.Modules.Users;

public static class UserModule
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
