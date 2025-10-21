using Microsoft.Extensions.Caching.Memory;
using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IRecommendationCacheService
{
    Task<List<RecommendationDto>> GetCachedRecommendationsAsync(string userId, string cacheKey);
    Task SetCachedRecommendationsAsync(string userId, string cacheKey, List<RecommendationDto> recommendations, TimeSpan expiration);
    Task<List<TrendingVideoDto>> GetCachedTrendingVideosAsync(string cacheKey);
    Task SetCachedTrendingVideosAsync(string cacheKey, List<TrendingVideoDto> trendingVideos, TimeSpan expiration);
    Task InvalidateUserRecommendationsAsync(string userId);
    Task InvalidateTrendingVideosAsync();
}

public class RecommendationCacheService : IRecommendationCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RecommendationCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);
    private readonly TimeSpan _trendingExpiration = TimeSpan.FromHours(1);

    public RecommendationCacheService(
        IMemoryCache cache,
        ILogger<RecommendationCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<RecommendationDto>> GetCachedRecommendationsAsync(string userId, string cacheKey)
    {
        try
        {
            var fullKey = $"recommendations:{userId}:{cacheKey}";
            if (_cache.TryGetValue(fullKey, out List<RecommendationDto>? cachedRecommendations))
            {
                _logger.LogDebug("Cache hit for recommendations: {Key}", fullKey);
                return cachedRecommendations ?? new List<RecommendationDto>();
            }

            _logger.LogDebug("Cache miss for recommendations: {Key}", fullKey);
            return new List<RecommendationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached recommendations for user {UserId}", userId);
            return new List<RecommendationDto>();
        }
    }

    public async Task SetCachedRecommendationsAsync(string userId, string cacheKey, List<RecommendationDto> recommendations, TimeSpan expiration)
    {
        try
        {
            var fullKey = $"recommendations:{userId}:{cacheKey}";
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(fullKey, recommendations, cacheOptions);
            _logger.LogDebug("Cached recommendations for user {UserId} with key {Key}", userId, cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching recommendations for user {UserId}", userId);
        }
    }

    public async Task<List<TrendingVideoDto>> GetCachedTrendingVideosAsync(string cacheKey)
    {
        try
        {
            var fullKey = $"trending:{cacheKey}";
            if (_cache.TryGetValue(fullKey, out List<TrendingVideoDto>? cachedTrending))
            {
                _logger.LogDebug("Cache hit for trending videos: {Key}", fullKey);
                return cachedTrending ?? new List<TrendingVideoDto>();
            }

            _logger.LogDebug("Cache miss for trending videos: {Key}", fullKey);
            return new List<TrendingVideoDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached trending videos");
            return new List<TrendingVideoDto>();
        }
    }

    public async Task SetCachedTrendingVideosAsync(string cacheKey, List<TrendingVideoDto> trendingVideos, TimeSpan expiration)
    {
        try
        {
            var fullKey = $"trending:{cacheKey}";
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(15),
                Priority = CacheItemPriority.High
            };

            _cache.Set(fullKey, trendingVideos, cacheOptions);
            _logger.LogDebug("Cached trending videos with key {Key}", cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching trending videos");
        }
    }

    public async Task InvalidateUserRecommendationsAsync(string userId)
    {
        try
        {
            // Remove all recommendation caches for the user
            var keysToRemove = new List<string>
            {
                $"recommendations:{userId}:personalized",
                $"recommendations:{userId}:trending",
                $"recommendations:{userId}:category",
                $"recommendations:{userId}:similar"
            };

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            _logger.LogDebug("Invalidated recommendation cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating recommendation cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateTrendingVideosAsync()
    {
        try
        {
            // Remove all trending video caches
            var keysToRemove = new List<string>
            {
                "trending:all",
                "trending:entertainment",
                "trending:technology",
                "trending:gaming",
                "trending:music"
            };

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            _logger.LogDebug("Invalidated trending videos cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating trending videos cache");
        }
    }
}
