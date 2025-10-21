using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class UserInteraction
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int VideoId { get; set; }

    public InteractionType Type { get; set; }

    public decimal Score { get; set; } = 1.0m;

    public TimeSpan? WatchDuration { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;
}

public enum InteractionType
{
    View,
    Like,
    Dislike,
    Comment,
    Share,
    WatchLater,
    Subscribe,
    Unsubscribe,
    Skip,
    Complete
}
