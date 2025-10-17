using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class UserInvite
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string InviterId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string InviteeEmail { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? InviteeFirstName { get; set; }

    [MaxLength(100)]
    public string? InviteeLastName { get; set; }

    [Required]
    [MaxLength(100)]
    public string InviteToken { get; set; } = string.Empty;

    public InviteStatus Status { get; set; } = InviteStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcceptedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? PersonalMessage { get; set; }

    public InviteType Type { get; set; } = InviteType.Email;

    [MaxLength(450)]
    public string? AcceptedUserId { get; set; }

    // Navigation properties
    public virtual ApplicationUser Inviter { get; set; } = null!;
    public virtual ApplicationUser? AcceptedUser { get; set; }
}

public enum InviteStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled
}

public enum InviteType
{
    Email,
    Social,
    DirectLink
}
