using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class AdCampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AdvertiserId { get; set; } = string.Empty;
    public string AdvertiserName { get; set; } = string.Empty;
    public AdType Type { get; set; }
    public decimal Budget { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerView { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AdStatus Status { get; set; }
    public string? TargetAudience { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AdCampaignCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AdvertiserId { get; set; } = string.Empty;
    public AdType Type { get; set; }
    public decimal Budget { get; set; }
    public decimal CostPerClick { get; set; }
    public decimal CostPerView { get; set; }
    public DateTime StartDate { get; set; }
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
    public AdStatus? Status { get; set; }
    public string? TargetAudience { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class AdRequestDto
{
    public string? UserId { get; set; }
    public int? VideoId { get; set; }
    public string? Category { get; set; }
    public string? Location { get; set; }
    public string? DeviceType { get; set; }
}

public class AdServeDto
{
    public bool HasAd { get; set; }
    public int? CampaignId { get; set; }
    public AdType? AdType { get; set; }
    public string? AdContent { get; set; }
    public string? AdUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal? CostPerClick { get; set; }
    public decimal? CostPerView { get; set; }
}

public class AdAnalyticsDto
{
    public int CampaignId { get; set; }
    public int TotalImpressions { get; set; }
    public int TotalClicks { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
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
}
