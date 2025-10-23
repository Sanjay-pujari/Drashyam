using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class LiveStreamDonationDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string DonorId { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    public string DonorAvatar { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsAnonymous { get; set; }
    public bool IsHighlighted { get; set; }
    public DateTime DonatedAt { get; set; }
}

public class SendDonationDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [MaxLength(500)]
    public string? Message { get; set; }
    
    public bool IsAnonymous { get; set; } = false;
}

public class LiveStreamSuperChatDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SuperChatTier Tier { get; set; }
    public int DurationSeconds { get; set; }
    public bool IsHighlighted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class SendSuperChatDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
    
    [Required]
    [MaxLength(200)]
    public string Message { get; set; } = string.Empty;
    
    public SuperChatTier Tier { get; set; }
}

public class LiveStreamSubscriptionDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string SubscriberId { get; set; } = string.Empty;
    public string SubscriberName { get; set; } = string.Empty;
    public string SubscriberAvatar { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public SubscriptionTier Tier { get; set; }
    public DateTime SubscribedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

public class SubscribeToLiveStreamDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public SubscriptionTier Tier { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";
}

public class LiveStreamRevenueDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DonationRevenue { get; set; }
    public decimal SuperChatRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal CreatorEarnings { get; set; }
    public DateTime CalculatedAt { get; set; }
}

public class LiveStreamMonetizationStatsDto
{
    public int LiveStreamId { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal DonationRevenue { get; set; }
    public decimal SuperChatRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public int TotalDonations { get; set; }
    public int TotalSuperChats { get; set; }
    public int TotalSubscriptions { get; set; }
    public decimal AverageDonation { get; set; }
    public decimal AverageSuperChat { get; set; }
    public decimal AverageSubscription { get; set; }
    public DateTime LastUpdated { get; set; }
}
