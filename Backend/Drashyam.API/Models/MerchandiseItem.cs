using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class MerchandiseItem
{
    public int Id { get; set; }

    [Required]
    public int ChannelId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Sizes { get; set; } // JSON string for available sizes

    [MaxLength(500)]
    public string? Colors { get; set; } // JSON string for available colors

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } = null!;

    public virtual ICollection<MerchandiseOrder> Orders { get; set; } = new List<MerchandiseOrder>();
}

public class MerchandiseOrder
{
    public int Id { get; set; }

    [Required]
    public int MerchandiseItemId { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CustomerEmail { get; set; }

    [MaxLength(500)]
    public string? CustomerAddress { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    public int Quantity { get; set; } = 1;

    [MaxLength(50)]
    public string? Size { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;

    public MerchandiseOrderStatus Status { get; set; } = MerchandiseOrderStatus.Pending;

    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    [MaxLength(100)]
    public string? TrackingNumber { get; set; }

    // Navigation properties
    [ForeignKey("MerchandiseItemId")]
    public virtual MerchandiseItem MerchandiseItem { get; set; } = null!;

    [ForeignKey("CustomerId")]
    public virtual ApplicationUser Customer { get; set; } = null!;
}

