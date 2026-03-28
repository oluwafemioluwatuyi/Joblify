using Joblify.Modules.Users.Enums;

namespace Joblify.Modules.Users.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // TODO: Add User-specific properties here
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
