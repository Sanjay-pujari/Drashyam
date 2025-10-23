using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class MerchandiseStore
{
    public int Id { get; set; }

    [Required]
    public int ChannelId { get; set; }

    [Required]
    [MaxLength(100)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    public StorePlatform Platform { get; set; }

    [Required]
    [MaxLength(500)]
    public string StoreUrl { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } = null!;
}

