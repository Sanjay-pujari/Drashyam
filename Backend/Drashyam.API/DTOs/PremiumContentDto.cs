using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class PremiumVideoDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoThumbnailUrl { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PremiumVideoCreateDto
{
    public int VideoId { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
}

public class PremiumVideoUpdateDto
{
    public string CreatorId { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public bool? IsActive { get; set; }
}

public class PremiumPurchaseDto
{
    public int Id { get; set; }
    public int PremiumVideoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string VideoTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public PremiumPurchaseStatus Status { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
}

public class PremiumPurchaseCreateDto
{
    public int PremiumVideoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class PremiumContentAnalyticsDto
{
    public int PremiumVideoId { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
