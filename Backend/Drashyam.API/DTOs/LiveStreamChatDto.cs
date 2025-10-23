using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class LiveStreamChatDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ChatMessageType Type { get; set; }
    public string? Emoji { get; set; }
    public bool IsModerator { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsPinned { get; set; }
    public DateTime Timestamp { get; set; }
}

public class SendChatMessageDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public ChatMessageType Type { get; set; } = ChatMessageType.Text;
    
    public string? Emoji { get; set; }
}

public class LiveStreamReactionDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ReactionType Type { get; set; }
    public DateTime Timestamp { get; set; }
}

public class SendReactionDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    public ReactionType Type { get; set; }
}

public class LiveStreamPollDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool AllowMultipleChoices { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<LiveStreamPollOptionDto> Options { get; set; } = new List<LiveStreamPollOptionDto>();
    public int TotalVotes { get; set; }
}

public class LiveStreamPollOptionDto
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public int Order { get; set; }
    public decimal Percentage { get; set; }
}

public class CreatePollDto
{
    [Required]
    public int LiveStreamId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Question { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public bool AllowMultipleChoices { get; set; } = false;
    
    [Required]
    [MinLength(2)]
    public List<string> Options { get; set; } = new List<string>();
}

public class VotePollDto
{
    [Required]
    public int PollId { get; set; }
    
    [Required]
    public List<int> OptionIds { get; set; } = new List<int>();
}

public class LiveStreamAnalyticsDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public long ViewerCount { get; set; }
    public long PeakViewerCount { get; set; }
    public long TotalViewTime { get; set; }
    public long ChatMessageCount { get; set; }
    public long ReactionCount { get; set; }
    public long ShareCount { get; set; }
    public decimal Revenue { get; set; }
    public DateTime Timestamp { get; set; }
}

public class LiveStreamQualityDto
{
    public int Id { get; set; }
    public int LiveStreamId { get; set; }
    public string Quality { get; set; } = string.Empty;
    public int Bitrate { get; set; }
    public int FrameRate { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
