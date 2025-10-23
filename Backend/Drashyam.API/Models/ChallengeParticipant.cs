using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class ChallengeParticipant
{
    public int Id { get; set; }
    
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public ParticipationStatus Status { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    
    public int SubmissionCount { get; set; } = 0;
    public int VoteCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    
    public bool IsWinner { get; set; } = false;
    public int? WinnerPosition { get; set; }
    public decimal? PrizeAmount { get; set; }
    
    // Navigation properties
    public CommunityChallenge Challenge { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

