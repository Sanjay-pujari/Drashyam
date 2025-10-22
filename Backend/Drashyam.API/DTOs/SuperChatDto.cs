namespace Drashyam.API.DTOs;

public class SuperChatDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? DonorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? PaymentIntentId { get; set; }
    public int DisplayDuration { get; set; } = 5; // Seconds to display
    public string? DonorAvatar { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class SuperChatRequestDto
{
    public int LiveStreamId { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? DonorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethodId { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; } = false;
}

public class SuperChatDisplayDto
{
    public int Id { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? DonorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; }
    public string? DonorAvatar { get; set; }
    public bool IsAnonymous { get; set; }
    public int DisplayDuration { get; set; }
}
