using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetPersonalizedRecommendationsAsync(string userId, RecommendationRequestDto request);
    Task<List<TrendingVideoDto>> GetTrendingVideosAsync(string? category = null, string? country = null, int limit = 20);
    Task<List<RecommendationDto>> GetSimilarVideosAsync(int videoId, int limit = 10);
    Task<List<RecommendationDto>> GetCategoryRecommendationsAsync(string category, int limit = 20);
    Task<List<RecommendationDto>> GetChannelRecommendationsAsync(int channelId, int limit = 20);
    Task TrackInteractionAsync(string userId, InteractionDto interaction);
    Task UpdateUserPreferencesAsync(string userId, List<UserPreferenceDto> preferences);
    Task<List<UserPreferenceDto>> GetUserPreferencesAsync(string userId);
    Task RecordRecommendationFeedbackAsync(string userId, RecommendationFeedbackDto feedback);
    Task CalculateTrendingVideosAsync();
    Task UpdateRecommendationsAsync(string userId);
}
