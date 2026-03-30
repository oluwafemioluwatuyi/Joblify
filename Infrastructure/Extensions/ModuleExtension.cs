using System.Reflection;

namespace Joblify.Infrastructure.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        // If no assemblies passed, scan all loaded assemblies
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : AppDomain.CurrentDomain.GetAssemblies();

        var moduleTypes = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModule).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToList();

        foreach (var moduleType in moduleTypes)
        {
            var module = Activator.CreateInstance(moduleType) as IModule;
            module?.RegisterModule(services, configuration);
        }

        return services;
    }

    public interface IModule
    {
        IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration);
    }
}