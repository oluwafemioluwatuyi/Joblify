namespace Joblify.Modules.Mails.Interfaces;

public interface IMailTemplateService
{
    Task<string> RenderAsync(string templateName, Dictionary<string, string> model);
    Task<bool> TemplateExistsAsync(string templateName);
    IEnumerable<string> GetAllTemplates();
}
