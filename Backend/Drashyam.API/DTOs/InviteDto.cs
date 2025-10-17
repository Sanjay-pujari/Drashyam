using Drashyam.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class UserInviteDto
{
    public int Id { get; set; }
    public string InviterId { get; set; } = string.Empty;
    public string InviterName { get; set; } = string.Empty;
    public string InviteeEmail { get; set; } = string.Empty;
    public string? InviteeFirstName { get; set; }
    public string? InviteeLastName { get; set; }
    public string InviteToken { get; set; } = string.Empty;
    public InviteStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? PersonalMessage { get; set; }
    public InviteType Type { get; set; }
    public string? AcceptedUserId { get; set; }
    public string? AcceptedUserName { get; set; }
}

public class CreateInviteDto
{
    [Required]
    [EmailAddress]
    public string InviteeEmail { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? InviteeFirstName { get; set; }

    [MaxLength(100)]
    public string? InviteeLastName { get; set; }

    [MaxLength(500)]
    public string? PersonalMessage { get; set; }

    public InviteType Type { get; set; } = InviteType.Email;

    public int? ExpirationDays { get; set; } = 7;
}

public class AcceptInviteDto
{
    [Required]
    public string InviteToken { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public class InviteStatsDto
{
    public int TotalInvites { get; set; }
    public int PendingInvites { get; set; }
    public int AcceptedInvites { get; set; }
    public int ExpiredInvites { get; set; }
    public decimal ConversionRate { get; set; }
}

public class BulkInviteDto
{
    [Required]
    public List<CreateInviteDto> Invites { get; set; } = new();
}

public class InviteLinkDto
{
    public string InviteLink { get; set; } = string.Empty;
    public string InviteToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int UsageCount { get; set; }
    public int MaxUsage { get; set; }
}
