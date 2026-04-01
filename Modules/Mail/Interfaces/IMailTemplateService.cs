namespace Joblify.Modules.Mail.Interfaces;

public interface IMailTemplateService
{
    Task<string> RenderAsync(string templateName, Dictionary<string, object> model);
    Task<bool> TemplateExistsAsync(string templateName);
    IEnumerable<string> GetAllTemplates();
}
