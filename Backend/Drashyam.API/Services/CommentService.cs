using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class CommentService : ICommentService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CommentService> _logger;

    public CommentService(DrashyamDbContext context, IMapper mapper, ILogger<CommentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
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

        return _mapper.Map<CommentDto>(comment);
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

        // Add like logic here
        return _mapper.Map<CommentDto>(comment);
    }

    public async Task<CommentDto> UnlikeCommentAsync(int commentId, string userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            throw new ArgumentException("Comment not found");

        // Remove like logic here
        return _mapper.Map<CommentDto>(comment);
    }

    public async Task<bool> IsCommentLikedAsync(int commentId, string userId)
    {
        // Check if comment is liked logic here
        return false;
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
