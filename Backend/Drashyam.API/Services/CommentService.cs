using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class CommentService : ICommentService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CommentService> _logger;
    private readonly IHubContext<VideoHub> _videoHub;

    public CommentService(DrashyamDbContext context, IMapper mapper, ILogger<CommentService> logger, IHubContext<VideoHub> videoHub)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _videoHub = videoHub;
    }

    public async Task<CommentDto> AddCommentAsync(CommentCreateDto createDto, string userId)
    {
        var comment = _mapper.Map<Comment>(createDto);
        comment.UserId = userId;
        comment.CreatedAt = DateTime.UtcNow;

        _context.Comments.Add(comment);
        
        // Update video comment count if this is a top-level comment
        if (comment.ParentCommentId == null)
        {
            var video = await _context.Videos.FindAsync(comment.VideoId);
            if (video != null)
            {
                video.CommentCount++;
            }
        }
        else
        {
            // Update parent comment reply count
            var parentComment = await _context.Comments.FindAsync(comment.ParentCommentId);
            if (parentComment != null)
            {
                parentComment.ReplyCount++;
                
                // Also update video comment count for replies
                var video = await _context.Videos.FindAsync(comment.VideoId);
                if (video != null)
                {
                    video.CommentCount++;
                }
            }
        }
        
        await _context.SaveChangesAsync();

        // Send real-time update via SignalR
        var commentDto = _mapper.Map<CommentDto>(comment);
        await _videoHub.Clients.Group($"Video_{comment.VideoId}").SendAsync("ReceiveComment", comment.VideoId, commentDto);

        return commentDto;
    }

    public async Task<CommentDto> UpdateCommentAsync(int commentId, CommentUpdateDto updateDto, string userId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment == null)
            throw new ArgumentException("Comment not found or access denied");

        _mapper.Map(updateDto, comment);
        comment.UpdatedAt = DateTime.UtcNow;
        comment.IsEdited = true;

        await _context.SaveChangesAsync();
        return _mapper.Map<CommentDto>(comment);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment == null)
            return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<CommentDto>> GetVideoCommentsAsync(int videoId, int page = 1, int pageSize = 20)
    {
        var comments = await _context.Comments
            .Where(c => c.VideoId == videoId && c.ParentCommentId == null)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Comments
            .Where(c => c.VideoId == videoId && c.ParentCommentId == null)
            .CountAsync();

        return new PagedResult<CommentDto>
        {
            Items = _mapper.Map<List<CommentDto>>(comments),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<CommentDto>> GetCommentRepliesAsync(int parentCommentId, int page = 1, int pageSize = 20)
    {
        var comments = await _context.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .CountAsync();

        return new PagedResult<CommentDto>
        {
            Items = _mapper.Map<List<CommentDto>>(comments),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CommentDto> LikeCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new ArgumentException("Comment not found");

        // Check if user already liked/disliked this comment
        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            if (existingLike.Type == DTOs.LikeType.Like)
            {
                // User already liked, remove the like
                _context.CommentLikes.Remove(existingLike);
                comment.LikeCount--;
            }
            else
            {
                // User disliked, change to like
                existingLike.Type = DTOs.LikeType.Like;
                comment.DislikeCount--;
                comment.LikeCount++;
            }
        }
        else
        {
            // Add new like
            _context.CommentLikes.Add(new CommentLike
            {
                UserId = userId,
                CommentId = commentId,
                Type = DTOs.LikeType.Like
            });
            comment.LikeCount++;
        }

        await _context.SaveChangesAsync();

        // Send real-time update via SignalR
        await _videoHub.Clients.Group($"Video_{comment.VideoId}").SendAsync("ReceiveCommentLike", commentId, comment.LikeCount, comment.DislikeCount);

        // Return updated comment with like status
        var updatedComment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        
        // Check current like status
        var currentLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
        
        commentDto.IsLiked = currentLike?.Type == DTOs.LikeType.Like;
        commentDto.IsDisliked = currentLike?.Type == DTOs.LikeType.Dislike;

        return commentDto;
    }

    public async Task<CommentDto> UnlikeCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new ArgumentException("Comment not found");

        // Find existing like
        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            if (existingLike.Type == DTOs.LikeType.Like)
            {
                comment.LikeCount--;
            }
            else
            {
                comment.DislikeCount--;
            }
            
            _context.CommentLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
        }

        // Return updated comment
        var updatedComment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        commentDto.IsLiked = false;
        commentDto.IsDisliked = false;

        return commentDto;
    }

    public async Task<CommentDto> DislikeCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new ArgumentException("Comment not found");

        // Check if user already liked/disliked this comment
        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            if (existingLike.Type == DTOs.LikeType.Dislike)
            {
                // User already disliked, remove the dislike
                _context.CommentLikes.Remove(existingLike);
                comment.DislikeCount--;
            }
            else
            {
                // User liked, change to dislike
                existingLike.Type = DTOs.LikeType.Dislike;
                comment.LikeCount--;
                comment.DislikeCount++;
            }
        }
        else
        {
            // Add new dislike
            _context.CommentLikes.Add(new CommentLike
            {
                UserId = userId,
                CommentId = commentId,
                Type = DTOs.LikeType.Dislike
            });
            comment.DislikeCount++;
        }

        await _context.SaveChangesAsync();

        // Send real-time update via SignalR
        await _videoHub.Clients.Group($"Video_{comment.VideoId}").SendAsync("ReceiveCommentDislike", commentId, comment.LikeCount, comment.DislikeCount);

        // Return updated comment with dislike status
        var updatedComment = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        var commentDto = _mapper.Map<CommentDto>(updatedComment);
        
        // Check current like status
        var currentLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);
        
        commentDto.IsLiked = currentLike?.Type == DTOs.LikeType.Like;
        commentDto.IsDisliked = currentLike?.Type == DTOs.LikeType.Dislike;

        return commentDto;
    }

    public async Task<bool> IsCommentLikedAsync(int commentId, string userId)
    {
        return await _context.CommentLikes
            .AnyAsync(l => l.CommentId == commentId && l.UserId == userId && l.Type == DTOs.LikeType.Like);
    }

    public async Task<bool> IsCommentDislikedAsync(int commentId, string userId)
    {
        return await _context.CommentLikes
            .AnyAsync(l => l.CommentId == commentId && l.UserId == userId && l.Type == DTOs.LikeType.Dislike);
    }

    public async Task<PagedResult<CommentDto>> GetUserCommentsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var comments = await _context.Comments
            .Where(c => c.UserId == userId)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Comments
            .Where(c => c.UserId == userId)
            .CountAsync();

        return new PagedResult<CommentDto>
        {
            Items = _mapper.Map<List<CommentDto>>(comments),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ReportCommentAsync(int commentId, string userId, string reason)
    {
        // Add reporting logic here
        return true;
    }
}
