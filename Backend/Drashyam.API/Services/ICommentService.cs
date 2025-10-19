using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ICommentService
{
    Task<CommentDto> AddCommentAsync(CommentCreateDto createDto, string userId);
    Task<CommentDto> UpdateCommentAsync(int commentId, CommentUpdateDto updateDto, string userId);
    Task<bool> DeleteCommentAsync(int commentId, string userId);
    Task<PagedResult<CommentDto>> GetVideoCommentsAsync(int videoId, int page = 1, int pageSize = 20);
    Task<PagedResult<CommentDto>> GetCommentRepliesAsync(int parentCommentId, int page = 1, int pageSize = 20);
    Task<CommentDto> LikeCommentAsync(int commentId, string userId);
    Task<CommentDto> UnlikeCommentAsync(int commentId, string userId);
    Task<CommentDto> DislikeCommentAsync(int commentId, string userId);
    Task<bool> IsCommentLikedAsync(int commentId, string userId);
    Task<bool> IsCommentDislikedAsync(int commentId, string userId);
    Task<PagedResult<CommentDto>> GetUserCommentsAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> ReportCommentAsync(int commentId, string userId, string reason);
}
