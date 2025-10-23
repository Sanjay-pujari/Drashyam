using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class PremiumVideo
{
    public int Id { get; set; }

    [Required]
    public int VideoId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;

    public virtual ICollection<PremiumPurchase> Purchases { get; set; } = new List<PremiumPurchase>();
}

public class PremiumPurchase
{
    public int Id { get; set; }

    [Required]
    public int PremiumVideoId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;

    public PremiumPurchaseStatus Status { get; set; } = PremiumPurchaseStatus.Pending;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    // Navigation properties
    [ForeignKey("PremiumVideoId")]
    public virtual PremiumVideo PremiumVideo { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}

