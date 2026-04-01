using Joblify.Modules.Mail.Services;
using Joblify.Modules.Mail.Interfaces;
using RazorLight;
using static Joblify.Infrastructure.Extensions.ModuleExtensions;
using Joblify.Modules.Mail.Configurations;

namespace Joblify.Modules.Mail;

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
                    Path.Combine(Directory.GetCurrentDirectory(), "Modules", "Mail", "Templates"))
                .UseMemoryCachingProvider()
                .Build()
        );

        services.AddScoped<IMailTemplateService, MailTemplateService>();
        services.AddScoped<IMailervice, Mailervice>();

        return services;
    }
}