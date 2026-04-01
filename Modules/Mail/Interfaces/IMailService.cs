using Joblify.Modules.Mail.DTOs;

namespace Joblify.Modules.Mail.Interfaces;

public interface IMailervice
{
    Task SendEmailAsync(MailRequest request);
    Task SendTemplateEmailAsync(TemplateMailRequest request);
    Task LoginNotificationAsync(string email, string userName, Dictionary<string, object> model);
    Task PasswordResetNotificationAsync(string email, string userName, Dictionary<string, object> model);
    Task WelcomeEmailAsync(string email, string userName, Dictionary<string, object> model);
}
