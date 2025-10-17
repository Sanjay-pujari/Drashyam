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

public class ReferralServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ReferralService>> _mockLogger;
    private readonly ReferralService _referralService;

    public ReferralServiceTests()
    {
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _mockLogger = new Mock<ILogger<ReferralService>>();

        _referralService = new ReferralService(
            _context,
            _mapper,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateReferralAsync_ValidData_ReturnsReferralDto()
    {
        // Arrange
        var referrerId = "test-referrer-id";
        var createDto = new CreateReferralDto
        {
            ReferredUserId = "test-referred-id",
            ReferralCode = "TEST123"
        };

        // Act
        var result = await _referralService.CreateReferralAsync(referrerId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(referrerId, result.ReferrerId);
        Assert.Equal(createDto.ReferredUserId, result.ReferredUserId);
        Assert.Equal(ReferralStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateReferralAsync_DuplicateReferral_ThrowsException()
    {
        // Arrange
        var referrerId = "test-referrer-id";
        var referredUserId = "test-referred-id";

        var existingReferral = new Referral
        {
            ReferrerId = referrerId,
            ReferredUserId = referredUserId,
            Status = ReferralStatus.Pending
        };

        _context.Referrals.Add(existingReferral);
        await _context.SaveChangesAsync();

        var createDto = new CreateReferralDto
        {
            ReferredUserId = referredUserId
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _referralService.CreateReferralAsync(referrerId, createDto));
    }

    [Fact]
    public async Task CreateReferralCodeAsync_ValidData_ReturnsCodeDto()
    {
        // Arrange
        var userId = "test-user-id";
        var createDto = new CreateReferralCodeDto
        {
            Code = "CUSTOM123",
            MaxUsage = 100,
            RewardAmount = 15.00m,
            RewardType = "Points"
        };

        // Act
        var result = await _referralService.CreateReferralCodeAsync(userId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CUSTOM123", result.Code);
        Assert.Equal(100, result.MaxUsage);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateReferralCodeAsync_DuplicateCode_ThrowsException()
    {
        // Arrange
        var userId = "test-user-id";
        var existingReferral = new Referral
        {
            ReferrerId = userId,
            ReferredUserId = userId,
            ReferralCode = "EXISTING123",
            Status = ReferralStatus.Pending
        };

        _context.Referrals.Add(existingReferral);
        await _context.SaveChangesAsync();

        var createDto = new CreateReferralCodeDto
        {
            Code = "EXISTING123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _referralService.CreateReferralCodeAsync(userId, createDto));
    }

    [Fact]
    public async Task GetReferralStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var userId = "test-user-id";
        var referrals = new List<Referral>
        {
            new() { ReferrerId = userId, Status = ReferralStatus.Pending },
            new() { ReferrerId = userId, Status = ReferralStatus.Completed },
            new() { ReferrerId = userId, Status = ReferralStatus.Rewarded }
        };

        var rewards = new List<ReferralReward>
        {
            new() { UserId = userId, Amount = 10.00m, Status = RewardStatus.Pending },
            new() { UserId = userId, Amount = 15.00m, Status = RewardStatus.Claimed }
        };

        _context.Referrals.AddRange(referrals);
        _context.ReferralRewards.AddRange(rewards);
        await _context.SaveChangesAsync();

        // Act
        var result = await _referralService.GetReferralStatsAsync(userId);

        // Assert
        Assert.Equal(3, result.TotalReferrals);
        Assert.Equal(1, result.CompletedReferrals);
        Assert.Equal(1, result.PendingReferrals);
        Assert.Equal(25.00m, result.TotalRewards);
        Assert.Equal(10.00m, result.PendingRewards);
        Assert.Equal(33.33m, result.ConversionRate, 2); // 1 completed out of 3 total ≈ 33.33%
    }

    [Fact]
    public async Task ClaimRewardAsync_ValidReward_UpdatesStatus()
    {
        // Arrange
        var userId = "test-user-id";
        var reward = new ReferralReward
        {
            UserId = userId,
            Amount = 10.00m,
            Status = RewardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _context.ReferralRewards.Add(reward);
        await _context.SaveChangesAsync();

        var claimDto = new ClaimRewardDto
        {
            RewardId = reward.Id
        };

        // Act
        var result = await _referralService.ClaimRewardAsync(userId, claimDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(RewardStatus.Claimed, result.Status);
        Assert.NotNull(result.ClaimedAt);

        var updatedReward = await _context.ReferralRewards.FindAsync(reward.Id);
        Assert.Equal(RewardStatus.Claimed, updatedReward.Status);
    }

    [Fact]
    public async Task ClaimRewardAsync_ExpiredReward_ThrowsException()
    {
        // Arrange
        var userId = "test-user-id";
        var reward = new ReferralReward
        {
            UserId = userId,
            Amount = 10.00m,
            Status = RewardStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _context.ReferralRewards.Add(reward);
        await _context.SaveChangesAsync();

        var claimDto = new ClaimRewardDto
        {
            RewardId = reward.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _referralService.ClaimRewardAsync(userId, claimDto));
    }

    [Fact]
    public async Task ProcessReferralRewardAsync_CompletedReferral_CreatesReward()
    {
        // Arrange
        var referral = new Referral
        {
            ReferrerId = "test-referrer-id",
            ReferredUserId = "test-referred-id",
            Status = ReferralStatus.Completed,
            RewardAmount = 20.00m,
            RewardType = "Points"
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // Act
        var result = await _referralService.ProcessReferralRewardAsync(referral.Id);

        // Assert
        Assert.True(result);

        var createdReward = await _context.ReferralRewards
            .FirstOrDefaultAsync(r => r.ReferralId == referral.Id);
        
        Assert.NotNull(createdReward);
        Assert.Equal(referral.ReferrerId, createdReward.UserId);
        Assert.Equal(20.00m, createdReward.Amount);
        Assert.Equal("Points", createdReward.RewardType);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
