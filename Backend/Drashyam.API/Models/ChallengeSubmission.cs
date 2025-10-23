using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class ChallengeSubmission
{
    public int Id { get; set; }
    
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public int? VideoId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public string VideoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    public int Duration { get; set; } // in seconds
    public long FileSize { get; set; }
    public string VideoQuality { get; set; } = string.Empty;
    
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
    
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    public int VoteCount { get; set; } = 0;
    
    public bool IsWinner { get; set; } = false;
    public int? WinnerPosition { get; set; }
    public decimal? PrizeAmount { get; set; }
    
    // Navigation properties
    public CommunityChallenge Challenge { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public Video? Video { get; set; }
    public ICollection<ChallengeVote> Votes { get; set; } = new List<ChallengeVote>();
    public ICollection<ChallengeComment> Comments { get; set; } = new List<ChallengeComment>();
}

