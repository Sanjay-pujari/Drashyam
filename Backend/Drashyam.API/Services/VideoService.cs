using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Drashyam.API.Services;

public class VideoService : IVideoService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<VideoService> _logger;

    public VideoService(
        DrashyamDbContext context,
        IMapper mapper,
        IFileStorageService fileStorage,
        ILogger<VideoService> logger)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<VideoDto> UploadVideoAsync(VideoUploadDto uploadDto, string userId)
    {
        try
        {
            // Check user's subscription limits
            var user = await _context.Users
                .Include(u => u.Channels)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            // Validate channel if specified
            if (uploadDto.ChannelId.HasValue)
            {
                var channel = await _context.Channels
                    .FirstOrDefaultAsync(c => c.Id == uploadDto.ChannelId && c.UserId == userId);

                if (channel == null)
                    throw new ArgumentException("Channel not found or access denied");

                // Check video limit for channel
                var currentVideoCount = await _context.Videos
                    .CountAsync(v => v.ChannelId == uploadDto.ChannelId);

                if (currentVideoCount >= channel.MaxVideos)
                    throw new InvalidOperationException("Channel video limit exceeded");
            }

            // Upload video file
            var videoUrl = await _fileStorage.UploadVideoAsync(uploadDto.VideoFile);
            
            // Upload thumbnail if provided
            string? thumbnailUrl = null;
            if (uploadDto.ThumbnailFile != null)
            {
                thumbnailUrl = await _fileStorage.UploadThumbnailAsync(uploadDto.ThumbnailFile);
            }

            // Generate share token
            var shareToken = GenerateShareToken();

            var video = new Video
            {
                Title = uploadDto.Title,
                Description = uploadDto.Description,
                VideoUrl = videoUrl,
                ThumbnailUrl = thumbnailUrl,
                UserId = userId,
                ChannelId = uploadDto.ChannelId,
                Visibility = (Models.VideoVisibility)uploadDto.Visibility,
                Tags = uploadDto.Tags,
                Category = uploadDto.Category,
                ShareToken = shareToken,
                FileSize = uploadDto.VideoFile.Length,
                Status = VideoProcessingStatus.Ready, // Set to Ready immediately since we're not doing processing
                PublishedAt = DateTime.UtcNow
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            // Update channel video count
            if (uploadDto.ChannelId.HasValue)
            {
                var channel = await _context.Channels.FindAsync(uploadDto.ChannelId);
                if (channel != null)
                {
                    channel.CurrentVideoCount++;
                    channel.VideoCount++;
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("Video uploaded successfully: {VideoId}", video.Id);

            return await GetVideoByIdAsync(video.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video for user {UserId}", userId);
            throw;
        }
    }

    public async Task<VideoDto> GetVideoByIdAsync(int id)
    {
        var video = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (video == null)
            throw new ArgumentException("Video not found");

        return _mapper.Map<VideoDto>(video);
    }

    public async Task<VideoDto> GetVideoByShareTokenAsync(string shareToken)
    {
        var video = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.ShareToken == shareToken);

        if (video == null)
            throw new ArgumentException("Video not found");

        return _mapper.Map<VideoDto>(video);
    }

    public async Task<PagedResult<VideoDto>> GetVideosAsync(VideoFilterDto filter)
    {
        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Search))
        {
            query = query.Where(v => v.Title.Contains(filter.Search) || 
                                   v.Description.Contains(filter.Search) ||
                                   v.Tags.Contains(filter.Search));
        }

        if (!string.IsNullOrEmpty(filter.Category))
        {
            query = query.Where(v => v.Category == filter.Category);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(v => v.Type == (Models.VideoType) filter.Type);
        }

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "views" => filter.SortOrder == "asc" ? query.OrderBy(v => v.ViewCount) : query.OrderByDescending(v => v.ViewCount),
            "likes" => filter.SortOrder == "asc" ? query.OrderBy(v => v.LikeCount) : query.OrderByDescending(v => v.LikeCount),
            "title" => filter.SortOrder == "asc" ? query.OrderBy(v => v.Title) : query.OrderByDescending(v => v.Title),
            _ => filter.SortOrder == "asc" ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<VideoDto>> GetUserVideosAsync(string userId, VideoFilterDto filter)
    {
        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.UserId == userId && v.Status != VideoProcessingStatus.Deleted);

        // Apply filters and sorting similar to GetVideosAsync
        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<VideoDto>> GetChannelVideosAsync(int channelId, VideoFilterDto filter)
    {
        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.ChannelId == channelId && v.Status == VideoProcessingStatus.Ready);

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<VideoDto> UpdateVideoAsync(int id, VideoUpdateDto updateDto, string userId)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

        if (video == null)
            throw new ArgumentException("Video not found or access denied");

        // Update properties
        if (!string.IsNullOrEmpty(updateDto.Title))
            video.Title = updateDto.Title;

        if (updateDto.Description != null)
            video.Description = updateDto.Description;

        if (updateDto.Visibility.HasValue)
            video.Visibility = (Models.VideoVisibility)updateDto.Visibility.Value;

        if (updateDto.Tags != null)
            video.Tags = updateDto.Tags;

        if (updateDto.Category != null)
            video.Category = updateDto.Category;

        // Update thumbnail if provided
        if (updateDto.ThumbnailFile != null)
        {
            video.ThumbnailUrl = await _fileStorage.UploadThumbnailAsync(updateDto.ThumbnailFile);
        }

        await _context.SaveChangesAsync();

        return await GetVideoByIdAsync(id);
    }

    public async Task DeleteVideoAsync(int id, string userId)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);

        if (video == null)
            throw new ArgumentException("Video not found or access denied");

        video.Status = VideoProcessingStatus.Deleted;
        await _context.SaveChangesAsync();

        // TODO: Delete files from storage
    }

    public async Task<VideoDto> LikeVideoAsync(int videoId, string userId, Drashyam.API.DTOs.LikeType type)
    {
        var video = await _context.Videos.FindAsync(videoId);
        if (video == null)
            throw new ArgumentException("Video not found");

        // Check if user already liked/disliked
        var existingLike = await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == videoId && l.UserId == userId);

        if (existingLike != null)
        {
            if (existingLike.Type != (Models.LikeType) type)
            {
                // Remove like/dislike
                _context.VideoLikes.Remove(existingLike);
                if (type == DTOs.LikeType.Like)
                    video.LikeCount--;
                else
                    video.DislikeCount--;
            }
            else
            {
                // Change like/dislike
                if (existingLike.Type == Models.LikeType.Like)
                {
                    video.LikeCount--;
                    video.DislikeCount++;
                }
                else
                {
                    video.DislikeCount--;
                    video.LikeCount++;
                }
                existingLike.Type = (Models.LikeType)type;
            }
        }
        else
        {
            // Add new like/dislike
            _context.VideoLikes.Add(new VideoLike
            {
                UserId = userId,
                VideoId = videoId,
                Type = (Models.LikeType)type
            });

            if (type == DTOs.LikeType.Like)
                video.LikeCount++;
            else
                video.DislikeCount++;
        }

        await _context.SaveChangesAsync();
        return await GetVideoByIdAsync(videoId);
    }

    public async Task<VideoDto> UnlikeVideoAsync(int videoId, string userId)
    {
        var existingLike = await _context.VideoLikes
            .FirstOrDefaultAsync(l => l.VideoId == videoId && l.UserId == userId);

        if (existingLike != null)
        {
            var video = await _context.Videos.FindAsync(videoId);
            if (video != null)
            {
                if (existingLike.Type == Models.LikeType.Like)
                    video.LikeCount--;
                else
                    video.DislikeCount--;

                _context.VideoLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
            }
        }

        return await GetVideoByIdAsync(videoId);
    }

    public async Task<VideoDto> RecordVideoViewAsync(int videoId, string userId, TimeSpan watchDuration, string? userAgent = null, string? ipAddress = null)
    {
        var video = await _context.Videos.FindAsync(videoId);
        if (video == null) 
        {
            _logger.LogWarning("Video {VideoId} not found when recording view", videoId);
            throw new ArgumentException("Video not found");
        }

        _logger.LogInformation("Recording view for video {VideoId} by user {UserId}, duration: {Duration}", videoId, userId, watchDuration);

        // Determine device type from user agent
        var deviceType = DetermineDeviceType(userAgent);

        // Record view
        _context.VideoViews.Add(new VideoView
        {
            UserId = userId,
            VideoId = videoId,
            WatchDuration = watchDuration,
            ViewedAt = DateTime.UtcNow,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            DeviceType = deviceType
        });

        var oldViewCount = video.ViewCount;
        video.ViewCount++;
        await _context.SaveChangesAsync();

        _logger.LogInformation("View count updated for video {VideoId}: {OldCount} -> {NewCount}", videoId, oldViewCount, video.ViewCount);

        // Return updated video
        return await GetVideoByIdAsync(videoId);
    }

    public async Task<string> GenerateShareLinkAsync(int videoId, string userId)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId && v.UserId == userId);

        if (video == null)
            throw new ArgumentException("Video not found or access denied");

        if (string.IsNullOrEmpty(video.ShareToken))
        {
            video.ShareToken = GenerateShareToken();
            await _context.SaveChangesAsync();
        }

        return video.ShareToken;
    }

    public async Task<PagedResult<VideoDto>> SearchVideosAsync(string query, VideoFilterDto filter)
    {
        filter.Search = query;
        return await GetVideosAsync(filter);
    }

    public async Task<PagedResult<VideoDto>> GetTrendingVideosAsync(VideoFilterDto filter)
    {
        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
            .OrderByDescending(v => v.ViewCount)
            .ThenByDescending(v => v.LikeCount);

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<VideoDto>> GetRecommendedVideosAsync(string userId, VideoFilterDto filter)
    {
        // Simple recommendation based on user's liked videos and subscribed channels
        var userLikes = await _context.VideoLikes
            .Where(l => l.UserId == userId && l.Type == Models.LikeType.Like)
            .Select(l => l.VideoId)
            .ToListAsync();

        var userSubscriptions = await _context.ChannelSubscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.ChannelId)
            .ToListAsync();

        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
            .Where(v => userSubscriptions.Contains(v.ChannelId ?? 0) || 
                       v.Category != null && _context.Videos
                           .Where(v2 => userLikes.Contains(v2.Id))
                           .Select(v2 => v2.Category)
                           .Contains(v.Category))
            .OrderByDescending(v => v.ViewCount);

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<VideoDto>> GetUserFavoriteVideosAsync(string userId, VideoFilterDto filter)
    {
        var favoriteVideoIds = await _context.VideoLikes
            .Where(l => l.UserId == userId && l.Type == Models.LikeType.Like)
            .Select(l => l.VideoId)
            .ToListAsync();

        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => favoriteVideoIds.Contains(v.Id) && v.Status == VideoProcessingStatus.Ready);

        var totalCount = await query.CountAsync();
        var videos = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<PagedResult<VideoDto>> GetSubscribedChannelsVideosAsync(string userId, VideoFilterDto filter)
    {
        var subscribedChannelIds = await _context.ChannelSubscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.ChannelId)
            .ToListAsync();

        var query = _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.ChannelId != null && subscribedChannelIds.Contains(v.ChannelId.Value))
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
            .OrderByDescending(v => v.CreatedAt);

        var totalCount = await query.CountAsync();
        var videos = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<VideoDto>
        {
            Items = _mapper.Map<List<VideoDto>>(videos),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    private string GenerateShareToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public async Task<VideoDto> UpdateVideoStatusAsync(int videoId, string userId, VideoProcessingStatus status)
    {
        var video = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == videoId && v.UserId == userId);

        if (video == null)
            throw new ArgumentException("Video not found or access denied");

        video.Status = status;
        await _context.SaveChangesAsync();

        return await GetVideoByIdAsync(videoId);
    }

    public async Task<int> UpdateProcessingVideosToReadyAsync()
    {
        var processingVideos = await _context.Videos
            .Where(v => v.Status == VideoProcessingStatus.Processing)
            .ToListAsync();

        foreach (var video in processingVideos)
        {
            video.Status = VideoProcessingStatus.Ready;
            if (video.PublishedAt == null)
            {
                video.PublishedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return processingVideos.Count;
    }

    private static string? DetermineDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
            return null;

        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("BlackBerry", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("IEMobile", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("Opera Mini", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }
        
        if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase) ||
            userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "Tablet";
        }

        return "Desktop";
    }
}
