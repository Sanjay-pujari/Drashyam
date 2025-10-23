using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class ChallengeVote
{
    public int Id { get; set; }
    
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    public int SubmissionId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public VoteType Type { get; set; }
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(200)]
    public string? Comment { get; set; }
    
    // Navigation properties
    public CommunityChallenge Challenge { get; set; } = null!;
    public ChallengeSubmission Submission { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

