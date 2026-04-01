using System.ComponentModel.DataAnnotations;

namespace Joblify.Modules.Mail.DTOs;

public class MailRequest
{
    [Required]
    [EmailAddress]
    public string ToEmail { get; set; } = string.Empty;

    public string? ToName { get; set; }

    [Required]
    public string Subject { get; set; } = string.Empty;

    public string? PlainTextContent { get; set; }

    public string? HtmlContent { get; set; }

    public List<string> CcEMail { get; set; } = new();

    public List<string> BccEMail { get; set; } = new();

    public Dictionary<string, byte[]> Attachments { get; set; } = new();
}

public class TemplateMailRequest : MailRequest
{
    [Required]
    public string TemplateName { get; set; } = string.Empty;

    public Dictionary<string, object> TemplateModel { get; set; } = new();
}