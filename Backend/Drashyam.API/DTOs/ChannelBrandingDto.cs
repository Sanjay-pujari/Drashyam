namespace Drashyam.API.DTOs;

public class ChannelBrandingDto
{
    public int Id { get; set; }
    public int ChannelId { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? CustomDomain { get; set; }
    public string? CustomCss { get; set; }
    public string? AboutText { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialMediaLinks { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ChannelDto? Channel { get; set; }
}

public class ChannelBrandingCreateDto
{
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? CustomDomain { get; set; }
    public string? CustomCss { get; set; }
    public string? AboutText { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialMediaLinks { get; set; }
}

public class ChannelBrandingUpdateDto
{
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? CustomDomain { get; set; }
    public string? CustomCss { get; set; }
    public string? AboutText { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialMediaLinks { get; set; }
    public bool? IsActive { get; set; }
}
