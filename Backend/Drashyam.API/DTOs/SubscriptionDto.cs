namespace Drashyam.API.DTOs;

public class SubscriptionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethodId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public SubscriptionPlanDto? Plan { get; set; }
    public UserDto? User { get; set; }
}

public class SubscriptionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public int MaxChannels { get; set; }
    public int MaxVideosPerChannel { get; set; }
    public int MaxStorageGB { get; set; }
    public bool HasAds { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasMonetization { get; set; }
    public bool HasLiveStreaming { get; set; }
    public bool IsActive { get; set; }
}

public class SubscriptionCreateDto
{
    public int SubscriptionPlanId { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class SubscriptionUpdateDto
{
    public int? SubscriptionPlanId { get; set; }
    public string? PaymentMethodId { get; set; }
}

public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended
}

public class SubscriptionHistoryDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class SubscriptionAnalyticsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int CancelledSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
    public List<SubscriptionPlanAnalyticsDto> PlanAnalytics { get; set; } = new();
}

public class SubscriptionPlanAnalyticsDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int SubscriberCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRevenuePerUser { get; set; }
}