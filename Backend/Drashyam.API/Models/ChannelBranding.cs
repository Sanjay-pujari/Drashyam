using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class ChannelBranding
{
    public int Id { get; set; }

    [Required]
    public int ChannelId { get; set; }

    [MaxLength(200)]
    public string? LogoUrl { get; set; }

    [MaxLength(200)]
    public string? BannerUrl { get; set; }

    [MaxLength(7)]
    public string? PrimaryColor { get; set; } // Hex color code

    [MaxLength(7)]
    public string? SecondaryColor { get; set; } // Hex color code

    [MaxLength(7)]
    public string? AccentColor { get; set; } // Hex color code

    [MaxLength(100)]
    public string? CustomDomain { get; set; }

    [MaxLength(500)]
    public string? CustomCss { get; set; }

    [MaxLength(1000)]
    public string? AboutText { get; set; }

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(200)]
    public string? SocialMediaLinks { get; set; } // JSON string

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } = null!;
}
