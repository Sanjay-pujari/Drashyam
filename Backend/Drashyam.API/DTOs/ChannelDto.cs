namespace Drashyam.API.DTOs;

public class NotificationPreferenceDto
{
    public bool NotificationsEnabled { get; set; }
}

public class ChannelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public long SubscriberCount { get; set; }
    public long VideoCount { get; set; }
    public long TotalViews { get; set; }
    public bool IsVerified { get; set; }
    public bool IsMonetized { get; set; }
    public ChannelType Type { get; set; }
    public int MaxVideos { get; set; }
    public int CurrentVideoCount { get; set; }
    public string? CustomUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
    public UserDto? User { get; set; }
    public bool IsSubscribed { get; set; }
}

public class ChannelCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChannelType Type { get; set; } = ChannelType.Personal;
    public string? CustomUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}

public class ChannelUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ChannelType? Type { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? SocialLinks { get; set; }
}
