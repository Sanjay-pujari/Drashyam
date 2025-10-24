using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class AdDto
{
    public int Id { get; set; }
    public AdType Type { get; set; }
    public string? Content { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerView { get; set; }
    public int Duration { get; set; } = 30; // Default 30 seconds
    public int SkipAfter { get; set; } = 5; // Default 5 seconds before skip
    public int? Position { get; set; } // For mid-roll ads
}

public class AdServeResponseDto
{
    public bool HasAd { get; set; }
    public AdDto? Ad { get; set; }
    public string? AdType { get; set; }
}

public class AdCampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AdvertiserId { get; set; } = string.Empty;
    public AdType Type { get; set; }
    public decimal Budget { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerView { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DTOs.AdStatus Status { get; set; }
    public string? TargetAudience { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public UserDto? Advertiser { get; set; }
    public List<AdImpressionDto>? Impressions { get; set; }
    
    // Analytics properties
    public decimal Spent { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class AdCampaignCreateDto
{
    [Required(ErrorMessage = "Campaign name is required")]
    [MaxLength(200, ErrorMessage = "Campaign name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Ad type is required")]
    public AdType Type { get; set; }

    [Required(ErrorMessage = "Budget is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget must be greater than 0")]
    public decimal Budget { get; set; }

    [Required(ErrorMessage = "Cost per click is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Cost per click must be 0 or greater")]
    public decimal CostPerClick { get; set; }

    [Required(ErrorMessage = "Cost per view is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Cost per view must be 0 or greater")]
    public decimal CostPerView { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    public string? TargetAudience { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class AdCampaignUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public AdType? Type { get; set; }
    public decimal? Budget { get; set; }
    public decimal? CostPerClick { get; set; }
    public decimal? CostPerView { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DTOs.AdStatus? Status { get; set; }
    public string? TargetAudience { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class AdImpressionDto
{
    public int Id { get; set; }
    public int AdCampaignId { get; set; }
    public int? VideoId { get; set; }
    public string? UserId { get; set; }
    public DateTime ViewedAt { get; set; }
    public bool WasClicked { get; set; }
    public DateTime? ClickedAt { get; set; }
    public decimal Revenue { get; set; }
    public UserDto? User { get; set; }
    public VideoDto? Video { get; set; }
}

public class AdRevenueDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal RevenuePerImpression { get; set; }
    public decimal RevenuePerClick { get; set; }
}

public class AdAnalyticsDto
{
    public int CampaignId { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerImpression { get; set; }
}

public class AdCampaignAnalyticsDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public decimal Spent { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerImpression { get; set; }
    public decimal ReturnOnInvestment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DTOs.AdStatus Status { get; set; }
}