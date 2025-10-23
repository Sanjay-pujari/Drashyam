using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class AdCampaign
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string AdvertiserId { get; set; } = string.Empty;

    public AdType Type { get; set; }

    public decimal Budget { get; set; }

    public decimal CostPerClick { get; set; }

    public decimal CostPerView { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public AdStatus Status { get; set; } = AdStatus.Draft;

    public string? TargetAudience { get; set; } // JSON string for targeting criteria

    public string? AdContent { get; set; } // JSON string for ad content

    public string? AdUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("AdvertiserId")]
    public virtual ApplicationUser Advertiser { get; set; } = null!;

    public virtual ICollection<AdImpression> Impressions { get; set; } = new List<AdImpression>();
}

public class AdImpression
{
    public int Id { get; set; }

    [Required]
    public int AdCampaignId { get; set; }

    public int? VideoId { get; set; }

    public string? UserId { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public bool WasClicked { get; set; } = false;

    public DateTime? ClickedAt { get; set; }

    public decimal Revenue { get; set; }

    // Navigation properties
    [ForeignKey("AdCampaignId")]
    public virtual AdCampaign AdCampaign { get; set; } = null!;

    [ForeignKey("VideoId")]
    public virtual Video? Video { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}

