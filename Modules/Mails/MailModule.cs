using Joblify.Modules.Mails.Services;
using Joblify.Modules.Mails.Interfaces;
using RazorLight;
using static Joblify.Infrastructure.Extensions.ModuleExtensions;
using Joblify.Modules.Mails.Configurations;

namespace Joblify.Modules.Mails;

public class MailModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<MailConfiguration>()
            .Bind(configuration.GetSection(MailConfiguration.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IRazorLightEngine>(_ =>
            new RazorLightEngineBuilder()
                .UseFileSystemProject(
                    Path.Combine(Directory.GetCurrentDirectory(), "Modules", "Mails", "Templates"))
                .UseMemoryCachingProvider()
                .Build()
        );

        services.AddScoped<IMailTemplateService, MailTemplateService>();
        services.AddScoped<IMailService, MailService>();

        return services;
    }
}