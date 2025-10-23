using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class ChallengeDto
{
    public int Id { get; set; }
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public string CreatorAvatar { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
    public string Rules { get; set; } = string.Empty;
    public string Prizes { get; set; } = string.Empty;
    public decimal? PrizeAmount { get; set; }
    public string? PrizeCurrency { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? BannerUrl { get; set; }
    public int ParticipantCount { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int ShareCount { get; set; }
    public bool AllowVoting { get; set; }
    public bool AllowComments { get; set; }
    public bool RequireApproval { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsParticipant { get; set; }
    public bool HasSubmitted { get; set; }
    public List<ChallengeSubmissionDto> TopSubmissions { get; set; } = new();
    public List<ChallengeParticipantDto> RecentParticipants { get; set; } = new();
}

public class ChallengeCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Hashtag { get; set; } = string.Empty;
    
    [Required]
    public ChallengeType Type { get; set; }
    
    public ChallengeVisibility Visibility { get; set; } = ChallengeVisibility.Public;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
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
    public bool AllowVoting { get; set; } = true;
    public bool AllowComments { get; set; } = true;
    public bool RequireApproval { get; set; } = false;
}

public class ChallengeUpdateDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Hashtag { get; set; }
    
    public ChallengeVisibility? Visibility { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? VotingEndDate { get; set; }
    public int? MaxParticipants { get; set; }
    public int? MinVideoLength { get; set; }
    public int? MaxVideoLength { get; set; }
    
    [MaxLength(500)]
    public string? Rules { get; set; }
    
    [MaxLength(1000)]
    public string? Prizes { get; set; }
    
    public decimal? PrizeAmount { get; set; }
    public string? PrizeCurrency { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? BannerUrl { get; set; }
    public bool? AllowVoting { get; set; }
    public bool? AllowComments { get; set; }
    public bool? RequireApproval { get; set; }
}

public class ChallengeSubmissionDto
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public int? VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int Duration { get; set; }
    public long FileSize { get; set; }
    public string VideoQuality { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int VoteCount { get; set; }
    public bool IsWinner { get; set; }
    public int? WinnerPosition { get; set; }
    public decimal? PrizeAmount { get; set; }
    public bool HasVoted { get; set; }
    public List<ChallengeCommentDto> RecentComments { get; set; } = new();
}

public class ChallengeSubmissionCreateDto
{
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string VideoUrl { get; set; } = string.Empty;
    
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int Duration { get; set; }
    public long FileSize { get; set; }
    public string VideoQuality { get; set; } = string.Empty;
    public int? VideoId { get; set; }
}

public class ChallengeParticipantDto
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public ParticipationStatus Status { get; set; }
    public DateTime JoinedAt { get; set; }
    public int SubmissionCount { get; set; }
    public int VoteCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsWinner { get; set; }
    public int? WinnerPosition { get; set; }
    public decimal? PrizeAmount { get; set; }
}

public class ChallengeCommentDto
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public int? SubmissionId { get; set; }
    public int? ParentCommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int ReplyCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasLiked { get; set; }
    public List<ChallengeCommentDto> Replies { get; set; } = new();
}

public class ChallengeCommentCreateDto
{
    [Required]
    public int ChallengeId { get; set; }
    
    public int? SubmissionId { get; set; }
    public int? ParentCommentId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}

public class ChallengeVoteDto
{
    public int Id { get; set; }
    public int ChallengeId { get; set; }
    public int SubmissionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public VoteType Type { get; set; }
    public DateTime VotedAt { get; set; }
    public string? Comment { get; set; }
}

public class ChallengeVoteCreateDto
{
    [Required]
    public int ChallengeId { get; set; }
    
    [Required]
    public int SubmissionId { get; set; }
    
    [Required]
    public VoteType Type { get; set; }
    
    [MaxLength(200)]
    public string? Comment { get; set; }
}

public class ChallengeFilterDto
{
    public ChallengeType? Type { get; set; }
    public ChallengeStatus? Status { get; set; }
    public ChallengeVisibility? Visibility { get; set; }
    public string? SearchTerm { get; set; }
    public string? Hashtag { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsParticipant { get; set; }
    public bool? HasPrize { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ChallengeAnalyticsDto
{
    public int TotalChallenges { get; set; }
    public int ActiveChallenges { get; set; }
    public int CompletedChallenges { get; set; }
    public int TotalParticipants { get; set; }
    public int TotalSubmissions { get; set; }
    public int TotalVotes { get; set; }
    public decimal TotalPrizeAmount { get; set; }
    public List<ChallengeTypeStats> TypeStats { get; set; } = new();
    public List<MonthlyChallengeStats> MonthlyStats { get; set; } = new();
    public List<TopChallengeDto> TopChallenges { get; set; } = new();
}

public class ChallengeTypeStats
{
    public ChallengeType Type { get; set; }
    public int Count { get; set; }
    public int Participants { get; set; }
    public int Submissions { get; set; }
}

public class MonthlyChallengeStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Challenges { get; set; }
    public int Participants { get; set; }
    public int Submissions { get; set; }
}

public class TopChallengeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public int SubmissionCount { get; set; }
    public int ViewCount { get; set; }
    public decimal? PrizeAmount { get; set; }
}
