using Joblify.Modules.Mail.Interfaces;
using RazorLight;

namespace Joblify.Modules.Mail.Services;

public class MailTemplateService : IMailTemplateService
{
    private readonly IRazorLightEngine _razorEngine;
    private readonly string _templatesPath;

    public MailTemplateService(IRazorLightEngine razorEngine)
    {
        _razorEngine = razorEngine;
        _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), "Modules", "Mail", "Templates");
    }

    public async Task<string> RenderAsync(string templateName, Dictionary<string, object> model)
    {
        if (!await TemplateExistsAsync(templateName))
            throw new FileNotFoundException($"Mail template '{templateName}' was not found in {_templatesPath}");

        return await _razorEngine.CompileRenderAsync(templateName, model);
    }

    public Task<bool> TemplateExistsAsync(string templateName)
    {
        var templateFile = Path.Combine(_templatesPath, $"{templateName}.cshtml");
        return Task.FromResult(File.Exists(templateFile));
    }

    public IEnumerable<string> GetAllTemplates()
    {
        if (!Directory.Exists(_templatesPath))
            return Enumerable.Empty<string>();

        return Directory
            .GetFiles(_templatesPath, "*.cshtml")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name != null)
            .Cast<string>();
    }
}