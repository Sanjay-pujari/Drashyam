using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(
        IRecommendationService recommendationService,
        ILogger<RecommendationController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpGet("personalized")]
    public async Task<ActionResult<List<RecommendationDto>>> GetPersonalizedRecommendations([FromQuery] RecommendationRequestDto request)
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var recommendations = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, request);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TrendingVideoDto>>> GetTrendingVideos(
        [FromQuery] string? category = null,
        [FromQuery] string? country = null,
        [FromQuery] int limit = 20)
    {
        try
        {
            var trendingVideos = await _recommendationService.GetTrendingVideosAsync(category, country, limit);
            return Ok(trendingVideos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending videos");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("similar/{videoId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RecommendationDto>>> GetSimilarVideos([FromRoute] int videoId, [FromQuery] int limit = 10)
    {
        try
        {
            var similarVideos = await _recommendationService.GetSimilarVideosAsync(videoId, limit);
            return Ok(similarVideos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar videos for video {VideoId}", videoId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RecommendationDto>>> GetCategoryRecommendations([FromRoute] string category, [FromQuery] int limit = 20)
    {
        try
        {
            var recommendations = await _recommendationService.GetCategoryRecommendationsAsync(category, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category recommendations for {Category}", category);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("channel/{channelId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RecommendationDto>>> GetChannelRecommendations([FromRoute] int channelId, [FromQuery] int limit = 20)
    {
        try
        {
            var recommendations = await _recommendationService.GetChannelRecommendationsAsync(channelId, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel recommendations for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("interaction")]
    public async Task<ActionResult> TrackInteraction([FromBody] InteractionDto interaction)
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            await _recommendationService.TrackInteractionAsync(userId, interaction);
            return Ok(new { message = "Interaction tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking interaction");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("preferences")]
    public async Task<ActionResult> UpdatePreferences([FromBody] List<UserPreferenceDto> preferences)
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            await _recommendationService.UpdateUserPreferencesAsync(userId, preferences);
            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<List<UserPreferenceDto>>> GetPreferences()
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            var preferences = await _recommendationService.GetUserPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("feedback")]
    public async Task<ActionResult> RecordFeedback([FromBody] RecommendationFeedbackDto feedback)
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            await _recommendationService.RecordRecommendationFeedbackAsync(userId, feedback);
            return Ok(new { message = "Feedback recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording recommendation feedback");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("update")]
    public async Task<ActionResult> UpdateRecommendations()
    {
        try
        {
            var userId = User.Identity?.Name ?? string.Empty;
            await _recommendationService.UpdateRecommendationsAsync(userId);
            return Ok(new { message = "Recommendations updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recommendations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("calculate-trending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CalculateTrendingVideos()
    {
        try
        {
            await _recommendationService.CalculateTrendingVideosAsync();
            return Ok(new { message = "Trending videos calculated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trending videos");
            return StatusCode(500, "Internal server error");
        }
    }
}
