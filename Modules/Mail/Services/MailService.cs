using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Joblify.Modules.Mail.Configurations;
using Joblify.Modules.Mail.DTOs;
using Joblify.Modules.Mail.Interfaces;
using Microsoft.Extensions.Logging;

namespace Joblify.Modules.Mail.Services;

public class Mailervice : IMailervice
{
    private readonly MailConfiguration _config;
    private readonly IMailTemplateService _templateService;
    private readonly ILogger<Mailervice> _logger;

    public Mailervice(
        IOptions<MailConfiguration> options,
        IMailTemplateService templateService,
        ILogger<Mailervice> logger)
    {
        _config = options.Value;
        _templateService = templateService;
        _logger = logger;
    }
    public async Task SendTemplateEmailAsync(TemplateMailRequest request)
    {
        _logger.LogInformation("Rendering email template {TemplateName} for {Email}", request.TemplateName, request.ToEmail);

        // 1. Generate the HTML body from the .cshtml file
        var htmlBody = await _templateService.RenderAsync(request.TemplateName, request.TemplateModel);

        // 2. Attach the rendered HTML to the request
        request.HtmlContent = htmlBody;

        // 3. Pass to the core sending logic
        await SendEmailAsync(request);
    }
    public async Task SendEmailAsync(MailRequest request)
    {
        var message = new MimeMessage();

        // Configure Sender
        message.From.Add(new MailboxAddress(_config.FromName, _config.FromEmail));

        // Configure Recipient
        message.To.Add(new MailboxAddress(request.ToName ?? request.ToEmail, request.ToEmail));

        // Add CC and BCC
        request.CcEMail.ForEach(email => message.Cc.Add(MailboxAddress.Parse(email)));
        request.BccEMail.ForEach(email => message.Bcc.Add(MailboxAddress.Parse(email)));

        message.Subject = request.Subject;

        // Construct Body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = request.HtmlContent,
            TextBody = request.PlainTextContent
        };

        // Handle Attachments
        foreach (var attachment in request.Attachments)
        {
            bodyBuilder.Attachments.Add(attachment.Key, attachment.Value);
        }

        message.Body = bodyBuilder.ToMessageBody();

        await SendInternalAsync(message);
    }
    public async Task LoginNotificationAsync(string email, string userName, Dictionary<string, object> model)
    {
        _logger.LogInformation("Sending login notification to {Email}", email);

        // Required fields
        model["UserName"] = userName;
        model["CompanyName"] = "Joblify";
        if (!model.ContainsKey("LoginTime"))
            model["LoginTime"] = DateTime.UtcNow.ToString("f");

        if (!model.ContainsKey("IpAddress"))
            model["IpAddress"] = "Unknown IP";

        if (!model.ContainsKey("UserAgent"))
            model["UserAgent"] = "Unknown Device";

        await SendTemplateEmailAsync(new TemplateMailRequest
        {
            ToEmail = email,
            ToName = userName,
            Subject = "New Login to Your Account",
            TemplateName = "LoginNotification",
            TemplateModel = model
        });
    }
    public async Task PasswordResetNotificationAsync(string email, string userName, Dictionary<string, object> model)
    {
        _logger.LogInformation("Sending password reset notification to {Email}", email);

        // Required fields
        model["UserName"] = userName;
        model["CompanyName"] = "Joblify";
        model["ResetLink"] = "https://joblify.com/reset-password";
        model["ExpiryTime"] = "15 minutes";

        await SendTemplateEmailAsync(new TemplateMailRequest
        {
            ToEmail = email,
            ToName = userName,
            Subject = "Password Reset Request",
            TemplateName = "PasswordResetNotification",
            TemplateModel = model
        });
    }
    public async Task WelcomeEmailAsync(string email, string userName, Dictionary<string, object> model)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        model["UserName"] = userName;

        if (!model.ContainsKey("CompanyName"))
            model["CompanyName"] = "Joblify";

        await SendTemplateEmailAsync(new TemplateMailRequest
        {
            ToEmail = email,
            ToName = userName,
            Subject = "Welcome to Joblify!",
            TemplateName = "WelcomeEmail",
            TemplateModel = model
        });
    }

    private async Task SendInternalAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        try
        {
            // Connect using the security options defined in your config
            var secureOptions = _config.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            await client.ConnectAsync(_config.Host, _config.Port, secureOptions);

            if (!string.IsNullOrEmpty(_config.Username))
            {
                await client.AuthenticateAsync(_config.Username, _config.Password);
            }

            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP Error: Failed to send email to {Recipient}", message.To);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}