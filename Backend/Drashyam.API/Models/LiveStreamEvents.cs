using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class LiveStreamEvent
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public string CreatorId { get; set; } = string.Empty;
    
    public int? ChannelId { get; set; }
    
    public EventType Type { get; set; }
    
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    
    public DateTime ScheduledStartTime { get; set; }
    
    public DateTime? ActualStartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public int DurationMinutes { get; set; } = 0;
    
    public bool IsPublic { get; set; } = true;
    
    public bool IsMonetized { get; set; } = false;
    
    public decimal TicketPrice { get; set; } = 0;
    
    public int MaxAttendees { get; set; } = 0;
    
    public int CurrentAttendees { get; set; } = 0;
    
    public string? ThumbnailUrl { get; set; }
    
    public string? Category { get; set; }
    
    public string? Tags { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser Creator { get; set; } = null!;
    public Channel? Channel { get; set; }
    public ICollection<LiveStream> LiveStreams { get; set; } = new List<LiveStream>();
    public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
}

public class EventAttendee
{
    public int Id { get; set; }
    
    [Required]
    public int EventId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Registered;
    
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CheckedInAt { get; set; }
    
    public DateTime? CheckedOutAt { get; set; }
    
    public bool IsPaid { get; set; } = false;
    
    public decimal AmountPaid { get; set; } = 0;
    
    // Navigation properties
    public LiveStreamEvent Event { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class LiveStreamCollaboration
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string CollaboratorId { get; set; } = string.Empty;
    
    public CollaborationRole Role { get; set; }
    
    public CollaborationStatus Status { get; set; } = CollaborationStatus.Pending;
    
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? AcceptedAt { get; set; }
    
    public DateTime? JoinedAt { get; set; }
    
    public DateTime? LeftAt { get; set; }
    
    public decimal RevenueShare { get; set; } = 0;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser Collaborator { get; set; } = null!;
}

public class LiveStreamChallenge
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public ChallengeType Type { get; set; }
    
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Active;
    
    public DateTime StartTime { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public int DurationMinutes { get; set; } = 0;
    
    public int MaxParticipants { get; set; } = 0;
    
    public int CurrentParticipants { get; set; } = 0;
    
    public decimal PrizeAmount { get; set; } = 0;
    
    public string? PrizeDescription { get; set; }
    
    public bool IsMonetized { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
}


