using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class LiveStreamChat
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    
    public string? Emoji { get; set; }
    
    public bool IsModerator { get; set; } = false;
    
    public bool IsHighlighted { get; set; } = false;
    
    public bool IsPinned { get; set; } = false;
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class LiveStreamReaction
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public ReactionType Type { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class LiveStreamPoll
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Question { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool AllowMultipleChoices { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndedAt { get; set; }
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
    public ICollection<LiveStreamPollOption> Options { get; set; } = new List<LiveStreamPollOption>();
    public ICollection<LiveStreamPollVote> Votes { get; set; } = new List<LiveStreamPollVote>();
}

public class LiveStreamPollOption
{
    public int Id { get; set; }
    
    [Required]
    public int PollId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Text { get; set; } = string.Empty;
    
    public int VoteCount { get; set; } = 0;
    
    public int Order { get; set; } = 0;
    
    // Navigation properties
    public LiveStreamPoll Poll { get; set; } = null!;
}

public class LiveStreamPollVote
{
    public int Id { get; set; }
    
    [Required]
    public int PollId { get; set; }
    
    [Required]
    public int OptionId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStreamPoll Poll { get; set; } = null!;
    public LiveStreamPollOption Option { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class LiveStreamAnalytics
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    public long ViewerCount { get; set; } = 0;
    
    public long PeakViewerCount { get; set; } = 0;
    
    public long TotalViewTime { get; set; } = 0; // in seconds
    
    public long ChatMessageCount { get; set; } = 0;
    
    public long ReactionCount { get; set; } = 0;
    
    public long ShareCount { get; set; } = 0;
    
    public decimal Revenue { get; set; } = 0;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
}

public class LiveStreamQuality
{
    public int Id { get; set; }
    
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Quality { get; set; } = string.Empty; // 720p, 1080p, 4K
    
    public int Bitrate { get; set; } = 0; // kbps
    
    public int FrameRate { get; set; } = 0; // fps
    
    public string Resolution { get; set; } = string.Empty; // 1920x1080
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public LiveStream LiveStream { get; set; } = null!;
}

