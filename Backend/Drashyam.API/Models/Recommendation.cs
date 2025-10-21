using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Recommendation
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int VideoId { get; set; }

    public RecommendationType Type { get; set; }

    public decimal Score { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public bool IsShown { get; set; } = false;

    public bool IsClicked { get; set; } = false;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;
}

public enum RecommendationType
{
    Trending,
    Personalized,
    Similar,
    Category,
    Channel,
    Popular,
    Recent
}
