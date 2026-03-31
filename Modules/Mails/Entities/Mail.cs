using Joblify.Modules.Mails.Enums;

namespace Joblify.Modules.Mails.Entities;

public class Mail
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // TODO: Add Mail-specific properties here
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public MailStatus Status { get; set; } = MailStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
