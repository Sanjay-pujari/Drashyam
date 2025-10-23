using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class EnhancedComment
{
    public int Id { get; set; }
    
    [Required]
    public int VideoId { get; set; }
    
    public int? ParentCommentId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public int LikeCount { get; set; } = 0;
    public int DislikeCount { get; set; } = 0;
    public int ReplyCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    
    public bool IsPinned { get; set; } = false;
    public bool IsHighlighted { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool IsEdited { get; set; } = false;
    
    public CommentVisibility Visibility { get; set; } = CommentVisibility.Public;
    public CommentStatus Status { get; set; } = CommentStatus.Approved;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    
    // Navigation properties
    public Video Video { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public EnhancedComment? ParentComment { get; set; }
    public ICollection<EnhancedComment> Replies { get; set; } = new List<EnhancedComment>();
    public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
    public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    public ICollection<CommentModeration> ModerationHistory { get; set; } = new List<CommentModeration>();
}

public class CommentReaction
{
    public int Id { get; set; }
    
    [Required]
    public int CommentId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public ReactionType Type { get; set; }
    
    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public EnhancedComment Comment { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public class CommentMention
{
    public int Id { get; set; }
    
    [Required]
    public int CommentId { get; set; }
    
    [Required]
    public string MentionedUserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string MentionedUserName { get; set; } = string.Empty;
    
    public DateTime MentionedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public EnhancedComment Comment { get; set; } = null!;
    public ApplicationUser MentionedUser { get; set; } = null!;
}

public class CommentModeration
{
    public int Id { get; set; }
    
    [Required]
    public int CommentId { get; set; }
    
    [Required]
    public string ModeratorId { get; set; } = string.Empty;
    
    [Required]
    public ModerationAction Action { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    public DateTime ModeratedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public EnhancedComment Comment { get; set; } = null!;
    public ApplicationUser Moderator { get; set; } = null!;
}

