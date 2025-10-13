namespace Drashyam.API.DTOs;

public class MonetizationStatusDto
{
    public bool IsEnabled { get; set; }
    public bool IsEligible { get; set; }
    public List<string> EligibilityRequirements { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public DateTime? EnabledAt { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected, Suspended
    public string? RejectionReason { get; set; }
}

public class MonetizationRequestDto
{
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankRoutingNumber { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string BusinessPhone { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;
}

public class AdPlacementDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public AdType Type { get; set; }
    public int Position { get; set; } // For mid-roll ads, this is the time in seconds
    public bool IsActive { get; set; }
    public decimal Revenue { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdPlacementRequestDto
{
    public int VideoId { get; set; }
    public AdType Type { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SponsorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<int> VideoIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SponsorRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public IFormFile? LogoFile { get; set; }
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int> VideoIds { get; set; } = new();
}

public class DonationDto
{
    public int Id { get; set; }
    public string DonorName { get; set; } = string.Empty;
    public string? DonorEmail { get; set; }
    public string? DonorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed, Refunded
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? PaymentIntentId { get; set; }
}

public class DonationRequestDto
{
    public string DonorName { get; set; } = string.Empty;
    public string? DonorEmail { get; set; }
    public string? DonorMessage { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethodId { get; set; } = string.Empty;
}

public class MerchandiseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string ImageUrl { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Sizes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class MerchandiseRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public IFormFile? ImageFile { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public string Category { get; set; } = string.Empty;
    public List<string> Sizes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
}

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SponsorRevenue { get; set; }
    public decimal DonationRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public List<RevenueBreakdownDto> Breakdown { get; set; } = new();
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public DateTime ReportDate { get; set; }
}

public class RevenueBreakdownDto
{
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Percentage { get; set; }
    public int TransactionCount { get; set; }
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}

public enum AdType
{
    PreRoll,
    MidRoll,
    PostRoll,
    Banner,
    Overlay
}
