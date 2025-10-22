using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class SuperChat
{
    public int Id { get; set; }

    [Required]
    public int LiveStreamId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DonorName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? DonorMessage { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(100)]
    public string? PaymentIntentId { get; set; }

    public int DisplayDuration { get; set; } = 5; // Seconds to display

    [MaxLength(500)]
    public string? DonorAvatar { get; set; }

    public bool IsAnonymous { get; set; } = false;

    // Navigation properties
    [ForeignKey("LiveStreamId")]
    public virtual LiveStream LiveStream { get; set; } = null!;
}
