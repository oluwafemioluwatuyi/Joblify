using Joblify.Modules.Mails.DTOs;
using Joblify.Modules.Mails.Entities;
using Joblify.Modules.Mails.Interfaces;

namespace Joblify.Modules.Mails.Services;

public class MailService : IMailService
{
    public MailService()
    {

    }

    public async Task SendAsync(string to, string subject, string templateName, Dictionary<string, string> model)
    {

    }
}
