using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class ChallengeComment
{
    public int Id { get; set; }
    
    [Required]
    public int ChallengeId { get; set; }
    
    public int? SubmissionId { get; set; }
    public int? ParentCommentId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    public int LikeCount { get; set; } = 0;
    public int ReplyCount { get; set; } = 0;
    
    public bool IsPinned { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public CommunityChallenge Challenge { get; set; } = null!;
    public ChallengeSubmission? Submission { get; set; }
    public ChallengeComment? ParentComment { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ChallengeComment> Replies { get; set; } = new List<ChallengeComment>();
    public ICollection<ChallengeCommentLike> Likes { get; set; } = new List<ChallengeCommentLike>();
}

public class ChallengeCommentLike
{
    public int Id { get; set; }
    
    [Required]
    public int CommentId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ChallengeComment Comment { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
