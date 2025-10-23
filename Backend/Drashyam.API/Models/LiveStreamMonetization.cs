using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class LiveStreamDonation
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string DonorId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [MaxLength(500)]
    public string? Message { get; set; }
    
    public bool IsAnonymous { get; set; } = false;
    
    public bool IsHighlighted { get; set; } = false;
    
    public DateTime DonatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser Donor { get; set; } = null!;
}

public class LiveStreamSuperChat
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    [MaxLength(200)]
    public string Message { get; set; } = string.Empty;
    
    public SuperChatTier Tier { get; set; }
    
    public int DurationSeconds { get; set; } = 0;
    
    public bool IsHighlighted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class LiveStreamSubscription
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string SubscriberId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    public SubscriptionTier Tier { get; set; }
    
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser Subscriber { get; set; } = null!;
}

public class LiveStreamRevenue
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    public decimal TotalRevenue { get; set; } = 0;
    
    public decimal DonationRevenue { get; set; } = 0;
    
    public decimal SuperChatRevenue { get; set; } = 0;
    
    public decimal SubscriptionRevenue { get; set; } = 0;
    
    public decimal AdRevenue { get; set; } = 0;
    
    public decimal PlatformFee { get; set; } = 0;
    
    public decimal CreatorEarnings { get; set; } = 0;
    
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
}

