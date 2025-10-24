using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class Ad
{
    public int Id { get; set; }

    [Required]
    public int CampaignId { get; set; }

    public AdType Type { get; set; }

    [MaxLength(2000)]
    public string? Content { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public decimal CostPerClick { get; set; }

    public decimal CostPerView { get; set; }

    public int Duration { get; set; } = 30; // Duration in seconds

    public int SkipAfter { get; set; } = 5; // Seconds before skip button appears

    public int? Position { get; set; } // For mid-roll ads, video timestamp

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CampaignId")]
    public virtual AdCampaign Campaign { get; set; } = null!;
}
