using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class HistoryService : IHistoryService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<HistoryService> _logger;

    public HistoryService(DrashyamDbContext context, IMapper mapper, ILogger<HistoryService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<HistoryDto>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        _logger.LogInformation("Getting history for user {UserId}, page {Page}, pageSize {PageSize}", userId, page, pageSize);

        var historyItems = await _context.VideoViews
            .Where(vv => vv.UserId == userId)
            .Include(vv => vv.Video)
                .ThenInclude(v => v.Channel)
            .OrderByDescending(vv => vv.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(vv => new HistoryDto
            {
                Id = vv.Id,
                VideoId = vv.VideoId,
                VideoTitle = vv.Video.Title,
                VideoThumbnailUrl = vv.Video.ThumbnailUrl,
                VideoUrl = vv.Video.VideoUrl,
                ChannelName = vv.Video.Channel.Name,
                ChannelId = vv.Video.Channel.Id.ToString(),
                ViewCount = vv.Video.ViewCount,
                Duration = vv.Video.Duration,
                ViewedAt = vv.ViewedAt,
                WatchDuration = vv.WatchDuration,
                UserAgent = vv.UserAgent,
                DeviceType = vv.DeviceType
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} history items for user {UserId}", historyItems.Count, userId);
        return historyItems;
    }

    public async Task<HistoryDto> AddToHistoryAsync(string userId, HistoryCreateDto historyDto)
    {
        _logger.LogInformation("Adding video {VideoId} to history for user {UserId}", historyDto.VideoId, userId);

        // Check if video exists
        var video = await _context.Videos
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == historyDto.VideoId);

        if (video == null)
        {
            throw new ArgumentException("Video not found");
        }

        // Check if already in history (get the most recent entry)
        var existingHistory = await _context.VideoViews
            .Where(vv => vv.UserId == userId && vv.VideoId == historyDto.VideoId)
            .OrderByDescending(vv => vv.ViewedAt)
            .FirstOrDefaultAsync();

        if (existingHistory != null)
        {
            // Update existing entry
            existingHistory.ViewedAt = DateTime.UtcNow;
            existingHistory.WatchDuration = TimeSpan.FromSeconds(historyDto.WatchDurationSeconds);
            existingHistory.UserAgent = historyDto.UserAgent;
            existingHistory.DeviceType = historyDto.DeviceType;

            await _context.SaveChangesAsync();

            return new HistoryDto
            {
                Id = existingHistory.Id,
                VideoId = existingHistory.VideoId,
                VideoTitle = video.Title,
                VideoThumbnailUrl = video.ThumbnailUrl,
                VideoUrl = video.VideoUrl,
                ChannelName = video.Channel.Name,
                ChannelId = video.Channel.Id.ToString(),
                ViewCount = video.ViewCount,
                Duration = video.Duration,
                ViewedAt = existingHistory.ViewedAt,
                WatchDuration = existingHistory.WatchDuration,
                UserAgent = existingHistory.UserAgent,
                DeviceType = existingHistory.DeviceType
            };
        }
        else
        {
            // Create new history entry
            var historyItem = new VideoView
            {
                UserId = userId,
                VideoId = historyDto.VideoId,
                ViewedAt = DateTime.UtcNow,
                WatchDuration = TimeSpan.FromSeconds(historyDto.WatchDurationSeconds),
                UserAgent = historyDto.UserAgent,
                DeviceType = historyDto.DeviceType
            };

            _context.VideoViews.Add(historyItem);
            await _context.SaveChangesAsync();

            return new HistoryDto
            {
                Id = historyItem.Id,
                VideoId = historyItem.VideoId,
                VideoTitle = video.Title,
                VideoThumbnailUrl = video.ThumbnailUrl,
                VideoUrl = video.VideoUrl,
                ChannelName = video.Channel.Name,
                ChannelId = video.Channel.Id.ToString(),
                ViewCount = video.ViewCount,
                Duration = video.Duration,
                ViewedAt = historyItem.ViewedAt,
                WatchDuration = historyItem.WatchDuration,
                UserAgent = historyItem.UserAgent,
                DeviceType = historyItem.DeviceType
            };
        }
    }

    public async Task<HistoryDto> UpdateHistoryAsync(int historyId, string userId, HistoryUpdateDto historyDto)
    {
        _logger.LogInformation("Updating history item {HistoryId} for user {UserId}", historyId, userId);

        var historyItem = await _context.VideoViews
            .Include(vv => vv.Video)
                .ThenInclude(v => v.Channel)
            .FirstOrDefaultAsync(vv => vv.Id == historyId && vv.UserId == userId);

        if (historyItem == null)
        {
            throw new ArgumentException("History item not found");
        }

        historyItem.WatchDuration = TimeSpan.FromSeconds(historyDto.WatchDurationSeconds);
        await _context.SaveChangesAsync();

        return new HistoryDto
        {
            Id = historyItem.Id,
            VideoId = historyItem.VideoId,
            VideoTitle = historyItem.Video.Title,
            VideoThumbnailUrl = historyItem.Video.ThumbnailUrl,
            VideoUrl = historyItem.Video.VideoUrl,
            ChannelName = historyItem.Video.Channel.Name,
            ChannelId = historyItem.Video.Channel.Id.ToString(),
            ViewCount = historyItem.Video.ViewCount,
            Duration = historyItem.Video.Duration,
            ViewedAt = historyItem.ViewedAt,
            WatchDuration = historyItem.WatchDuration,
            UserAgent = historyItem.UserAgent,
            DeviceType = historyItem.DeviceType
        };
    }

    public async Task<bool> RemoveFromHistoryAsync(int historyId, string userId)
    {
        _logger.LogInformation("Removing history item {HistoryId} for user {UserId}", historyId, userId);

        var historyItem = await _context.VideoViews
            .FirstOrDefaultAsync(vv => vv.Id == historyId && vv.UserId == userId);

        if (historyItem == null)
        {
            return false;
        }

        _context.VideoViews.Remove(historyItem);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed history item {HistoryId} for user {UserId}", historyId, userId);
        return true;
    }

    public async Task<bool> ClearUserHistoryAsync(string userId)
    {
        _logger.LogInformation("Clearing all history for user {UserId}", userId);

        var historyItems = await _context.VideoViews
            .Where(vv => vv.UserId == userId)
            .ToListAsync();

        if (historyItems.Any())
        {
            _context.VideoViews.RemoveRange(historyItems);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Cleared {Count} history items for user {UserId}", historyItems.Count, userId);
        return true;
    }

    public async Task<HistoryDto?> GetHistoryItemAsync(int historyId, string userId)
    {
        var historyItem = await _context.VideoViews
            .Include(vv => vv.Video)
                .ThenInclude(v => v.Channel)
            .FirstOrDefaultAsync(vv => vv.Id == historyId && vv.UserId == userId);

        if (historyItem == null)
        {
            return null;
        }

        return new HistoryDto
        {
            Id = historyItem.Id,
            VideoId = historyItem.VideoId,
            VideoTitle = historyItem.Video.Title,
            VideoThumbnailUrl = historyItem.Video.ThumbnailUrl,
            VideoUrl = historyItem.Video.VideoUrl,
            ChannelName = historyItem.Video.Channel.Name,
            ChannelId = historyItem.Video.Channel.Id.ToString(),
            ViewCount = historyItem.Video.ViewCount,
            Duration = historyItem.Video.Duration,
            ViewedAt = historyItem.ViewedAt,
            WatchDuration = historyItem.WatchDuration,
            UserAgent = historyItem.UserAgent,
            DeviceType = historyItem.DeviceType
        };
    }

    public async Task<bool> IsVideoInHistoryAsync(int videoId, string userId)
    {
        return await _context.VideoViews
            .AnyAsync(vv => vv.VideoId == videoId && vv.UserId == userId);
    }

    public async Task<int> GetUserHistoryCountAsync(string userId)
    {
        return await _context.VideoViews
            .CountAsync(vv => vv.UserId == userId);
    }
}

