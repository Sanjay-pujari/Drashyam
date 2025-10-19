using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class InviteService : IInviteService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IAnalyticsService _analyticsService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InviteService> _logger;

    public InviteService(
        DrashyamDbContext context,
        IMapper mapper,
        IEmailService emailService,
        IEmailTemplateService templateService,
        IAnalyticsService analyticsService,
        INotificationService notificationService,
        ILogger<InviteService> logger)
    {
        _context = context;
        _mapper = mapper;
        _emailService = emailService;
        _templateService = templateService;
        _analyticsService = analyticsService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<UserInviteDto> CreateInviteAsync(string inviterId, CreateInviteDto createDto)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == createDto.InviteeEmail);

        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        // Check for existing pending invite
        var existingInvite = await _context.UserInvites
            .FirstOrDefaultAsync(i => i.InviteeEmail == createDto.InviteeEmail && 
                                     i.Status == InviteStatus.Pending);

        if (existingInvite != null)
            throw new InvalidOperationException("Pending invite already exists for this email");

        var invite = new UserInvite
        {
            InviterId = inviterId,
            InviteeEmail = createDto.InviteeEmail,
            InviteeFirstName = createDto.InviteeFirstName,
            InviteeLastName = createDto.InviteeLastName,
            InviteToken = GenerateInviteToken(),
            PersonalMessage = createDto.PersonalMessage,
            Type = createDto.Type,
            ExpiresAt = DateTime.UtcNow.AddDays(createDto.ExpirationDays ?? 7)
        };

        _context.UserInvites.Add(invite);
        await _context.SaveChangesAsync();

        // Send invite email
        await SendInviteEmailAsync(invite);

        // Track analytics
        await _analyticsService.TrackInviteEventAsync(inviterId, InviteEventType.Created, invite.Id, "Invite created", createDto.Type.ToString());

        // TODO: Add notification for invite sent if needed

        return await GetInviteDtoAsync(invite.Id);
    }

    public async Task<UserInviteDto> GetInviteByTokenAsync(string token)
    {
        var invite = await _context.UserInvites
            .Include(i => i.Inviter)
            .Include(i => i.AcceptedUser)
            .FirstOrDefaultAsync(i => i.InviteToken == token);

        if (invite == null)
            throw new ArgumentException("Invalid invite token");

        return _mapper.Map<UserInviteDto>(invite);
    }

    public async Task<UserInviteDto> AcceptInviteAsync(string token, AcceptInviteDto acceptDto)
    {
        var invite = await _context.UserInvites
            .Include(i => i.Inviter)
            .FirstOrDefaultAsync(i => i.InviteToken == token);

        if (invite == null)
            throw new ArgumentException("Invalid invite token");

        if (invite.Status != InviteStatus.Pending)
            throw new InvalidOperationException("Invite has already been processed");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("Invite has expired");

        // Create user account
        var user = new ApplicationUser
        {
            UserName = invite.InviteeEmail,
            Email = invite.InviteeEmail,
            FirstName = acceptDto.FirstName,
            LastName = acceptDto.LastName,
            EmailConfirmed = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Update invite
        invite.Status = InviteStatus.Accepted;
        invite.AcceptedAt = DateTime.UtcNow;
        invite.AcceptedUserId = user.Id;

        await _context.SaveChangesAsync();

        // Create referral if applicable
        await CreateReferralFromInviteAsync(invite, user.Id);

        // Track analytics
        await _analyticsService.TrackInviteEventAsync(invite.InviterId, InviteEventType.Accepted, invite.Id, "Invite accepted");

        // TODO: Add notification for invite accepted if needed

        return await GetInviteDtoAsync(invite.Id);
    }

    public async Task<PagedResult<UserInviteDto>> GetUserInvitesAsync(string userId, int page = 1, int pageSize = 20)
    {
        var invites = await _context.UserInvites
            .Include(i => i.AcceptedUser)
            .Where(i => i.InviterId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.UserInvites
            .Where(i => i.InviterId == userId)
            .CountAsync();

        return new PagedResult<UserInviteDto>
        {
            Items = _mapper.Map<List<UserInviteDto>>(invites),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<UserInviteDto>> GetInvitesByEmailAsync(string email, int page = 1, int pageSize = 20)
    {
        var invites = await _context.UserInvites
            .Include(i => i.Inviter)
            .Include(i => i.AcceptedUser)
            .Where(i => i.InviteeEmail == email)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.UserInvites
            .Where(i => i.InviteeEmail == email)
            .CountAsync();

        return new PagedResult<UserInviteDto>
        {
            Items = _mapper.Map<List<UserInviteDto>>(invites),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> CancelInviteAsync(int inviteId, string userId)
    {
        var invite = await _context.UserInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.InviterId == userId);

        if (invite == null)
            return false;

        if (invite.Status != InviteStatus.Pending)
            return false;

        invite.Status = InviteStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResendInviteAsync(int inviteId, string userId)
    {
        var invite = await _context.UserInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.InviterId == userId);

        if (invite == null)
            return false;

        if (invite.Status != InviteStatus.Pending)
            return false;

        // Update expiration
        invite.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();
        await SendInviteEmailAsync(invite);
        return true;
    }

    public async Task<InviteStatsDto> GetInviteStatsAsync(string userId)
    {
        var invites = await _context.UserInvites
            .Where(i => i.InviterId == userId)
            .ToListAsync();

        var totalInvites = invites.Count;
        var pendingInvites = invites.Count(i => i.Status == InviteStatus.Pending);
        var acceptedInvites = invites.Count(i => i.Status == InviteStatus.Accepted);
        var expiredInvites = invites.Count(i => i.Status == InviteStatus.Expired);

        var conversionRate = totalInvites > 0 ? (decimal)acceptedInvites / totalInvites * 100 : 0;

        return new InviteStatsDto
        {
            TotalInvites = totalInvites,
            PendingInvites = pendingInvites,
            AcceptedInvites = acceptedInvites,
            ExpiredInvites = expiredInvites,
            ConversionRate = conversionRate
        };
    }

    public async Task<InviteLinkDto> CreateInviteLinkAsync(string userId, int? maxUsage = null, DateTime? expiresAt = null)
    {
        var token = GenerateInviteToken();
        var invite = new UserInvite
        {
            InviterId = userId,
            InviteeEmail = "direct-link", // Special marker for direct links
            InviteToken = token,
            Type = InviteType.DirectLink,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(30)
        };

        _context.UserInvites.Add(invite);
        await _context.SaveChangesAsync();

        return new InviteLinkDto
        {
            InviteLink = $"https://yourdomain.com/invite/{token}",
            InviteToken = token,
            ExpiresAt = invite.ExpiresAt ?? DateTime.UtcNow.AddDays(30),
            UsageCount = 0,
            MaxUsage = maxUsage ?? int.MaxValue
        };
    }

    public async Task<bool> ValidateInviteTokenAsync(string token)
    {
        var invite = await _context.UserInvites
            .FirstOrDefaultAsync(i => i.InviteToken == token);

        if (invite == null)
            return false;

        if (invite.Status != InviteStatus.Pending)
            return false;

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
        {
            invite.Status = InviteStatus.Expired;
            await _context.SaveChangesAsync();
            return false;
        }

        return true;
    }

    public async Task<List<UserInviteDto>> BulkCreateInvitesAsync(string inviterId, BulkInviteDto bulkDto)
    {
        var invites = new List<UserInvite>();
        var results = new List<UserInviteDto>();

        foreach (var createDto in bulkDto.Invites)
        {
            try
            {
                var invite = await CreateInviteAsync(inviterId, createDto);
                results.Add(invite);
            }
            catch (Exception ex)
            {
            }
        }

        return results;
    }

    public async Task<bool> ExpireInviteAsync(int inviteId)
    {
        var invite = await _context.UserInvites.FindAsync(inviteId);
        if (invite == null)
            return false;

        invite.Status = InviteStatus.Expired;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<UserInviteDto> GetInviteDtoAsync(int inviteId)
    {
        var invite = await _context.UserInvites
            .Include(i => i.Inviter)
            .Include(i => i.AcceptedUser)
            .FirstOrDefaultAsync(i => i.Id == inviteId);

        return _mapper.Map<UserInviteDto>(invite!);
    }

    private async Task SendInviteEmailAsync(UserInvite invite)
    {
        var inviter = await _context.Users.FindAsync(invite.InviterId);
        if (inviter == null) return;

        var inviterName = $"{inviter.FirstName} {inviter.LastName}";
        var inviteLink = $"https://yourdomain.com/invite/{invite.InviteToken}";
        
        var subject = $"{inviterName} invited you to join Drashyam";
        var body = _templateService.GetInviteEmailTemplate(
            inviterName, 
            invite.PersonalMessage ?? "", 
            inviteLink, 
            invite.ExpiresAt ?? DateTime.UtcNow.AddDays(7)
        );

        await _emailService.SendEmailAsync(invite.InviteeEmail, subject, body);
    }

    private async Task CreateReferralFromInviteAsync(UserInvite invite, string acceptedUserId)
    {
        var referral = new Referral
        {
            ReferrerId = invite.InviterId,
            ReferredUserId = acceptedUserId,
            Status = ReferralStatus.Completed,
            ReferralCode = invite.InviteToken
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        // Process referral reward
        await ProcessReferralRewardAsync(referral.Id);
    }

    private async Task ProcessReferralRewardAsync(int referralId)
    {
        var referral = await _context.Referrals
            .Include(r => r.Referrer)
            .FirstOrDefaultAsync(r => r.Id == referralId);

        if (referral == null) return;

        // Create reward for referrer
        var reward = new ReferralReward
        {
            UserId = referral.ReferrerId,
            ReferralId = referralId,
            Amount = 10.00m, // Default reward amount
            RewardType = "Points",
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        _context.ReferralRewards.Add(reward);
        await _context.SaveChangesAsync();
    }

    private string GenerateInviteToken()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}
