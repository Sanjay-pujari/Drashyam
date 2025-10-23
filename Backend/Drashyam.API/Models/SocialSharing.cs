using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class SocialShare
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int VideoId { get; set; }
    
    [Required]
    public SharePlatform Platform { get; set; }
    
    [MaxLength(500)]
    public string? CustomMessage { get; set; }
    
    [MaxLength(200)]
    public string? Hashtags { get; set; }
    
    public string ShareUrl { get; set; } = string.Empty;
    public string? ExternalPostId { get; set; }
    
    public int ViewCount { get; set; } = 0;
    public int ClickCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    
    public bool IsScheduled { get; set; } = false;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PostedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Video Video { get; set; } = null!;
}

public class ViralContent
{
    public int Id { get; set; }
    
    [Required]
    public int VideoId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public ViralStatus Status { get; set; }
    public ViralTrigger Trigger { get; set; }
    
    public int ViewVelocity { get; set; } = 0; // views per hour
    public int ShareVelocity { get; set; } = 0; // shares per hour
    public int EngagementVelocity { get; set; } = 0; // likes + comments per hour
    
    public decimal ViralScore { get; set; } = 0;
    public int PeakViews { get; set; } = 0;
    public int PeakShares { get; set; } = 0;
    public int PeakEngagement { get; set; } = 0;
    
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PeakAt { get; set; }
    public DateTime? EndedAt { get; set; }
    
    public int DurationHours { get; set; } = 0;
    public int TotalViews { get; set; } = 0;
    public int TotalShares { get; set; } = 0;
    public int TotalEngagement { get; set; } = 0;
    
    // Navigation properties
    public Video Video { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class ContentPromotion
{
    public int Id { get; set; }
    
    [Required]
    public int VideoId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public PromotionType Type { get; set; }
    public PromotionStatus Status { get; set; }
    
    public decimal Budget { get; set; } = 0;
    public string Currency { get; set; } = "USD";
    public decimal SpentAmount { get; set; } = 0;
    
    public int TargetViews { get; set; } = 0;
    public int TargetClicks { get; set; } = 0;
    public int TargetEngagement { get; set; } = 0;
    
    public int ActualViews { get; set; } = 0;
    public int ActualClicks { get; set; } = 0;
    public int ActualEngagement { get; set; } = 0;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Video Video { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class SocialConnection
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public SharePlatform Platform { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string PlatformUserId { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string PlatformUsername { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string AccessToken { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    public DateTime TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
}

