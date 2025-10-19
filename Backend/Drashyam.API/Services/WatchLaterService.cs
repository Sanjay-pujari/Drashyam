using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class WatchLaterService : IWatchLaterService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WatchLaterService> _logger;

    public WatchLaterService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<WatchLaterService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<WatchLaterResponseDto>> GetUserWatchLaterAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.WatchLater
            .Include(w => w.Video)
                .ThenInclude(v => v.User)
            .Include(w => w.Video)
                .ThenInclude(v => v.Channel)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WatchLaterResponseDto
            {
                Id = w.Id,
                VideoId = w.VideoId,
                VideoTitle = w.Video.Title,
                VideoThumbnailUrl = w.Video.ThumbnailUrl,
                VideoDuration = w.Video.Duration.ToString(@"hh\:mm\:ss"),
                VideoViewCount = w.Video.ViewCount,
                ChannelName = w.Video.Channel != null ? w.Video.Channel.Name : 
                             (w.Video.User.FirstName + " " + w.Video.User.LastName),
                AddedAt = w.AddedAt
            })
            .ToListAsync();

        return new PagedResult<WatchLaterResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WatchLaterResponseDto> AddToWatchLaterAsync(string userId, WatchLaterCreateDto createDto)
    {
        // Check if video already exists in watch later
        var existing = await _context.WatchLater
            .FirstOrDefaultAsync(w => w.UserId == userId && w.VideoId == createDto.VideoId);

        if (existing != null)
        {
            throw new InvalidOperationException("Video is already in watch later");
        }

        // Check if video exists
        var video = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == createDto.VideoId);

        if (video == null)
        {
            throw new ArgumentException("Video not found");
        }

        var watchLater = new WatchLater
        {
            UserId = userId,
            VideoId = createDto.VideoId,
            AddedAt = DateTime.UtcNow
        };

        _context.WatchLater.Add(watchLater);
        await _context.SaveChangesAsync();

        return new WatchLaterResponseDto
        {
            Id = watchLater.Id,
            VideoId = watchLater.VideoId,
            VideoTitle = video.Title,
            VideoThumbnailUrl = video.ThumbnailUrl,
            VideoDuration = video.Duration.ToString(@"hh\:mm\:ss"),
            VideoViewCount = video.ViewCount,
            ChannelName = video.Channel != null ? video.Channel.Name : 
                         (video.User.FirstName + " " + video.User.LastName),
            AddedAt = watchLater.AddedAt
        };
    }

    public async Task<bool> RemoveFromWatchLaterAsync(string userId, int videoId)
    {
        var watchLater = await _context.WatchLater
            .FirstOrDefaultAsync(w => w.UserId == userId && w.VideoId == videoId);

        if (watchLater == null)
        {
            return false;
        }

        _context.WatchLater.Remove(watchLater);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsVideoInWatchLaterAsync(string userId, int videoId)
    {
        return await _context.WatchLater
            .AnyAsync(w => w.UserId == userId && w.VideoId == videoId);
    }

    public async Task ClearWatchLaterAsync(string userId)
    {
        var watchLaterItems = await _context.WatchLater
            .Where(w => w.UserId == userId)
            .ToListAsync();

        _context.WatchLater.RemoveRange(watchLaterItems);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetWatchLaterCountAsync(string userId)
    {
        return await _context.WatchLater
            .CountAsync(w => w.UserId == userId);
    }
}
