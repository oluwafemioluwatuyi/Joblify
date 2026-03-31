using System.ComponentModel.DataAnnotations;
using Joblify.Modules.Mails.Enums;

namespace Joblify.Modules.Mails.DTOs;

// ── Response DTO ──────────────────────────────────────────────────────────────
public class MailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── Create DTO ────────────────────────────────────────────────────────────────
public class CreateMailDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}

// ── Update DTO ────────────────────────────────────────────────────────────────
public class UpdateMailDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public MailStatus? Status { get; set; }
}
