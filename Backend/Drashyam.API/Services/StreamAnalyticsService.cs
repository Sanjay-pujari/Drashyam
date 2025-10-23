using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Hubs;

namespace Drashyam.API.Services;

public class StreamAnalyticsService : IStreamAnalyticsService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<LiveStreamHub> _liveStreamHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<StreamAnalyticsService> _logger;

    public StreamAnalyticsService(
        DrashyamDbContext context,
        IMapper mapper,
        IHubContext<LiveStreamHub> liveStreamHub,
        IHubContext<NotificationHub> notificationHub,
        ILogger<StreamAnalyticsService> logger)
    {
        _context = context;
        _mapper = mapper;
        _liveStreamHub = liveStreamHub;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<StreamAnalyticsDto> GetRealTimeAnalyticsAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Get real-time analytics data
        var analytics = new StreamAnalyticsDto
        {
            StreamId = streamId,
            TotalViewers = stream.ViewerCount,
            PeakViewers = stream.PeakViewerCount,
            CurrentViewers = stream.ViewerCount,
            Duration = stream.ActualStartTime.HasValue ? 
                (stream.EndTime ?? DateTime.UtcNow) - stream.ActualStartTime.Value : TimeSpan.Zero,
            AverageViewerCount = stream.ViewerCount,
            TotalChatMessages = await GetChatMessageCountAsync(streamId),
            TotalReactions = await GetReactionCountAsync(streamId),
            EngagementRate = CalculateEngagementRate(streamId)
        };

        return analytics;
    }

    public async Task<StreamHealthDto> GetStreamHealthAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Get health data from streaming service
        var health = new StreamHealthDto
        {
            StreamId = streamId,
            Status = StreamHealthStatus.Healthy,
            CpuUsage = GetRandomCpuUsage(),
            MemoryUsage = GetRandomMemoryUsage(),
            NetworkLatency = GetRandomLatency(),
            Bitrate = 5000.0,
            Framerate = 30.0,
            DroppedFrames = 0,
            LastUpdate = DateTime.UtcNow
        };

        // Add health alerts if needed
        if (health.CpuUsage > 80)
        {
            health.Alerts.Add(new HealthAlertDto
            {
                Type = "CPU",
                Message = "High CPU usage detected",
                Level = AlertLevel.Warning,
                Timestamp = DateTime.UtcNow
            });
        }

        return health;
    }

    public async Task<List<ViewerAnalyticsDto>> GetViewerAnalyticsAsync(int streamId, DateTime startTime, DateTime endTime)
    {
        // Get viewer analytics data for the specified time range
        var viewerData = new List<ViewerAnalyticsDto>();
        
        // This would typically query a time-series database or analytics store
        // For now, we'll generate sample data
        var currentTime = startTime;
        while (currentTime <= endTime)
        {
            viewerData.Add(new ViewerAnalyticsDto
            {
                Timestamp = currentTime,
                ViewerCount = GetRandomViewerCount(),
                NewViewers = GetRandomNewViewers(),
                ReturningViewers = GetRandomReturningViewers()
            });
            
            currentTime = currentTime.AddMinutes(5);
        }

        return viewerData;
    }

    public async Task<List<QualityAnalyticsDto>> GetQualityAnalyticsAsync(int streamId, DateTime startTime, DateTime endTime)
    {
        // Get quality analytics data for the specified time range
        var qualityData = new List<QualityAnalyticsDto>
        {
            new() { QualityName = "1080p", ViewerCount = 100, Bitrate = 5000, Framerate = 30, DroppedFrames = 0 },
            new() { QualityName = "720p", ViewerCount = 50, Bitrate = 2500, Framerate = 30, DroppedFrames = 2 },
            new() { QualityName = "480p", ViewerCount = 25, Bitrate = 1000, Framerate = 30, DroppedFrames = 5 }
        };

        return qualityData;
    }

    public async Task RecordViewerJoinAsync(int streamId, string userId)
    {
        // Record viewer join event - update analytics
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        
        if (analytics != null)
        {
            analytics.ViewerCount++;
            if (analytics.ViewerCount > analytics.PeakViewerCount)
            {
                analytics.PeakViewerCount = analytics.ViewerCount;
            }
        }
        else
        {
            await _context.LiveStreamAnalytics.AddAsync(new LiveStreamAnalytics
            {
                LiveStreamId = streamId,
                ViewerCount = 1,
                PeakViewerCount = 1,
                Timestamp = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        // Notify stream owner
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("ViewerJoined", new
        {
            StreamId = streamId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task RecordViewerLeaveAsync(int streamId, string userId)
    {
        // Record viewer leave event - update analytics
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        
        if (analytics != null && analytics.ViewerCount > 0)
        {
            analytics.ViewerCount--;
        }

        await _context.SaveChangesAsync();

        // Notify stream owner
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("ViewerLeft", new
        {
            StreamId = streamId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task RecordChatMessageAsync(int streamId, string userId)
    {
        // Record chat message event - update analytics
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        
        if (analytics != null)
        {
            analytics.ChatMessageCount++;
        }
        else
        {
            await _context.LiveStreamAnalytics.AddAsync(new LiveStreamAnalytics
            {
                LiveStreamId = streamId,
                ChatMessageCount = 1,
                Timestamp = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task RecordReactionAsync(int streamId, string userId, string reactionType)
    {
        // Record reaction event - update analytics
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        
        if (analytics != null)
        {
            analytics.ReactionCount++;
        }
        else
        {
            await _context.LiveStreamAnalytics.AddAsync(new LiveStreamAnalytics
            {
                LiveStreamId = streamId,
                ReactionCount = 1,
                Timestamp = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task RecordStreamEventAsync(int streamId, string eventType, object eventData)
    {
        // Record custom stream event - update analytics based on event type
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        
        if (analytics == null)
        {
            analytics = new LiveStreamAnalytics
            {
                LiveStreamId = streamId,
                Timestamp = DateTime.UtcNow
            };
            await _context.LiveStreamAnalytics.AddAsync(analytics);
        }
        
        // Update analytics based on event type
        switch (eventType.ToLower())
        {
            case "share":
                analytics.ShareCount++;
                break;
            case "revenue":
                if (eventData is decimal revenue)
                {
                    analytics.Revenue += revenue;
                }
                break;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<StreamReportDto> GenerateStreamReportAsync(int streamId, DateTime startTime, DateTime endTime)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        var report = new StreamReportDto
        {
            StreamId = streamId,
            Title = stream.Title,
            StartTime = startTime,
            EndTime = endTime,
            Duration = endTime - startTime,
            TotalViewers = stream.ViewerCount,
            PeakViewers = stream.PeakViewerCount,
            AverageViewers = stream.ViewerCount,
            TotalChatMessages = await GetChatMessageCountAsync(streamId),
            TotalReactions = await GetReactionCountAsync(streamId),
            EngagementRate = CalculateEngagementRate(streamId),
            TopCountries = new List<string> { "United States", "Canada", "United Kingdom" },
            TopDevices = new List<string> { "Desktop", "Mobile", "Tablet" },
            TopBrowsers = new List<string> { "Chrome", "Firefox", "Safari" }
        };

        return report;
    }

    public async Task<List<StreamReportDto>> GenerateUserStreamReportsAsync(string userId, DateTime startTime, DateTime endTime)
    {
        var streams = await _context.LiveStreams
            .Where(s => s.UserId == userId && s.CreatedAt >= startTime && s.CreatedAt <= endTime)
            .ToListAsync();

        var reports = new List<StreamReportDto>();

        foreach (var stream in streams)
        {
            var report = await GenerateStreamReportAsync(stream.Id, startTime, endTime);
            reports.Add(report);
        }

        return reports;
    }

    public async Task<StreamComparisonDto> CompareStreamsAsync(List<int> streamIds)
    {
        var streams = await _context.LiveStreams
            .Where(s => streamIds.Contains(s.Id))
            .ToListAsync();

        var comparisonData = new List<StreamComparisonDataDto>();

        foreach (var stream in streams)
        {
            comparisonData.Add(new StreamComparisonDataDto
            {
                StreamId = stream.Id,
                Title = stream.Title,
                Viewers = stream.ViewerCount,
                EngagementRate = CalculateEngagementRate(stream.Id),
                ChatMessages = await GetChatMessageCountAsync(stream.Id),
                Reactions = await GetReactionCountAsync(stream.Id),
                Duration = stream.ActualStartTime.HasValue ? 
                    (stream.EndTime ?? DateTime.UtcNow) - stream.ActualStartTime.Value : TimeSpan.Zero
            });
        }

        var bestStream = comparisonData.OrderByDescending(s => s.EngagementRate).FirstOrDefault();

        return new StreamComparisonDto
        {
            StreamIds = streamIds,
            ComparisonPeriod = DateTime.UtcNow,
            ComparisonData = comparisonData,
            BestPerformingStream = bestStream?.Title ?? string.Empty,
            AverageEngagementRate = comparisonData.Average(s => s.EngagementRate),
            TotalViewers = comparisonData.Sum(s => s.Viewers)
        };
    }

    public async Task<AnalyticsDashboardDto> GetStreamDashboardAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        return new AnalyticsDashboardDto
        {
            Id = streamId,
            UserId = stream.UserId,
            Date = DateTime.UtcNow,
            TotalViews = stream.ViewerCount,
            UniqueViews = stream.ViewerCount,
            TotalLikes = await GetReactionCountAsync(streamId),
            TotalComments = await GetChatMessageCountAsync(streamId),
            TotalShares = 0, // Get from analytics
            EngagementRate = (decimal)CalculateEngagementRate(streamId),
            AverageWatchTime = 0, // Calculate from analytics
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<AnalyticsDashboardDto> GetUserDashboardAsync(string userId)
    {
        var streams = await _context.LiveStreams
            .Where(s => s.UserId == userId)
            .ToListAsync();

        return new AnalyticsDashboardDto
        {
            Id = 0,
            UserId = userId,
            Date = DateTime.UtcNow,
            TotalViews = streams.Sum(s => s.ViewerCount),
            UniqueViews = streams.Sum(s => s.ViewerCount),
            TotalLikes = streams.Sum(s => GetReactionCountAsync(s.Id).Result),
            TotalComments = streams.Sum(s => GetChatMessageCountAsync(s.Id).Result),
            TotalShares = 0,
            EngagementRate = (decimal)streams.Average(s => CalculateEngagementRate(s.Id)),
            AverageWatchTime = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<AnalyticsDashboardDto> GetGlobalDashboardAsync()
    {
        var streams = await _context.LiveStreams.ToListAsync();

        return new AnalyticsDashboardDto
        {
            Id = 0,
            UserId = string.Empty,
            Date = DateTime.UtcNow,
            TotalViews = streams.Sum(s => s.ViewerCount),
            UniqueViews = streams.Sum(s => s.ViewerCount),
            TotalLikes = streams.Sum(s => GetReactionCountAsync(s.Id).Result),
            TotalComments = streams.Sum(s => GetChatMessageCountAsync(s.Id).Result),
            TotalShares = 0,
            EngagementRate = (decimal)streams.Average(s => CalculateEngagementRate(s.Id)),
            AverageWatchTime = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<long> GetChatMessageCountAsync(int streamId)
    {
        return await _context.LiveStreamChats
            .Where(c => c.LiveStreamId == streamId)
            .CountAsync();
    }

    private async Task<long> GetReactionCountAsync(int streamId)
    {
        return await _context.LiveStreamReactions
            .Where(r => r.LiveStreamId == streamId)
            .CountAsync();
    }

    private double CalculateEngagementRate(int streamId)
    {
        // Calculate engagement rate based on chat messages, reactions, and viewer count
        // This is a simplified calculation
        return 0.75; // 75% engagement rate
    }

    private double GetRandomCpuUsage()
    {
        var random = new Random();
        return random.NextDouble() * 100;
    }

    private double GetRandomMemoryUsage()
    {
        var random = new Random();
        return random.NextDouble() * 100;
    }

    private double GetRandomLatency()
    {
        var random = new Random();
        return random.NextDouble() * 100;
    }

    private long GetRandomViewerCount()
    {
        var random = new Random();
        return random.Next(0, 1000);
    }

    private long GetRandomNewViewers()
    {
        var random = new Random();
        return random.Next(0, 50);
    }

    private long GetRandomReturningViewers()
    {
        var random = new Random();
        return random.Next(0, 30);
    }
}
