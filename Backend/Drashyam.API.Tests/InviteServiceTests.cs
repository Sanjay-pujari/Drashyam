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

public class InviteServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IEmailTemplateService> _mockTemplateService;
    private readonly Mock<ILogger<InviteService>> _mockLogger;
    private readonly InviteService _inviteService;

    public InviteServiceTests()
    {
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _mockEmailService = new Mock<IEmailService>();
        _mockTemplateService = new Mock<IEmailTemplateService>();
        _mockLogger = new Mock<ILogger<InviteService>>();

        _inviteService = new InviteService(
            _context,
            _mapper,
            _mockEmailService.Object,
            _mockTemplateService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateInviteAsync_ValidData_ReturnsInviteDto()
    {
        // Arrange
        var inviterId = "test-user-id";
        var createDto = new CreateInviteDto
        {
            InviteeEmail = "test@example.com",
            InviteeFirstName = "John",
            InviteeLastName = "Doe",
            PersonalMessage = "Join me on Drashyam!",
            Type = InviteType.Email,
            ExpirationDays = 7
        };

        _mockTemplateService.Setup(x => x.GetInviteEmailTemplate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns("Email template");

        _mockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _inviteService.CreateInviteAsync(inviterId, createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.InviteeEmail, result.InviteeEmail);
        Assert.Equal(createDto.InviteeFirstName, result.InviteeFirstName);
        Assert.Equal(createDto.InviteeLastName, result.InviteeLastName);
        Assert.Equal(InviteStatus.Pending, result.Status);
        Assert.NotNull(result.InviteToken);
    }

    [Fact]
    public async Task CreateInviteAsync_ExistingUser_ThrowsException()
    {
        // Arrange
        var inviterId = "test-user-id";
        var existingUser = new ApplicationUser
        {
            Id = "existing-user",
            Email = "test@example.com",
            FirstName = "Existing",
            LastName = "User"
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var createDto = new CreateInviteDto
        {
            InviteeEmail = "test@example.com",
            Type = InviteType.Email
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _inviteService.CreateInviteAsync(inviterId, createDto));
    }

    [Fact]
    public async Task CreateInviteAsync_ExistingPendingInvite_ThrowsException()
    {
        // Arrange
        var inviterId = "test-user-id";
        var existingInvite = new UserInvite
        {
            InviterId = inviterId,
            InviteeEmail = "test@example.com",
            Status = InviteStatus.Pending,
            InviteToken = "existing-token"
        };

        _context.UserInvites.Add(existingInvite);
        await _context.SaveChangesAsync();

        var createDto = new CreateInviteDto
        {
            InviteeEmail = "test@example.com",
            Type = InviteType.Email
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _inviteService.CreateInviteAsync(inviterId, createDto));
    }

    [Fact]
    public async Task GetInviteByTokenAsync_ValidToken_ReturnsInviteDto()
    {
        // Arrange
        var invite = new UserInvite
        {
            InviterId = "test-user-id",
            InviteeEmail = "test@example.com",
            InviteToken = "test-token",
            Status = InviteStatus.Pending
        };

        _context.UserInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetInviteByTokenAsync("test-token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.InviteToken);
    }

    [Fact]
    public async Task GetInviteByTokenAsync_InvalidToken_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _inviteService.GetInviteByTokenAsync("invalid-token"));
    }

    [Fact]
    public async Task AcceptInviteAsync_ValidToken_CreatesUserAndUpdatesInvite()
    {
        // Arrange
        var invite = new UserInvite
        {
            InviterId = "test-user-id",
            InviteeEmail = "test@example.com",
            InviteToken = "test-token",
            Status = InviteStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.UserInvites.Add(invite);
        await _context.SaveChangesAsync();

        var acceptDto = new AcceptInviteDto
        {
            InviteToken = "test-token",
            FirstName = "John",
            LastName = "Doe",
            Password = "password123"
        };

        // Act
        var result = await _inviteService.AcceptInviteAsync("test-token", acceptDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(InviteStatus.Accepted, result.Status);
        Assert.NotNull(result.AcceptedAt);

        var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(createdUser);
        Assert.Equal("John", createdUser.FirstName);
        Assert.Equal("Doe", createdUser.LastName);
    }

    [Fact]
    public async Task GetInviteStatsAsync_ReturnsCorrectStats()
    {
        // Arrange
        var userId = "test-user-id";
        var invites = new List<UserInvite>
        {
            new() { InviterId = userId, Status = InviteStatus.Pending },
            new() { InviterId = userId, Status = InviteStatus.Accepted },
            new() { InviterId = userId, Status = InviteStatus.Expired },
            new() { InviterId = userId, Status = InviteStatus.Cancelled }
        };

        _context.UserInvites.AddRange(invites);
        await _context.SaveChangesAsync();

        // Act
        var result = await _inviteService.GetInviteStatsAsync(userId);

        // Assert
        Assert.Equal(4, result.TotalInvites);
        Assert.Equal(1, result.PendingInvites);
        Assert.Equal(1, result.AcceptedInvites);
        Assert.Equal(1, result.ExpiredInvites);
        Assert.Equal(25, result.ConversionRate); // 1 accepted out of 4 total = 25%
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
