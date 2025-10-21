using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Drashyam.API.Mapping;

namespace Drashyam.API.Tests;

public class RecommendationServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<RecommendationService>> _mockLogger;
    private readonly RecommendationService _recommendationService;

    public RecommendationServiceTests()
    {
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _mockLogger = new Mock<ILogger<RecommendationService>>();

        _recommendationService = new RecommendationService(
            _context,
            _mapper,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetPersonalizedRecommendationsAsync_WithUserPreferences_ReturnsRecommendations()
    {
        // Arrange
        var userId = "test-user-id";
        var videoId = 1;
        
        // Create test data
        var user = new ApplicationUser { Id = userId, FirstName = "Test", LastName = "User" };
        var video = new Video 
        { 
            Id = videoId, 
            Title = "Test Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public,
            ViewCount = 100,
            LikeCount = 10
        };
        
        var userPreference = new UserPreference
        {
            UserId = userId,
            Category = "Technology",
            Weight = 1.0m
        };

        _context.Users.Add(user);
        _context.Videos.Add(video);
        _context.UserPreferences.Add(userPreference);
        await _context.SaveChangesAsync();

        var request = new RecommendationRequestDto { Limit = 10 };

        // Act
        var result = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<RecommendationDto>>(result);
    }

    [Fact]
    public async Task GetTrendingVideosAsync_ReturnsTrendingVideos()
    {
        // Arrange
        var video = new Video 
        { 
            Id = 1, 
            Title = "Trending Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public,
            ViewCount = 1000,
            LikeCount = 100,
            Category = "Entertainment"
        };

        var trendingVideo = new TrendingVideo
        {
            VideoId = video.Id,
            Category = "Entertainment",
            TrendingScore = 0.85m,
            Position = 1,
            CalculatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _context.Videos.Add(video);
        _context.TrendingVideos.Add(trendingVideo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _recommendationService.GetTrendingVideosAsync("Entertainment", null, 10);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<TrendingVideoDto>>(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task TrackInteractionAsync_ValidInteraction_RecordsInteraction()
    {
        // Arrange
        var userId = "test-user-id";
        var interaction = new InteractionDto
        {
            VideoId = 1,
            Type = "Like",
            Score = 1.0m,
            WatchDuration = "30"
        };

        // Create test video
        var video = new Video 
        { 
            Id = 1, 
            Title = "Test Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public
        };
        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        await _recommendationService.TrackInteractionAsync(userId, interaction);

        // Assert
        var recordedInteraction = await _context.UserInteractions
            .FirstOrDefaultAsync(i => i.UserId == userId && i.VideoId == 1);
        
        Assert.NotNull(recordedInteraction);
        Assert.Equal(InteractionType.Like, recordedInteraction.Type);
        Assert.Equal(1.0m, recordedInteraction.Score);
    }

    [Fact]
    public async Task UpdateUserPreferencesAsync_ValidPreferences_UpdatesPreferences()
    {
        // Arrange
        var userId = "test-user-id";
        var preferences = new List<UserPreferenceDto>
        {
            new() { Category = "Technology", Weight = 1.0m },
            new() { Tag = "programming", Weight = 0.8m }
        };

        // Act
        await _recommendationService.UpdateUserPreferencesAsync(userId, preferences);

        // Assert
        var savedPreferences = await _context.UserPreferences
            .Where(p => p.UserId == userId)
            .ToListAsync();
        
        Assert.Equal(2, savedPreferences.Count);
        Assert.Contains(savedPreferences, p => p.Category == "Technology");
        Assert.Contains(savedPreferences, p => p.Tag == "programming");
    }

    [Fact]
    public async Task GetSimilarVideosAsync_WithValidVideo_ReturnsSimilarVideos()
    {
        // Arrange
        var videoId = 1;
        var similarVideo = new Video 
        { 
            Id = 2, 
            Title = "Similar Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public,
            Category = "Technology"
        };

        var originalVideo = new Video 
        { 
            Id = videoId, 
            Title = "Original Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public,
            Category = "Technology"
        };

        _context.Videos.Add(originalVideo);
        _context.Videos.Add(similarVideo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _recommendationService.GetSimilarVideosAsync(videoId, 10);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<RecommendationDto>>(result);
    }

    [Fact]
    public async Task CalculateTrendingVideosAsync_WithVideos_CalculatesTrendingScores()
    {
        // Arrange
        var video = new Video 
        { 
            Id = 1, 
            Title = "Popular Video", 
            UserId = "creator-id",
            Status = VideoProcessingStatus.Ready,
            Visibility = Models.VideoVisibility.Public,
            ViewCount = 1000,
            LikeCount = 100,
            CommentCount = 50,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.Videos.Add(video);
        await _context.SaveChangesAsync();

        // Act
        await _recommendationService.CalculateTrendingVideosAsync();

        // Assert
        var trendingVideos = await _context.TrendingVideos.ToListAsync();
        Assert.NotEmpty(trendingVideos);
        Assert.Contains(trendingVideos, tv => tv.VideoId == 1);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
