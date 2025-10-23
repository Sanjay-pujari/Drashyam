using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Drashyam.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Drashyam.API.Tests;

public class LiveStreamPollServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamPollService>> _mockLogger;
    private readonly LiveStreamPollService _pollService;

    public LiveStreamPollServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamPollService>>();

        // Setup hub context mocks
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        _mockLiveStreamHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockLiveStreamHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);
        _mockNotificationHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockNotificationHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        _pollService = new LiveStreamPollService(
            _context,
            _mockLiveStreamHub.Object,
            _mockNotificationHub.Object,
            _mockLogger.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test user
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);

        // Add test channel
        var channel = new Channel
        {
            Id = 1,
            Name = "Test Channel",
            Description = "Test Channel Description",
            UserId = "test-user-id",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Channels.Add(channel);

        // Add test live stream
        var liveStream = new LiveStream
        {
            Id = 1,
            Title = "Test Stream",
            Description = "Test Stream Description",
            UserId = "test-user-id",
            ChannelId = 1,
            StreamKey = "test-stream-key",
            Status = DTOs.LiveStreamStatus.Live,
            ViewerCount = 100,
            PeakViewerCount = 150,
            CreatedAt = DateTime.UtcNow,
            ActualStartTime = DateTime.UtcNow.AddMinutes(-30)
        };
        _context.LiveStreams.Add(liveStream);

        // Add test poll
        var poll = new LiveStreamPoll
        {
            Id = 1,
            LiveStreamId = 1,
            Question = "What's your favorite color?",
            IsActive = true,
            AllowMultipleChoices = false,
            IsAnonymous = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamPolls.Add(poll);

        // Add test poll options
        var option1 = new LiveStreamPollOption
        {
            Id = 1,
            PollId = 1,
            Text = "Red",
            Order = 1
        };
        _context.LiveStreamPollOptions.Add(option1);

        var option2 = new LiveStreamPollOption
        {
            Id = 2,
            PollId = 1,
            Text = "Blue",
            Order = 2
        };
        _context.LiveStreamPollOptions.Add(option2);

        // Add test poll votes
        var vote1 = new LiveStreamPollVote
        {
            Id = 1,
            PollId = 1,
            OptionId = 1,
            UserId = "test-user-id",
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.LiveStreamPollVotes.Add(vote1);

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreatePollAsync_ValidPoll_ReturnsPoll()
    {
        // Arrange
        var dto = new CreatePollDto
        {
            LiveStreamId = 1,
            Question = "What's your favorite food?",
            Options = new List<string> { "Pizza", "Burger", "Pasta" },
            AllowMultipleChoices = false,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _pollService.CreatePollAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Question, result.Question);
        Assert.Equal(dto.AllowMultipleChoices, result.AllowMultipleChoices);
        Assert.Equal(dto.IsAnonymous, result.IsAnonymous);
        Assert.Equal(userId, result.CreatedBy);
        Assert.NotNull(result.CreatedAt);
        Assert.NotEmpty(result.Options);
    }

    [Fact]
    public async Task CreatePollAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreatePollDto
        {
            LiveStreamId = 999,
            Question = "What's your favorite food?",
            Options = new List<string> { "Pizza", "Burger", "Pasta" },
            AllowMultipleChoices = false,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.CreatePollAsync(dto, userId));
    }

    [Fact]
    public async Task CreatePollAsync_EmptyOptions_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreatePollDto
        {
            LiveStreamId = 1,
            Question = "What's your favorite food?",
            Options = new List<string>(), // Empty options
            AllowMultipleChoices = false,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.CreatePollAsync(dto, userId));
    }

    [Fact]
    public async Task CreatePollAsync_TooManyOptions_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreatePollDto
        {
            LiveStreamId = 1,
            Question = "What's your favorite food?",
            Options = Enumerable.Range(1, 11).Select(i => $"Option {i}").ToList(), // 11 options (max is 10)
            AllowMultipleChoices = false,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.CreatePollAsync(dto, userId));
    }

    [Fact]
    public async Task GetPollsAsync_ValidStream_ReturnsPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, poll => Assert.Equal(liveStreamId, poll.LiveStreamId));
    }

    [Fact]
    public async Task GetPollsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPollsAsync_WithIsActive_ReturnsFilteredPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var isActive = true;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, isActive);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, poll => Assert.Equal(isActive, poll.IsActive));
    }

    [Fact]
    public async Task GetPollsAsync_WithIsAnonymous_ReturnsFilteredPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var isAnonymous = false;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, isAnonymous);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, poll => Assert.Equal(isAnonymous, poll.IsAnonymous));
    }

    [Fact]
    public async Task GetPollsAsync_WithAllowMultipleChoices_ReturnsFilteredPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var allowMultipleChoices = false;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, null, allowMultipleChoices);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, poll => Assert.Equal(allowMultipleChoices, poll.AllowMultipleChoices));
    }

    [Fact]
    public async Task GetPollsAsync_WithStartTime_ReturnsFilteredPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, poll => Assert.True(poll.CreatedAt >= startTime));
    }

    [Fact]
    public async Task GetPollsAsync_WithEndTime_ReturnsFilteredPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, poll => Assert.True(poll.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetPollsAsync_WithSortBy_ReturnsSortedPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPollsAsync_WithSortOrder_ReturnsSortedPolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";
        var sortOrder = "desc";

        // Act
        var result = await _pollService.GetPollsAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPollAsync_ValidPoll_ReturnsPoll()
    {
        // Arrange
        var pollId = 1;

        // Act
        var result = await _pollService.GetPollAsync(pollId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.Id);
        Assert.Equal("What's your favorite color?", result.Question);
        Assert.True(result.IsActive);
        Assert.False(result.AllowMultipleChoices);
        Assert.False(result.IsAnonymous);
        Assert.NotEmpty(result.Options);
    }

    [Fact]
    public async Task GetPollAsync_InvalidPoll_ThrowsArgumentException()
    {
        // Arrange
        var pollId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.GetPollAsync(pollId));
    }

    [Fact]
    public async Task UpdatePollAsync_ValidPoll_ReturnsUpdatedPoll()
    {
        // Arrange
        var pollId = 1;
        var dto = new UpdatePollDto
        {
            Question = "Updated question?",
            AllowMultipleChoices = true,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _pollService.UpdatePollAsync(pollId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Question, result.Question);
        Assert.Equal(dto.AllowMultipleChoices, result.AllowMultipleChoices);
        Assert.Equal(dto.IsAnonymous, result.IsAnonymous);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdatePollAsync_InvalidPoll_ThrowsArgumentException()
    {
        // Arrange
        var pollId = 999;
        var dto = new UpdatePollDto
        {
            Question = "Updated question?",
            AllowMultipleChoices = true,
            IsAnonymous = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.UpdatePollAsync(pollId, dto, userId));
    }

    [Fact]
    public async Task UpdatePollAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var pollId = 1;
        var dto = new UpdatePollDto
        {
            Question = "Updated question?",
            AllowMultipleChoices = true,
            IsAnonymous = true
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _pollService.UpdatePollAsync(pollId, dto, userId));
    }

    [Fact]
    public async Task DeletePollAsync_ValidPoll_ReturnsTrue()
    {
        // Arrange
        var pollId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _pollService.DeletePollAsync(pollId, userId);

        // Assert
        Assert.True(result);

        // Verify poll is deleted
        var poll = await _context.LiveStreamPolls.FindAsync(pollId);
        Assert.Null(poll);
    }

    [Fact]
    public async Task DeletePollAsync_InvalidPoll_ThrowsArgumentException()
    {
        // Arrange
        var pollId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.DeletePollAsync(pollId, userId));
    }

    [Fact]
    public async Task DeletePollAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var pollId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _pollService.DeletePollAsync(pollId, userId));
    }

    [Fact]
    public async Task VoteOnPollAsync_ValidVote_ReturnsVote()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 1,
            OptionIds = new List<int> { 1 }
        };
        var userId = "test-user-id";

        // Act
        var result = await _pollService.VoteOnPollAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.PollId, result.PollId);
        Assert.Equal(dto.OptionIds, result.OptionIds);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task VoteOnPollAsync_InvalidPoll_ThrowsArgumentException()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 999,
            OptionIds = new List<int> { 1 }
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.VoteOnPollAsync(dto, userId));
    }

    [Fact]
    public async Task VoteOnPollAsync_InvalidOption_ThrowsArgumentException()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 1,
            OptionIds = new List<int> { 999 } // Invalid option
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.VoteOnPollAsync(dto, userId));
    }

    [Fact]
    public async Task VoteOnPollAsync_EmptyOptions_ThrowsArgumentException()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 1,
            OptionIds = new List<int>() // Empty options
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.VoteOnPollAsync(dto, userId));
    }

    [Fact]
    public async Task VoteOnPollAsync_TooManyOptions_ThrowsArgumentException()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 1,
            OptionIds = new List<int> { 1, 2, 3, 4, 5, 6 } // Too many options (max is 5)
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _pollService.VoteOnPollAsync(dto, userId));
    }

    [Fact]
    public async Task VoteOnPollAsync_AlreadyVoted_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new VoteOnPollDto
        {
            PollId = 1,
            OptionIds = new List<int> { 2 }
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pollService.VoteOnPollAsync(dto, userId));
    }

    [Fact]
    public async Task GetPollVotesAsync_ValidPoll_ReturnsVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, vote => Assert.Equal(pollId, vote.PollId));
    }

    [Fact]
    public async Task GetPollVotesAsync_InvalidPoll_ReturnsEmptyList()
    {
        // Arrange
        var pollId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPollVotesAsync_WithUserId_ReturnsFilteredVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var userId = "test-user-id";

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, vote => Assert.Equal(userId, vote.UserId));
    }

    [Fact]
    public async Task GetPollVotesAsync_WithOptionId_ReturnsFilteredVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var optionId = 1;

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, null, optionId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, vote => Assert.Contains(optionId, vote.OptionIds));
    }

    [Fact]
    public async Task GetPollVotesAsync_WithStartTime_ReturnsFilteredVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, vote => Assert.True(vote.CreatedAt >= startTime));
    }

    [Fact]
    public async Task GetPollVotesAsync_WithEndTime_ReturnsFilteredVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, vote => Assert.True(vote.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetPollVotesAsync_WithSortBy_ReturnsSortedVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPollVotesAsync_WithSortOrder_ReturnsSortedVotes()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";
        var sortOrder = "desc";

        // Act
        var result = await _pollService.GetPollVotesAsync(pollId, page, pageSize, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetPollStatsAsync_ValidPoll_ReturnsStats()
    {
        // Arrange
        var pollId = 1;

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
        Assert.NotNull(result.OptionStats);
        Assert.NotEmpty(result.OptionStats);
    }

    [Fact]
    public async Task GetPollStatsAsync_InvalidPoll_ReturnsEmptyStats()
    {
        // Arrange
        var pollId = 999;

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.Equal(0, result.TotalVotes);
        Assert.Equal(0, result.UniqueVoters);
        Assert.Empty(result.OptionStats);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var pollId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithUserId_ReturnsFilteredStats()
    {
        // Arrange
        var pollId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, null, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithOptionId_ReturnsFilteredStats()
    {
        // Arrange
        var pollId = 1;
        var optionId = 1;

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, null, null, null, optionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var pollId = 1;
        var sortBy = "voteCount";

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var pollId = 1;
        var sortBy = "voteCount";
        var sortOrder = "desc";

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    [Fact]
    public async Task GetPollStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var pollId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _pollService.GetPollStatsAsync(pollId, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pollId, result.PollId);
        Assert.True(result.TotalVotes >= 0);
        Assert.True(result.UniqueVoters >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
