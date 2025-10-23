using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class SocialShareDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int VideoId { get; set; }
    public SharePlatform Platform { get; set; }
    public string? CustomMessage { get; set; }
    public string? Hashtags { get; set; }
    public string ShareUrl { get; set; } = string.Empty;
    public string? ExternalPostId { get; set; }
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public bool IsScheduled { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PostedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SocialShareCreateDto
{
    [Required]
    public int VideoId { get; set; }
    
    [Required]
    public SharePlatform Platform { get; set; }
    
    [MaxLength(500)]
    public string? CustomMessage { get; set; }
    
    [MaxLength(200)]
    public string? Hashtags { get; set; }
    
    public bool IsScheduled { get; set; } = false;
    public DateTime? ScheduledAt { get; set; }
}

public class ViralContentDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ViralStatus Status { get; set; }
    public ViralTrigger Trigger { get; set; }
    public int ViewVelocity { get; set; }
    public int ShareVelocity { get; set; }
    public int EngagementVelocity { get; set; }
    public decimal ViralScore { get; set; }
    public int PeakViews { get; set; }
    public int PeakShares { get; set; }
    public int PeakEngagement { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? PeakAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationHours { get; set; }
    public int TotalViews { get; set; }
    public int TotalShares { get; set; }
    public int TotalEngagement { get; set; }
}

public class ContentPromotionDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromotionType Type { get; set; }
    public PromotionStatus Status { get; set; }
    public decimal Budget { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal SpentAmount { get; set; }
    public int TargetViews { get; set; }
    public int TargetClicks { get; set; }
    public int TargetEngagement { get; set; }
    public int ActualViews { get; set; }
    public int ActualClicks { get; set; }
    public int ActualEngagement { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ContentPromotionCreateDto
{
    [Required]
    public int VideoId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public PromotionType Type { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }
    
    public string Currency { get; set; } = "USD";
    
    [Range(0, int.MaxValue)]
    public int TargetViews { get; set; }
    
    [Range(0, int.MaxValue)]
    public int TargetClicks { get; set; }
    
    [Range(0, int.MaxValue)]
    public int TargetEngagement { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
}

public class SocialConnectionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public SharePlatform Platform { get; set; }
    public string PlatformUserId { get; set; } = string.Empty;
    public string PlatformUsername { get; set; } = string.Empty;
    public DateTime TokenExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class SocialConnectionCreateDto
{
    [Required]
    public SharePlatform Platform { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string PlatformUserId { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string PlatformUsername { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string AccessToken { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    [Required]
    public DateTime TokenExpiresAt { get; set; }
}

public class SocialAnalyticsDto
{
    public int TotalShares { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public int TotalEngagement { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ROAS { get; set; } // Return on Ad Spend
    public List<PlatformStats> PlatformStats { get; set; } = new();
    public List<MonthlySocialStats> MonthlyStats { get; set; } = new();
    public List<TopViralContent> TopViralContent { get; set; } = new();
}

public class PlatformStats
{
    public SharePlatform Platform { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
    public int Clicks { get; set; }
    public int Engagement { get; set; }
    public decimal Spent { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlySocialStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Shares { get; set; }
    public int Views { get; set; }
    public int Clicks { get; set; }
    public int Engagement { get; set; }
    public decimal Spent { get; set; }
    public decimal Revenue { get; set; }
}

public class TopViralContent
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal ViralScore { get; set; }
    public int Views { get; set; }
    public int Shares { get; set; }
    public int Engagement { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SocialFilterDto
{
    public SharePlatform? Platform { get; set; }
    public PromotionType? Type { get; set; }
    public PromotionStatus? Status { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
