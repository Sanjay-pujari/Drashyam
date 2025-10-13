namespace Drashyam.API.DTOs;

public class PaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethodId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SubscriptionPaymentDto
{
    public int SubscriptionPlanId { get; set; }
    public string PaymentMethodId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PaymentHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PaymentMethodId { get; set; }
}

public class CustomerUpdateDto
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
