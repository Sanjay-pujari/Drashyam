using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class CommunityChallenge
{
    public int Id { get; set; }
    
    [Required]
    public string CreatorId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Hashtag { get; set; } = string.Empty;
    
    public ChallengeType Type { get; set; }
    public ChallengeStatus Status { get; set; }
    public ChallengeVisibility Visibility { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? VotingEndDate { get; set; }
    
    public int? MaxParticipants { get; set; }
    public int? MinVideoLength { get; set; }
    public int? MaxVideoLength { get; set; }
    
    [MaxLength(500)]
    public string Rules { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Prizes { get; set; } = string.Empty;
    
    public decimal? PrizeAmount { get; set; }
    public string? PrizeCurrency { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    public string? BannerUrl { get; set; }
    
    public int ParticipantCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    
    public bool AllowVoting { get; set; } = true;
    public bool AllowComments { get; set; } = true;
    public bool RequireApproval { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser Creator { get; set; } = null!;
    public ICollection<ChallengeParticipant> Participants { get; set; } = new List<ChallengeParticipant>();
    public ICollection<ChallengeSubmission> Submissions { get; set; } = new List<ChallengeSubmission>();
    public ICollection<ChallengeVote> Votes { get; set; } = new List<ChallengeVote>();
    public ICollection<ChallengeComment> Comments { get; set; } = new List<ChallengeComment>();
}

