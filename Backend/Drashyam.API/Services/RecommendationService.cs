using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class RecommendationService : IRecommendationService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<RecommendationService> _logger;
    private readonly IRecommendationCacheService _cacheService;

    public RecommendationService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<RecommendationService> logger,
        IRecommendationCacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<List<RecommendationDto>> GetPersonalizedRecommendationsAsync(string userId, RecommendationRequestDto request)
    {
        try
        {
            var recommendations = new List<RecommendationDto>();

            // Get existing recommendations
            var existingRecommendations = await _context.Recommendations
                .Include(r => r.Video)
                    .ThenInclude(v => v.User)
                .Include(r => r.Video)
                    .ThenInclude(v => v.Channel)
                .Where(r => r.UserId == userId && r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.Score)
                .Take(request.Limit ?? 20)
                .ToListAsync();

            if (existingRecommendations.Any())
            {
                recommendations.AddRange(_mapper.Map<List<RecommendationDto>>(existingRecommendations));
            }
            else
            {
                // Generate new recommendations
                recommendations = await GeneratePersonalizedRecommendationsAsync(userId, request);
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations for user {UserId}", userId);
            return new List<RecommendationDto>();
        }
    }

    public async Task<List<TrendingVideoDto>> GetTrendingVideosAsync(string? category = null, string? country = null, int limit = 20)
    {
        try
        {
            var query = _context.TrendingVideos
                .Include(tv => tv.Video)
                    .ThenInclude(v => v.User)
                .Include(tv => tv.Video)
                    .ThenInclude(v => v.Channel)
                .Where(tv => tv.ExpiresAt > DateTime.UtcNow);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(tv => tv.Category == category);
            }

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(tv => tv.Country == country);
            }

            var trendingVideos = await query
                .OrderBy(tv => tv.Position)
                .Take(limit)
                .ToListAsync();

            return _mapper.Map<List<TrendingVideoDto>>(trendingVideos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending videos");
            return new List<TrendingVideoDto>();
        }
    }

    public async Task<List<RecommendationDto>> GetSimilarVideosAsync(int videoId, int limit = 10)
    {
        try
        {
            var video = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.Channel)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null)
                return new List<RecommendationDto>();

            // Find similar videos based on category, tags, and user
            var similarVideos = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.Channel)
                .Where(v => v.Id != videoId && v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
                .Where(v => v.Category == video.Category || 
                           (v.Tags != null && video.Tags != null && v.Tags.Contains(video.Tags)) ||
                           v.UserId == video.UserId)
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.LikeCount)
                .Take(limit)
                .ToListAsync();

            var recommendations = similarVideos.Select((v, index) => new RecommendationDto
            {
                VideoId = v.Id,
                Type = "Similar",
                Score = 1.0m - (index * 0.1m),
                Reason = $"Similar to {video.Title}",
                CreatedAt = DateTime.UtcNow,
                Video = _mapper.Map<VideoDto>(v)
            }).ToList();

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar videos for video {VideoId}", videoId);
            return new List<RecommendationDto>();
        }
    }

    public async Task<List<RecommendationDto>> GetCategoryRecommendationsAsync(string category, int limit = 20)
    {
        try
        {
            var videos = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.Channel)
                .Where(v => v.Category == category && v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.LikeCount)
                .Take(limit)
                .ToListAsync();

            var recommendations = videos.Select((v, index) => new RecommendationDto
            {
                VideoId = v.Id,
                Type = "Category",
                Score = 1.0m - (index * 0.05m),
                Reason = $"Popular in {category}",
                CreatedAt = DateTime.UtcNow,
                Video = _mapper.Map<VideoDto>(v)
            }).ToList();

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category recommendations for {Category}", category);
            return new List<RecommendationDto>();
        }
    }

    public async Task<List<RecommendationDto>> GetChannelRecommendationsAsync(int channelId, int limit = 20)
    {
        try
        {
            var videos = await _context.Videos
                .Include(v => v.User)
                .Include(v => v.Channel)
                .Where(v => v.ChannelId == channelId && v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
                .OrderByDescending(v => v.CreatedAt)
                .Take(limit)
                .ToListAsync();

            var recommendations = videos.Select((v, index) => new RecommendationDto
            {
                VideoId = v.Id,
                Type = "Channel",
                Score = 1.0m - (index * 0.05m),
                Reason = "From this channel",
                CreatedAt = DateTime.UtcNow,
                Video = _mapper.Map<VideoDto>(v)
            }).ToList();

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel recommendations for channel {ChannelId}", channelId);
            return new List<RecommendationDto>();
        }
    }

    public async Task TrackInteractionAsync(string userId, InteractionDto interaction)
    {
        try
        {
            var userInteraction = new UserInteraction
            {
                UserId = userId,
                VideoId = interaction.VideoId,
                Type = Enum.Parse<InteractionType>(interaction.Type),
                Score = interaction.Score ?? 1.0m,
                WatchDuration = interaction.WatchDuration,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserInteractions.Add(userInteraction);
            await _context.SaveChangesAsync();

            // Update user preferences based on interaction
            await UpdateUserPreferencesFromInteractionAsync(userId, interaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking interaction for user {UserId}", userId);
        }
    }

    public async Task UpdateUserPreferencesAsync(string userId, List<UserPreferenceDto> preferences)
    {
        try
        {
            // Remove existing preferences
            var existingPreferences = await _context.UserPreferences
                .Where(p => p.UserId == userId)
                .ToListAsync();

            _context.UserPreferences.RemoveRange(existingPreferences);

            // Add new preferences
            var newPreferences = preferences.Select(p => new UserPreference
            {
                UserId = userId,
                Category = p.Category,
                Tag = p.Tag,
                Weight = p.Weight,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _context.UserPreferences.AddRange(newPreferences);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user {UserId}", userId);
        }
    }

    public async Task<List<UserPreferenceDto>> GetUserPreferencesAsync(string userId)
    {
        try
        {
            var preferences = await _context.UserPreferences
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Weight)
                .ToListAsync();

            return _mapper.Map<List<UserPreferenceDto>>(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences for user {UserId}", userId);
            return new List<UserPreferenceDto>();
        }
    }

    public async Task RecordRecommendationFeedbackAsync(string userId, RecommendationFeedbackDto feedback)
    {
        try
        {
            var recommendation = await _context.Recommendations
                .FirstOrDefaultAsync(r => r.Id == feedback.RecommendationId && r.UserId == userId);

            if (recommendation != null)
            {
                recommendation.IsShown = true;
                recommendation.IsClicked = feedback.IsClicked;

                if (feedback.IsClicked)
                {
                    // Track positive interaction
                    await TrackInteractionAsync(userId, new InteractionDto
                    {
                        VideoId = recommendation.VideoId,
                        Type = "View",
                        Score = 1.0m,
                        WatchDuration = feedback.WatchDuration
                    });
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording recommendation feedback for user {UserId}", userId);
        }
    }

    public async Task CalculateTrendingVideosAsync()
    {
        try
        {
            // Clear existing trending videos
            var existingTrending = await _context.TrendingVideos.ToListAsync();
            _context.TrendingVideos.RemoveRange(existingTrending);

            // Calculate trending scores for videos from the last 7 days
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var videos = await _context.Videos
                .Where(v => v.CreatedAt >= sevenDaysAgo && v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
                .ToListAsync();

            var trendingVideos = new List<TrendingVideo>();

            foreach (var video in videos)
            {
                var trendingScore = CalculateTrendingScore(video);
                
                if (trendingScore > 0)
                {
                    trendingVideos.Add(new TrendingVideo
                    {
                        VideoId = video.Id,
                        Category = video.Category,
                        Country = null, // Could be enhanced with user location
                        TrendingScore = trendingScore,
                        Position = 0, // Will be set after sorting
                        CalculatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    });
                }
            }

            // Sort by trending score and set positions
            trendingVideos = trendingVideos
                .OrderByDescending(tv => tv.TrendingScore)
                .Select((tv, index) => { tv.Position = index + 1; return tv; })
                .ToList();

            _context.TrendingVideos.AddRange(trendingVideos);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Calculated trending videos: {Count} videos", trendingVideos.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trending videos");
        }
    }

    public async Task UpdateRecommendationsAsync(string userId)
    {
        try
        {
            // Remove expired recommendations
            var expiredRecommendations = await _context.Recommendations
                .Where(r => r.UserId == userId && r.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.Recommendations.RemoveRange(expiredRecommendations);

            // Generate new recommendations
            var request = new RecommendationRequestDto { Limit = 50 };
            var newRecommendations = await GeneratePersonalizedRecommendationsAsync(userId, request);

            // Save new recommendations to database
            var recommendations = newRecommendations.Select(r => new Recommendation
            {
                UserId = userId,
                VideoId = r.VideoId,
                Type = Enum.Parse<RecommendationType>(r.Type),
                Score = r.Score,
                Reason = r.Reason,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            }).ToList();

            _context.Recommendations.AddRange(recommendations);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recommendations for user {UserId}", userId);
        }
    }

    private async Task<List<RecommendationDto>> GeneratePersonalizedRecommendationsAsync(string userId, RecommendationRequestDto request)
    {
        var recommendations = new List<RecommendationDto>();

        // Get user preferences
        var userPreferences = await GetUserPreferencesAsync(userId);
        var userInteractions = await _context.UserInteractions
            .Where(i => i.UserId == userId)
            .Include(i => i.Video)
            .ToListAsync();

        // Content-based filtering
        if (userPreferences.Any())
        {
            var contentBasedRecommendations = await GetContentBasedRecommendationsAsync(userId, userPreferences, request.Limit ?? 20);
            recommendations.AddRange(contentBasedRecommendations);
        }

        // Collaborative filtering
        var collaborativeRecommendations = await GetCollaborativeRecommendationsAsync(userId, userInteractions, request.Limit ?? 20);
        recommendations.AddRange(collaborativeRecommendations);

        // Trending videos
        if (request.IncludeTrending)
        {
            var trendingRecommendations = await GetTrendingRecommendationsAsync(request.Limit ?? 20);
            recommendations.AddRange(trendingRecommendations);
        }

        // Remove duplicates and sort by score
        return recommendations
            .GroupBy(r => r.VideoId)
            .Select(g => g.OrderByDescending(r => r.Score).First())
            .OrderByDescending(r => r.Score)
            .Take(request.Limit ?? 20)
            .ToList();
    }

    private async Task<List<RecommendationDto>> GetContentBasedRecommendationsAsync(string userId, List<UserPreferenceDto> preferences, int limit)
    {
        var categories = preferences.Where(p => !string.IsNullOrEmpty(p.Category)).Select(p => p.Category).ToList();
        var tags = preferences.Where(p => !string.IsNullOrEmpty(p.Tag)).Select(p => p.Tag).ToList();

        var videos = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
            .Where(v => categories.Contains(v.Category) || 
                       (v.Tags != null && tags.Any(tag => v.Tags.Contains(tag))))
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return videos.Select((v, index) => new RecommendationDto
        {
            VideoId = v.Id,
            Type = "Personalized",
            Score = 0.8m - (index * 0.05m),
            Reason = "Based on your preferences",
            CreatedAt = DateTime.UtcNow,
            Video = _mapper.Map<VideoDto>(v)
        }).ToList();
    }

    private async Task<List<RecommendationDto>> GetCollaborativeRecommendationsAsync(string userId, List<UserInteraction> interactions, int limit)
    {
        // Find users with similar interactions
        var similarUsers = await _context.UserInteractions
            .Where(i => i.UserId != userId && interactions.Any(ui => ui.VideoId == i.VideoId))
            .Select(i => i.UserId)
            .Distinct()
            .ToListAsync();

        if (!similarUsers.Any())
            return new List<RecommendationDto>();

        // Get videos liked by similar users
        var recommendedVideos = await _context.UserInteractions
            .Include(i => i.Video)
                .ThenInclude(v => v.User)
            .Include(i => i.Video)
                .ThenInclude(v => v.Channel)
            .Where(i => similarUsers.Contains(i.UserId) && 
                       i.Type == InteractionType.Like &&
                       !interactions.Any(ui => ui.VideoId == i.VideoId))
            .Select(i => i.Video)
            .Where(v => v.Status == VideoProcessingStatus.Ready && v.Visibility == Models.VideoVisibility.Public)
            .Distinct()
            .OrderByDescending(v => v.ViewCount)
            .Take(limit)
            .ToListAsync();

        return recommendedVideos.Select((v, index) => new RecommendationDto
        {
            VideoId = v.Id,
            Type = "Collaborative",
            Score = 0.7m - (index * 0.05m),
            Reason = "Users with similar tastes liked this",
            CreatedAt = DateTime.UtcNow,
            Video = _mapper.Map<VideoDto>(v)
        }).ToList();
    }

    private async Task<List<RecommendationDto>> GetTrendingRecommendationsAsync(int limit)
    {
        var trendingVideos = await GetTrendingVideosAsync(limit: limit);
        return trendingVideos.Select(tv => new RecommendationDto
        {
            VideoId = tv.VideoId,
            Type = "Trending",
            Score = (decimal)tv.TrendingScore,
            Reason = "Trending now",
            CreatedAt = DateTime.UtcNow,
            Video = tv.Video
        }).ToList();
    }

    private decimal CalculateTrendingScore(Video video)
    {
        var timeDecay = Math.Max(0.1, 1.0 - (DateTime.UtcNow - video.CreatedAt).TotalDays / 7.0);
        var engagementScore = (video.LikeCount * 2 + video.CommentCount * 3 + video.ViewCount) / 1000.0;
        var velocityScore = video.ViewCount / Math.Max(1, (DateTime.UtcNow - video.CreatedAt).TotalHours);
        
        return (decimal)(timeDecay * (engagementScore + velocityScore));
    }

    private async Task UpdateUserPreferencesFromInteractionAsync(string userId, InteractionDto interaction)
    {
        try
        {
            var video = await _context.Videos.FindAsync(interaction.VideoId);
            if (video == null) return;

            var weight = interaction.Score ?? 1.0m;

            // Update category preference
            if (!string.IsNullOrEmpty(video.Category))
            {
                var categoryPreference = await _context.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == video.Category);

                if (categoryPreference != null)
                {
                    categoryPreference.Weight = Math.Min(10.0m, categoryPreference.Weight + weight);
                    categoryPreference.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.UserPreferences.Add(new UserPreference
                    {
                        UserId = userId,
                        Category = video.Category,
                        Weight = weight,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }

            // Update tag preferences
            if (!string.IsNullOrEmpty(video.Tags))
            {
                var tags = video.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var tag in tags)
                {
                    var tagPreference = await _context.UserPreferences
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.Tag == tag.Trim());

                    if (tagPreference != null)
                    {
                        tagPreference.Weight = Math.Min(10.0m, tagPreference.Weight + weight);
                        tagPreference.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.UserPreferences.Add(new UserPreference
                        {
                            UserId = userId,
                            Tag = tag.Trim(),
                            Weight = weight,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences from interaction");
        }
    }
}
