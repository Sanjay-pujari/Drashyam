using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<AnalyticsService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task TrackInviteEventAsync(string userId, InviteEventType eventType, int? inviteId = null, string? details = null, string? source = null)
    {
        try
        {
            var inviteEvent = new InviteEvent
            {
                UserId = userId,
                InviteId = inviteId,
                EventType = eventType,
                Details = details,
                Source = source,
                Timestamp = DateTime.UtcNow
            };

            _context.InviteEvents.Add(inviteEvent);
            await _context.SaveChangesAsync();

            // Update daily analytics
            await UpdateInviteAnalyticsAsync(userId, DateTime.UtcNow.Date);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task TrackReferralEventAsync(string userId, ReferralEventType eventType, int? referralId = null, string? details = null, string? source = null)
    {
        try
        {
            var referralEvent = new ReferralEvent
            {
                UserId = userId,
                ReferralId = referralId,
                EventType = eventType,
                Details = details,
                Source = source,
                Timestamp = DateTime.UtcNow
            };

            _context.ReferralEvents.Add(referralEvent);
            await _context.SaveChangesAsync();

            // Update daily analytics
            await UpdateReferralAnalyticsAsync(userId, DateTime.UtcNow.Date);
        }
        catch (Exception ex)
        {
        }
    }

    public async Task UpdateInviteAnalyticsAsync(string userId, DateTime date)
    {
        try
        {
            var analytics = await _context.InviteAnalytics
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date);

            if (analytics == null)
            {
                analytics = new InviteAnalytics
                {
                    UserId = userId,
                    Date = date
                };
                _context.InviteAnalytics.Add(analytics);
            }

            // Calculate metrics from events
            var events = await _context.InviteEvents
                .Where(e => e.UserId == userId && e.Timestamp.Date == date)
                .ToListAsync();

            analytics.InvitesSent = events.Count(e => e.EventType == InviteEventType.Created);
            analytics.InvitesAccepted = events.Count(e => e.EventType == InviteEventType.Accepted);
            analytics.InvitesExpired = events.Count(e => e.EventType == InviteEventType.Expired);
            analytics.InvitesCancelled = events.Count(e => e.EventType == InviteEventType.Cancelled);
            analytics.EmailInvites = events.Count(e => e.EventType == InviteEventType.Created && e.Source == "Email");
            analytics.SocialInvites = events.Count(e => e.EventType == InviteEventType.Created && e.Source == "Social");
            analytics.DirectLinkInvites = events.Count(e => e.EventType == InviteEventType.Created && e.Source == "DirectLink");
            analytics.BulkInvites = events.Count(e => e.EventType == InviteEventType.Created && e.Source == "Bulk");
            analytics.Resends = events.Count(e => e.EventType == InviteEventType.Resent);

            // Calculate conversion rate
            analytics.ConversionRate = analytics.InvitesSent > 0 
                ? (decimal)analytics.InvitesAccepted / analytics.InvitesSent * 100 
                : 0;

            // Calculate average time to accept
            var acceptedEvents = events.Where(e => e.EventType == InviteEventType.Accepted).ToList();
            if (acceptedEvents.Any())
            {
                var createdEvents = events.Where(e => e.EventType == InviteEventType.Created).ToList();
                var timeSpans = new List<TimeSpan>();

                foreach (var accepted in acceptedEvents)
                {
                    var created = createdEvents.FirstOrDefault(c => c.InviteId == accepted.InviteId);
                    if (created != null)
                    {
                        timeSpans.Add(accepted.Timestamp - created.Timestamp);
                    }
                }

                if (timeSpans.Any())
                {
                    analytics.AverageTimeToAccept = TimeSpan.FromTicks((long)timeSpans.Average(t => t.Ticks));
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
        }
    }

    public async Task UpdateReferralAnalyticsAsync(string userId, DateTime date)
    {
        try
        {
            var analytics = await _context.ReferralAnalytics
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date);

            if (analytics == null)
            {
                analytics = new ReferralAnalytics
                {
                    UserId = userId,
                    Date = date
                };
                _context.ReferralAnalytics.Add(analytics);
            }

            // Calculate metrics from events
            var events = await _context.ReferralEvents
                .Where(e => e.UserId == userId && e.Timestamp.Date == date)
                .ToListAsync();

            analytics.ReferralsCreated = events.Count(e => e.EventType == ReferralEventType.Created);
            analytics.ReferralsCompleted = events.Count(e => e.EventType == ReferralEventType.Completed);
            analytics.ReferralsRewarded = events.Count(e => e.EventType == ReferralEventType.Rewarded);
            analytics.ReferralCodesGenerated = events.Count(e => e.EventType == ReferralEventType.CodeGenerated);
            analytics.ReferralCodesUsed = events.Count(e => e.EventType == ReferralEventType.CodeUsed);

            // Calculate conversion rate
            analytics.ConversionRate = analytics.ReferralsCreated > 0 
                ? (decimal)analytics.ReferralsCompleted / analytics.ReferralsCreated * 100 
                : 0;

            // Calculate total rewards
            var rewards = await _context.ReferralRewards
                .Where(r => r.UserId == userId && r.CreatedAt.Date == date)
                .ToListAsync();

            analytics.TotalRewardsEarned = rewards.Sum(r => r.Amount);
            analytics.TotalRewardsClaimed = rewards.Where(r => r.Status == RewardStatus.Claimed).Sum(r => r.Amount);
            analytics.AverageRewardAmount = rewards.Any() ? rewards.Average(r => r.Amount) : 0;

            // Calculate average time to complete
            var completedEvents = events.Where(e => e.EventType == ReferralEventType.Completed).ToList();
            if (completedEvents.Any())
            {
                var createdEvents = events.Where(e => e.EventType == ReferralEventType.Created).ToList();
                var timeSpans = new List<TimeSpan>();

                foreach (var completed in completedEvents)
                {
                    var created = createdEvents.FirstOrDefault(c => c.ReferralId == completed.ReferralId);
                    if (created != null)
                    {
                        timeSpans.Add(completed.Timestamp - created.Timestamp);
                    }
                }

                if (timeSpans.Any())
                {
                    analytics.AverageTimeToComplete = TimeSpan.FromTicks((long)timeSpans.Average(t => t.Ticks));
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
        }
    }

    public async Task<InviteAnalyticsDto> GetInviteAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var analytics = await _context.InviteAnalytics
            .Where(a => a.UserId == userId && a.Date >= start && a.Date <= end)
            .OrderBy(a => a.Date)
            .ToListAsync();

        var summary = new InviteAnalyticsDto
        {
            UserId = userId,
            Date = start,
            InvitesSent = analytics.Sum(a => a.InvitesSent),
            InvitesAccepted = analytics.Sum(a => a.InvitesAccepted),
            InvitesExpired = analytics.Sum(a => a.InvitesExpired),
            InvitesCancelled = analytics.Sum(a => a.InvitesCancelled),
            EmailInvites = analytics.Sum(a => a.EmailInvites),
            SocialInvites = analytics.Sum(a => a.SocialInvites),
            DirectLinkInvites = analytics.Sum(a => a.DirectLinkInvites),
            BulkInvites = analytics.Sum(a => a.BulkInvites),
            Resends = analytics.Sum(a => a.Resends)
        };

        summary.ConversionRate = summary.InvitesSent > 0 
            ? (decimal)summary.InvitesAccepted / summary.InvitesSent * 100 
            : 0;

        return summary;
    }

    public async Task<ReferralAnalyticsDto> GetReferralAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var analytics = await _context.ReferralAnalytics
            .Where(a => a.UserId == userId && a.Date >= start && a.Date <= end)
            .OrderBy(a => a.Date)
            .ToListAsync();

        var summary = new ReferralAnalyticsDto
        {
            UserId = userId,
            Date = start,
            ReferralsCreated = analytics.Sum(a => a.ReferralsCreated),
            ReferralsCompleted = analytics.Sum(a => a.ReferralsCompleted),
            ReferralsRewarded = analytics.Sum(a => a.ReferralsRewarded),
            ReferralCodesGenerated = analytics.Sum(a => a.ReferralCodesGenerated),
            ReferralCodesUsed = analytics.Sum(a => a.ReferralCodesUsed),
            TotalRewardsEarned = analytics.Sum(a => a.TotalRewardsEarned),
            TotalRewardsClaimed = analytics.Sum(a => a.TotalRewardsClaimed)
        };

        summary.ConversionRate = summary.ReferralsCreated > 0 
            ? (decimal)summary.ReferralsCompleted / summary.ReferralsCreated * 100 
            : 0;

        summary.AverageRewardAmount = summary.ReferralsRewarded > 0 
            ? summary.TotalRewardsEarned / summary.ReferralsRewarded 
            : 0;

        return summary;
    }

    public async Task<InviteAnalyticsDto> GetInviteAnalyticsSummaryAsync(string userId)
    {
        return await GetInviteAnalyticsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
    }

    public async Task<ReferralAnalyticsDto> GetReferralAnalyticsSummaryAsync(string userId)
    {
        return await GetReferralAnalyticsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
    }

    public async Task<List<InviteEventDto>> GetInviteEventsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var events = await _context.InviteEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<InviteEventDto>>(events);
    }

    public async Task<List<ReferralEventDto>> GetReferralEventsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var events = await _context.ReferralEvents
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<ReferralEventDto>>(events);
    }

    // Video Analytics Methods (stub implementations)
    public async Task<AnalyticsDto> GetVideoAnalyticsAsync(int videoId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement video analytics
        return new AnalyticsDto
        {
            VideoId = videoId,
            Title = "Sample Video",
            Views = 0,
            Likes = 0,
            Comments = 0,
            Shares = 0,
            Revenue = 0,
            WatchTime = TimeSpan.Zero,
            EngagementRate = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<AnalyticsDto> GetChannelAnalyticsAsync(int channelId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement channel analytics
        return new AnalyticsDto
        {
            VideoId = 0,
            Title = "Channel Analytics",
            Views = 0,
            Likes = 0,
            Comments = 0,
            Shares = 0,
            Revenue = 0,
            WatchTime = TimeSpan.Zero,
            EngagementRate = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<RevenueDto> GetRevenueAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement revenue analytics
        return new RevenueDto
        {
            TotalRevenue = 0,
            AdRevenue = 0,
            SubscriptionRevenue = 0,
            DonationRevenue = 0,
            Period = DateTime.UtcNow
        };
    }

    public async Task<List<TopVideoDto>> GetTopVideosAsync(string userId, int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement top videos analytics
        return new List<TopVideoDto>();
    }

    public async Task<List<AudienceInsightDto>> GetAudienceInsightsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement audience insights
        return new List<AudienceInsightDto>();
    }

    public async Task<List<GeographicDataDto>> GetGeographicDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement geographic data
        return new List<GeographicDataDto>();
    }

    public async Task<List<DeviceDataDto>> GetDeviceDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement device data
        return new List<DeviceDataDto>();
    }

    public async Task<EngagementMetricsDto> GetEngagementMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement engagement metrics
        return new EngagementMetricsDto
        {
            LikeRate = 0,
            CommentRate = 0,
            ShareRate = 0,
            WatchTimeRate = 0,
            ClickThroughRate = 0
        };
    }

    public async Task<AnalyticsDto> GetUserAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // TODO: Implement user analytics
        return new AnalyticsDto
        {
            VideoId = 0,
            Title = "User Analytics",
            Views = 0,
            Likes = 0,
            Comments = 0,
            Shares = 0,
            Revenue = 0,
            WatchTime = TimeSpan.Zero,
            EngagementRate = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}