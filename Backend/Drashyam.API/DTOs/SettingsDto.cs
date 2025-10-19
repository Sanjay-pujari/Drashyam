using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class PrivacySettingsDto
{
    public bool ProfilePublic { get; set; } = true;
    public bool ShowEmail { get; set; } = false;
    public bool AllowDataSharing { get; set; } = true;
}

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool NewVideoNotifications { get; set; } = true;
    public bool CommentNotifications { get; set; } = true;
}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class DeleteAccountDto
{
    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmationText { get; set; } = string.Empty;
}
