using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class EnhancedCommentDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public int? ParentCommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
    public int ReplyCount { get; set; }
    public int ShareCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsHighlighted { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEdited { get; set; }
    public CommentVisibility Visibility { get; set; }
    public CommentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool HasLiked { get; set; }
    public bool HasDisliked { get; set; }
    public List<CommentReactionDto> Reactions { get; set; } = new();
    public List<CommentMentionDto> Mentions { get; set; } = new();
    public List<EnhancedCommentDto> Replies { get; set; } = new();
}

public class CommentReactionDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ReactionType Type { get; set; }
    public DateTime ReactedAt { get; set; }
}

public class CommentMentionDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string MentionedUserId { get; set; } = string.Empty;
    public string MentionedUserName { get; set; } = string.Empty;
    public DateTime MentionedAt { get; set; }
}

public class EnhancedCommentCreateDto
{
    [Required]
    public int VideoId { get; set; }
    
    public int? ParentCommentId { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public CommentVisibility Visibility { get; set; } = CommentVisibility.Public;
}

public class EnhancedCommentUpdateDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class CommentReactionCreateDto
{
    [Required]
    public int CommentId { get; set; }
    
    [Required]
    public ReactionType Type { get; set; }
}

public class CommentModerationDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string ModeratorId { get; set; } = string.Empty;
    public string ModeratorName { get; set; } = string.Empty;
    public ModerationAction Action { get; set; }
    public string? Reason { get; set; }
    public DateTime ModeratedAt { get; set; }
}

public class CommentFilterDto
{
    public int? VideoId { get; set; }
    public int? ParentCommentId { get; set; }
    public string? UserId { get; set; }
    public CommentStatus? Status { get; set; }
    public CommentVisibility? Visibility { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsHighlighted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
