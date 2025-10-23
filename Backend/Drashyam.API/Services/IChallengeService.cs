using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IChallengeService
{
    // Challenge Management
    Task<ChallengeDto> CreateChallengeAsync(ChallengeCreateDto createDto, string userId);
    Task<ChallengeDto> GetChallengeByIdAsync(int id, string? userId = null);
    Task<PagedResult<ChallengeDto>> GetChallengesAsync(ChallengeFilterDto filter, string? userId = null);
    Task<PagedResult<ChallengeDto>> GetUserChallengesAsync(string userId, ChallengeFilterDto filter);
    Task<ChallengeDto> UpdateChallengeAsync(int id, ChallengeUpdateDto updateDto, string userId);
    Task<bool> DeleteChallengeAsync(int id, string userId);
    Task<bool> PublishChallengeAsync(int id, string userId);
    Task<bool> CancelChallengeAsync(int id, string userId);
    
    // Challenge Participation
    Task<bool> JoinChallengeAsync(int challengeId, string userId);
    Task<bool> LeaveChallengeAsync(int challengeId, string userId);
    Task<PagedResult<ChallengeParticipantDto>> GetChallengeParticipantsAsync(int challengeId, int page = 1, int pageSize = 20);
    
    // Challenge Submissions
    Task<ChallengeSubmissionDto> SubmitToChallengeAsync(ChallengeSubmissionCreateDto createDto, string userId);
    Task<PagedResult<ChallengeSubmissionDto>> GetChallengeSubmissionsAsync(int challengeId, int page = 1, int pageSize = 20, string? userId = null);
    Task<ChallengeSubmissionDto> GetSubmissionByIdAsync(int id, string? userId = null);
    Task<bool> ApproveSubmissionAsync(int submissionId, string userId);
    Task<bool> RejectSubmissionAsync(int submissionId, string userId, string reason);
    Task<bool> DeleteSubmissionAsync(int submissionId, string userId);
    
    // Challenge Voting
    Task<bool> VoteOnSubmissionAsync(ChallengeVoteCreateDto createDto, string userId);
    Task<bool> RemoveVoteAsync(int challengeId, int submissionId, string userId);
    Task<List<ChallengeSubmissionDto>> GetTopSubmissionsAsync(int challengeId, int count = 10);
    
    // Challenge Comments
    Task<ChallengeCommentDto> AddCommentAsync(ChallengeCommentCreateDto createDto, string userId);
    Task<PagedResult<ChallengeCommentDto>> GetCommentsAsync(int challengeId, int? submissionId = null, int page = 1, int pageSize = 20);
    Task<bool> LikeCommentAsync(int commentId, string userId);
    Task<bool> UnlikeCommentAsync(int commentId, string userId);
    Task<bool> DeleteCommentAsync(int commentId, string userId);
    Task<bool> PinCommentAsync(int commentId, string userId);
    
    // Challenge Analytics
    Task<ChallengeAnalyticsDto> GetChallengeAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ChallengeAnalyticsDto> GetChallengeAnalyticsByIdAsync(int challengeId, string userId);
    
    // Challenge Discovery
    Task<List<ChallengeDto>> GetTrendingChallengesAsync(int count = 10);
    Task<List<ChallengeDto>> GetRecommendedChallengesAsync(string userId, int count = 10);
    Task<List<ChallengeDto>> GetChallengesByHashtagAsync(string hashtag, int page = 1, int pageSize = 20);
}
