using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class LiveStreamEventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorAvatar { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public EventType Type { get; set; }
    public EventStatus Status { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsPublic { get; set; }
    public bool IsMonetized { get; set; }
    public decimal TicketPrice { get; set; }
    public int MaxAttendees { get; set; }
    public int CurrentAttendees { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LiveStreamDto> LiveStreams { get; set; } = new List<LiveStreamDto>();
    public List<EventAttendeeDto> Attendees { get; set; } = new List<EventAttendeeDto>();
}

public class CreateLiveStreamEventDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public int? ChannelId { get; set; }
    
    [Required]
    public EventType Type { get; set; }
    
    [Required]
    public DateTime ScheduledStartTime { get; set; }
    
    public int DurationMinutes { get; set; } = 60;
    
    public bool IsPublic { get; set; } = true;
    
    public bool IsMonetized { get; set; } = false;
    
    public decimal TicketPrice { get; set; } = 0;
    
    public int MaxAttendees { get; set; } = 0;
    
    public string? ThumbnailUrl { get; set; }
    
    public string? Category { get; set; }
    
    public string? Tags { get; set; }
}

public class UpdateLiveStreamEventDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? ScheduledStartTime { get; set; }
    
    public int? DurationMinutes { get; set; }
    
    public bool? IsPublic { get; set; }
    
    public bool? IsMonetized { get; set; }
    
    public decimal? TicketPrice { get; set; }
    
    public int? MaxAttendees { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    
    public string? Category { get; set; }
    
    public string? Tags { get; set; }
}

public class EventAttendeeDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public AttendanceStatus Status { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public bool IsPaid { get; set; }
    public decimal AmountPaid { get; set; }
}

public class RegisterForEventDto
{
    [Required]
    public int EventId { get; set; }
    
    public bool IsPaid { get; set; } = false;
    
    public decimal AmountPaid { get; set; } = 0;
}

public class LiveStreamCollaborationDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string CollaboratorId { get; set; } = string.Empty;
    public string CollaboratorName { get; set; } = string.Empty;
    public string CollaboratorAvatar { get; set; } = string.Empty;
    public CollaborationRole Role { get; set; }
    public CollaborationStatus Status { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public decimal RevenueShare { get; set; }
}

public class InviteCollaboratorDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string CollaboratorId { get; set; } = string.Empty;
    
    [Required]
    public CollaborationRole Role { get; set; }
    
    public decimal RevenueShare { get; set; } = 0;
}

public class AcceptCollaborationDto
{
    [Required]
    public int CollaborationId { get; set; }
    
    public decimal? RevenueShare { get; set; }
}

public class LiveStreamChallengeDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChallengeType Type { get; set; }
    public ChallengeStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public decimal PrizeAmount { get; set; }
    public string? PrizeDescription { get; set; }
    public bool IsMonetized { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ChallengeParticipantDto> Participants { get; set; } = new List<ChallengeParticipantDto>();
}

public class CreateChallengeDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public ChallengeType Type { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    public int DurationMinutes { get; set; } = 60;
    
    public int MaxParticipants { get; set; } = 0;
    
    public decimal PrizeAmount { get; set; } = 0;
    
    public string? PrizeDescription { get; set; }
    
    public bool IsMonetized { get; set; } = false;
}


public class JoinChallengeDto
{
    [Required]
    public int ChallengeId { get; set; }
}

public class SubmitChallengeScoreDto
{
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Score { get; set; }
}
