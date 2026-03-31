using Joblify.Modules.Mails.DTOs;

namespace Joblify.Modules.Mails.Interfaces;

public interface IMailService
{
    Task SendAsync(string to, string subject, string templateName, Dictionary<string, string> model);


}
