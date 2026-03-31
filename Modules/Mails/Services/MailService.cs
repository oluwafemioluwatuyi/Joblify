using Joblify.Modules.Mails.DTOs;
using Joblify.Modules.Mails.Entities;
using Joblify.Modules.Mails.Interfaces;
using Joblify.Modules.Mails.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace Joblify.Modules.Mails.Services;

public class MailService : IMailService
{
    private readonly MailConfiguration _mailConfig;
    private readonly IMailTemplateService _templateService;

    public MailService(IOptions<MailConfiguration> mailConfig, IMailTemplateService templateService)
    {
        _mailConfig = mailConfig.Value;
        _templateService = templateService;
    }

    public async Task SendAsync(MailRequest request)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_mailConfig.FromName, _mailConfig.FromEmail));
        message.To.Add(new MailboxAddress(request.ToName, request.ToEmail));
        message.Subject = request.Subject;

        // Add CC
        foreach (var cc in request.CcEmails)
        {
            message.Cc.Add(new MailboxAddress(cc, cc));
        }

        // Add BCC
        foreach (var bcc in request.BccEmails)
        {
            message.Bcc.Add(new MailboxAddress(bcc, bcc));
        }

        // Handle template or direct content
        MimeEntity body;
        if (request is TemplateMailRequest templateRequest)
        {
            var htmlContent = await _templateService.RenderAsync(templateRequest.TemplateName, templateRequest.TemplateData);
            body = new TextPart("html") { Text = htmlContent };
        }
        else
        {
            if (!string.IsNullOrEmpty(request.HtmlContent))
            {
                body = new TextPart("html") { Text = request.HtmlContent };
            }
            else if (!string.IsNullOrEmpty(request.PlainTextContent))
            {
                body = new TextPart("plain") { Text = request.PlainTextContent };
            }
            else
            {
                throw new ArgumentException("Either HtmlContent, PlainTextContent, or a template must be provided.");
            }
        }

        // Handle attachments
        if (request.Attachments.Any())
        {
            var multipart = new Multipart("mixed");
            multipart.Add(body);

            foreach (var attachment in request.Attachments)
            {
                var attachmentPart = new MimePart()
                {
                    Content = new MimeContent(new MemoryStream(attachment.Value)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Key
                };
                multipart.Add(attachmentPart);
            }

            message.Body = multipart;
        }
        else
        {
            message.Body = body;
        }

        using var client = new SmtpClient();
        await client.ConnectAsync(_mailConfig.Host, _mailConfig.Port, _mailConfig.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
        await client.AuthenticateAsync(_mailConfig.Username, _mailConfig.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
