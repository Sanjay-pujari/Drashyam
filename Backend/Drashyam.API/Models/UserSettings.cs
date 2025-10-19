using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class UserSettings
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Privacy Settings
    public bool ProfilePublic { get; set; } = true;
    public bool ShowEmail { get; set; } = false;
    public bool AllowDataSharing { get; set; } = true;

    // Notification Settings
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool NewVideoNotifications { get; set; } = true;
    public bool CommentNotifications { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}
