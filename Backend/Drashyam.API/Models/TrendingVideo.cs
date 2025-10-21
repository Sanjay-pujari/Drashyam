using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class TrendingVideo
{
    public int Id { get; set; }

    [Required]
    public int VideoId { get; set; }

    public string? Category { get; set; }

    public string? Country { get; set; }

    public decimal TrendingScore { get; set; }

    public int Position { get; set; }

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    // Navigation properties
    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;
}
