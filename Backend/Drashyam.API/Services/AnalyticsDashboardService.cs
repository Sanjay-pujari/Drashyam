using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class AnalyticsDashboardService : IAnalyticsDashboardService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AnalyticsDashboardService> _logger;

    public AnalyticsDashboardService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<AnalyticsDashboardService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AnalyticsDashboards.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var analytics = await query.ToListAsync();

            var summary = new AnalyticsSummaryDto
            {
                TotalViews = analytics.Sum(a => a.TotalViews),
                TotalLikes = analytics.Sum(a => a.TotalLikes),
                TotalComments = analytics.Sum(a => a.TotalComments),
                TotalShares = analytics.Sum(a => a.TotalShares),
                TotalSubscribers = analytics.Sum(a => a.TotalSubscribers),
                TotalRevenue = analytics.Sum(a => a.TotalRevenue),
                AverageWatchTime = analytics.Any() ? analytics.Average(a => a.AverageWatchTime) : 0,
                EngagementRate = analytics.Any() ? analytics.Average(a => a.EngagementRate) : 0,
                RevenueGrowth = CalculateGrowthRate(analytics.Select(a => a.TotalRevenue).ToList()),
                SubscriberGrowth = CalculateGrowthRate(analytics.Select(a => (decimal)a.TotalSubscribers).ToList())
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics summary for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<TimeSeriesAnalyticsDto>> GetTimeSeriesDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AnalyticsDashboards.Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var analytics = await query
                .OrderBy(a => a.Date)
                .ToListAsync();

            return analytics.Select(a => new TimeSeriesAnalyticsDto
            {
                Date = a.Date,
                Views = a.TotalViews,
                Likes = a.TotalLikes,
                Comments = a.TotalComments,
                Shares = a.TotalShares,
                Revenue = a.TotalRevenue,
                EngagementRate = a.EngagementRate
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time series data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<TopVideoAnalyticsDto>> GetTopVideosAsync(string userId, int count = 10, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.VideoAnalytics
                .Include(va => va.Video)
                .Where(va => va.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(va => va.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(va => va.Date <= endDate.Value);

            var topVideos = await query
                .GroupBy(va => va.VideoId)
                .Select(g => new TopVideoAnalyticsDto
                {
                    VideoId = g.Key,
                    Title = g.First().Video.Title,
                    ThumbnailUrl = g.First().Video.ThumbnailUrl ?? "",
                    Views = g.Sum(va => va.Views),
                    Likes = g.Sum(va => va.Likes),
                    Comments = g.Sum(va => va.Comments),
                    Revenue = g.Sum(va => va.Revenue),
                    EngagementRate = g.Average(va => va.EngagementRate),
                    CreatedAt = g.First().Video.CreatedAt
                })
                .OrderByDescending(v => v.Views)
                .Take(count)
                .ToListAsync();

            return topVideos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top videos for user {UserId}", userId);
            throw;
        }
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.RevenueAnalytics.Where(ra => ra.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ra => ra.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ra => ra.Date <= endDate.Value);

            var revenueData = await query.ToListAsync();

            var revenue = new RevenueAnalyticsDto
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                TotalRevenue = revenueData.Sum(r => r.TotalRevenue),
                AdRevenue = revenueData.Sum(r => r.AdRevenue),
                SubscriptionRevenue = revenueData.Sum(r => r.SubscriptionRevenue),
                PremiumContentRevenue = revenueData.Sum(r => r.PremiumContentRevenue),
                MerchandiseRevenue = revenueData.Sum(r => r.MerchandiseRevenue),
                DonationRevenue = revenueData.Sum(r => r.DonationRevenue),
                ReferralRevenue = revenueData.Sum(r => r.ReferralRevenue),
                RevenuePerView = CalculateRevenuePerView(revenueData),
                RevenuePerSubscriber = CalculateRevenuePerSubscriber(revenueData),
                RevenueGrowthRate = CalculateGrowthRate(revenueData.Select(r => r.TotalRevenue).ToList())
            };

            return revenue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<TimeSeriesAnalyticsDto>> GetRevenueTimeSeriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.RevenueAnalytics.Where(ra => ra.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ra => ra.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ra => ra.Date <= endDate.Value);

            var revenueData = await query
                .OrderBy(ra => ra.Date)
                .ToListAsync();

            return revenueData.Select(r => new TimeSeriesAnalyticsDto
            {
                Date = r.Date,
                Revenue = r.TotalRevenue,
                Views = 0, // Revenue data doesn't include views
                Likes = 0,
                Comments = 0,
                Shares = 0,
                EngagementRate = 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue time series for user {UserId}", userId);
            throw;
        }
    }

    public async Task<decimal> GetTotalRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.RevenueAnalytics.Where(ra => ra.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ra => ra.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ra => ra.Date <= endDate.Value);

            return await query.SumAsync(ra => ra.TotalRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total revenue for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<GeographicAnalyticsDto>> GetGeographicDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AudienceAnalytics.Where(aa => aa.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(aa => aa.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(aa => aa.Date <= endDate.Value);

            var geographicData = await query
                .Where(aa => !string.IsNullOrEmpty(aa.Country))
                .GroupBy(aa => aa.Country)
                .Select(g => new
                {
                    Country = g.Key ?? "Unknown",
                    Views = g.Sum(aa => aa.ViewCount),
                    Revenue = g.Sum(aa => aa.Revenue)
                })
                .OrderByDescending(g => g.Views)
                .ToListAsync();

            var result = geographicData.Select(g => new GeographicAnalyticsDto
            {
                Country = g.Country,
                CountryCode = GetCountryCode(g.Country),
                Views = g.Views,
                Revenue = g.Revenue,
                Subscribers = 0, // Would need separate query for subscribers
                Percentage = 0 // Will be calculated after total is known
            }).ToList();

            // If no data found, return sample data for development
            if (!result.Any())
            {
                return new List<GeographicAnalyticsDto>
                {
                    new GeographicAnalyticsDto
                    {
                        Country = "United States",
                        CountryCode = "US",
                        Views = 1000,
                        Revenue = 50.00m,
                        Subscribers = 25,
                        Percentage = 40.0m
                    },
                    new GeographicAnalyticsDto
                    {
                        Country = "United Kingdom",
                        CountryCode = "GB",
                        Views = 600,
                        Revenue = 30.00m,
                        Subscribers = 15,
                        Percentage = 24.0m
                    },
                    new GeographicAnalyticsDto
                    {
                        Country = "Canada",
                        CountryCode = "CA",
                        Views = 400,
                        Revenue = 20.00m,
                        Subscribers = 10,
                        Percentage = 16.0m
                    },
                    new GeographicAnalyticsDto
                    {
                        Country = "Australia",
                        CountryCode = "AU",
                        Views = 300,
                        Revenue = 15.00m,
                        Subscribers = 8,
                        Percentage = 12.0m
                    },
                    new GeographicAnalyticsDto
                    {
                        Country = "Germany",
                        CountryCode = "DE",
                        Views = 200,
                        Revenue = 10.00m,
                        Subscribers = 5,
                        Percentage = 8.0m
                    }
                };
            }

            var totalViews = result.Sum(g => g.Views);
            foreach (var item in result)
            {
                item.Percentage = totalViews > 0 ? (decimal)item.Views / totalViews * 100 : 0;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting geographic data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<DeviceAnalyticsDto>> GetDeviceDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AudienceAnalytics.Where(aa => aa.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(aa => aa.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(aa => aa.Date <= endDate.Value);

            var deviceData = await query
                .Where(aa => !string.IsNullOrEmpty(aa.DeviceType))
                .GroupBy(aa => aa.DeviceType)
                .Select(g => new DeviceAnalyticsDto
                {
                    DeviceType = g.Key ?? "Unknown",
                    Views = g.Sum(aa => aa.ViewCount),
                    AverageWatchTime = g.Average(aa => aa.WatchTime),
                    EngagementRate = g.Average(aa => aa.EngagementScore),
                    Percentage = 0 // Will be calculated after total is known
                })
                .OrderByDescending(d => d.Views)
                .ToListAsync();

            // If no data found, return sample data for development
            if (!deviceData.Any())
            {
                return new List<DeviceAnalyticsDto>
                {
                    new DeviceAnalyticsDto
                    {
                        DeviceType = "mobile",
                        Views = 1500,
                        AverageWatchTime = 120.5m,
                        EngagementRate = 8.5m,
                        Percentage = 60.0m
                    },
                    new DeviceAnalyticsDto
                    {
                        DeviceType = "desktop",
                        Views = 800,
                        AverageWatchTime = 180.2m,
                        EngagementRate = 12.3m,
                        Percentage = 32.0m
                    },
                    new DeviceAnalyticsDto
                    {
                        DeviceType = "tablet",
                        Views = 200,
                        AverageWatchTime = 95.8m,
                        EngagementRate = 6.7m,
                        Percentage = 8.0m
                    }
                };
            }

            var totalViews = deviceData.Sum(d => d.Views);
            foreach (var item in deviceData)
            {
                item.Percentage = totalViews > 0 ? (decimal)item.Views / totalViews * 100 : 0;
            }

            return deviceData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ReferrerAnalyticsDto>> GetReferrerDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AudienceAnalytics.Where(aa => aa.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(aa => aa.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(aa => aa.Date <= endDate.Value);

            var referrerData = await query
                .Where(aa => !string.IsNullOrEmpty(aa.Referrer))
                .GroupBy(aa => aa.Referrer)
                .Select(g => new ReferrerAnalyticsDto
                {
                    Referrer = g.Key ?? "Unknown",
                    Views = g.Sum(aa => aa.ViewCount),
                    ConversionRate = g.Average(aa => aa.EngagementScore),
                    Percentage = 0 // Will be calculated after total is known
                })
                .OrderByDescending(r => r.Views)
                .ToListAsync();

            var totalViews = referrerData.Sum(r => r.Views);
            foreach (var item in referrerData)
            {
                item.Percentage = totalViews > 0 ? (decimal)item.Views / totalViews * 100 : 0;
            }

            return referrerData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting referrer data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AudienceAnalyticsDto>> GetAudienceInsightsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AudienceAnalytics.Where(aa => aa.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(aa => aa.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(aa => aa.Date <= endDate.Value);

            var audienceData = await query
                .OrderByDescending(aa => aa.ViewCount)
                .ToListAsync();

            return _mapper.Map<List<AudienceAnalyticsDto>>(audienceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audience insights for user {UserId}", userId);
            throw;
        }
    }

    public async Task<EngagementAnalyticsDto> GetEngagementMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.EngagementAnalytics.Where(ea => ea.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ea => ea.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ea => ea.Date <= endDate.Value);

            var engagementData = await query.ToListAsync();

            var engagement = new EngagementAnalyticsDto
            {
                UserId = userId,
                Date = DateTime.UtcNow,
                LikeRate = engagementData.Any() ? engagementData.Average(e => e.LikeRate) : 0,
                CommentRate = engagementData.Any() ? engagementData.Average(e => e.CommentRate) : 0,
                ShareRate = engagementData.Any() ? engagementData.Average(e => e.ShareRate) : 0,
                WatchTimeRate = engagementData.Any() ? engagementData.Average(e => e.WatchTimeRate) : 0,
                ClickThroughRate = engagementData.Any() ? engagementData.Average(e => e.ClickThroughRate) : 0,
                RetentionRate = engagementData.Any() ? engagementData.Average(e => e.RetentionRate) : 0,
                TotalLikes = engagementData.Sum(e => e.TotalLikes),
                TotalComments = engagementData.Sum(e => e.TotalComments),
                TotalShares = engagementData.Sum(e => e.TotalShares),
                TotalViews = engagementData.Sum(e => e.TotalViews)
            };

            return engagement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement metrics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<TimeSeriesAnalyticsDto>> GetEngagementTimeSeriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.EngagementAnalytics.Where(ea => ea.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ea => ea.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ea => ea.Date <= endDate.Value);

            var engagementData = await query
                .OrderBy(ea => ea.Date)
                .ToListAsync();

            return engagementData.Select(e => new TimeSeriesAnalyticsDto
            {
                Date = e.Date,
                Views = e.TotalViews,
                Likes = e.TotalLikes,
                Comments = e.TotalComments,
                Shares = e.TotalShares,
                Revenue = 0, // Engagement data doesn't include revenue
                EngagementRate = (e.LikeRate + e.CommentRate + e.ShareRate) / 3
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement time series for user {UserId}", userId);
            throw;
        }
    }

    public async Task<VideoAnalyticsDto> GetVideoAnalyticsAsync(int videoId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.VideoAnalytics
                .Include(va => va.Video)
                .Where(va => va.VideoId == videoId && va.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(va => va.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(va => va.Date <= endDate.Value);

            var videoAnalytics = await query.ToListAsync();

            if (!videoAnalytics.Any())
            {
                return new VideoAnalyticsDto
                {
                    VideoId = videoId,
                    VideoTitle = "Unknown Video",
                    UserId = userId,
                    Date = DateTime.UtcNow
                };
            }

            var aggregated = new VideoAnalyticsDto
            {
                VideoId = videoId,
                VideoTitle = videoAnalytics.First().Video.Title,
                VideoThumbnailUrl = videoAnalytics.First().Video.ThumbnailUrl ?? "",
                UserId = userId,
                Date = DateTime.UtcNow,
                Views = videoAnalytics.Sum(va => va.Views),
                UniqueViews = videoAnalytics.Sum(va => va.UniqueViews),
                Likes = videoAnalytics.Sum(va => va.Likes),
                Dislikes = videoAnalytics.Sum(va => va.Dislikes),
                Comments = videoAnalytics.Sum(va => va.Comments),
                Shares = videoAnalytics.Sum(va => va.Shares),
                Revenue = videoAnalytics.Sum(va => va.Revenue),
                AverageWatchTime = videoAnalytics.Average(va => va.AverageWatchTime),
                EngagementRate = videoAnalytics.Average(va => va.EngagementRate)
            };

            return aggregated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video analytics for video {VideoId} and user {UserId}", videoId, userId);
            throw;
        }
    }

    public async Task<List<VideoAnalyticsDto>> GetVideoAnalyticsListAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.VideoAnalytics
                .Include(va => va.Video)
                .Where(va => va.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(va => va.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(va => va.Date <= endDate.Value);

            var videoAnalytics = await query
                .GroupBy(va => va.VideoId)
                .Select(g => new VideoAnalyticsDto
                {
                    VideoId = g.Key,
                    VideoTitle = g.First().Video.Title,
                    VideoThumbnailUrl = g.First().Video.ThumbnailUrl ?? "",
                    UserId = userId,
                    Date = g.Max(va => va.Date),
                    Views = g.Sum(va => va.Views),
                    UniqueViews = g.Sum(va => va.UniqueViews),
                    Likes = g.Sum(va => va.Likes),
                    Dislikes = g.Sum(va => va.Dislikes),
                    Comments = g.Sum(va => va.Comments),
                    Shares = g.Sum(va => va.Shares),
                    Revenue = g.Sum(va => va.Revenue),
                    AverageWatchTime = g.Average(va => va.AverageWatchTime),
                    EngagementRate = g.Average(va => va.EngagementRate)
                })
                .OrderByDescending(va => va.Views)
                .ToListAsync();

            return videoAnalytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video analytics list for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<ChannelComparisonDto>> GetChannelComparisonAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AnalyticsDashboards.Where(ad => ad.UserId == userId && ad.ChannelId.HasValue);

            if (startDate.HasValue)
                query = query.Where(ad => ad.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ad => ad.Date <= endDate.Value);

            var channelData = await query
                .Include(ad => ad.Channel)
                .GroupBy(ad => ad.ChannelId)
                .Select(g => new ChannelComparisonDto
                {
                    ChannelId = g.Key ?? 0,
                    ChannelName = g.First().Channel!.Name,
                    Views = g.Sum(ad => ad.TotalViews),
                    Subscribers = g.Sum(ad => ad.TotalSubscribers),
                    Revenue = g.Sum(ad => ad.TotalRevenue),
                    EngagementRate = g.Average(ad => ad.EngagementRate),
                    GrowthRate = 0 // Would need historical data to calculate
                })
                .OrderByDescending(c => c.Views)
                .ToListAsync();

            return channelData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel comparison for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AnalyticsSummaryDto> GetChannelAnalyticsAsync(int channelId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AnalyticsDashboards.Where(ad => ad.ChannelId == channelId && ad.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ad => ad.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ad => ad.Date <= endDate.Value);

            var analytics = await query.ToListAsync();

            var summary = new AnalyticsSummaryDto
            {
                TotalViews = analytics.Sum(a => a.TotalViews),
                TotalLikes = analytics.Sum(a => a.TotalLikes),
                TotalComments = analytics.Sum(a => a.TotalComments),
                TotalShares = analytics.Sum(a => a.TotalShares),
                TotalSubscribers = analytics.Sum(a => a.TotalSubscribers),
                TotalRevenue = analytics.Sum(a => a.TotalRevenue),
                AverageWatchTime = analytics.Any() ? analytics.Average(a => a.AverageWatchTime) : 0,
                EngagementRate = analytics.Any() ? analytics.Average(a => a.EngagementRate) : 0,
                RevenueGrowth = CalculateGrowthRate(analytics.Select(a => a.TotalRevenue).ToList()),
                SubscriberGrowth = CalculateGrowthRate(analytics.Select(a => (decimal)a.TotalSubscribers).ToList())
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel analytics for channel {ChannelId} and user {UserId}", channelId, userId);
            throw;
        }
    }

    public async Task TrackViewAsync(int videoId, string userId, string? country = null, string? deviceType = null, string? referrer = null)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            // Update or create video analytics
            var videoAnalytics = await _context.VideoAnalytics
                .FirstOrDefaultAsync(va => va.VideoId == videoId && va.UserId == userId && va.Date == today);

            if (videoAnalytics == null)
            {
                var video = await _context.Videos.FindAsync(videoId);
                if (video == null) return;

                videoAnalytics = new VideoAnalytics
                {
                    VideoId = videoId,
                    UserId = userId,
                    Date = today,
                    Country = country,
                    DeviceType = deviceType,
                    Referrer = referrer
                };
                _context.VideoAnalytics.Add(videoAnalytics);
            }

            videoAnalytics.Views++;
            videoAnalytics.UniqueViews++; // This should be more sophisticated in production

            // Update audience analytics
            if (!string.IsNullOrEmpty(country) || !string.IsNullOrEmpty(deviceType) || !string.IsNullOrEmpty(referrer))
            {
                var audienceAnalytics = await _context.AudienceAnalytics
                    .FirstOrDefaultAsync(aa => aa.UserId == userId && aa.Date == today &&
                                             aa.Country == country && aa.DeviceType == deviceType && aa.Referrer == referrer);

                if (audienceAnalytics == null)
                {
                    audienceAnalytics = new AudienceAnalytics
                    {
                        UserId = userId,
                        Date = today,
                        Country = country,
                        DeviceType = deviceType,
                        Referrer = referrer
                    };
                    _context.AudienceAnalytics.Add(audienceAnalytics);
                }

                audienceAnalytics.ViewCount++;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking view for video {VideoId} and user {UserId}", videoId, userId);
        }
    }

    public async Task TrackEngagementAsync(int videoId, string userId, string engagementType, decimal value = 1)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            // Update video analytics
            var videoAnalytics = await _context.VideoAnalytics
                .FirstOrDefaultAsync(va => va.VideoId == videoId && va.UserId == userId && va.Date == today);

            if (videoAnalytics == null)
            {
                var video = await _context.Videos.FindAsync(videoId);
                if (video == null) return;

                videoAnalytics = new VideoAnalytics
                {
                    VideoId = videoId,
                    UserId = userId,
                    Date = today
                };
                _context.VideoAnalytics.Add(videoAnalytics);
            }

            switch (engagementType.ToLower())
            {
                case "like":
                    videoAnalytics.Likes += (long)value;
                    break;
                case "dislike":
                    videoAnalytics.Dislikes += (long)value;
                    break;
                case "comment":
                    videoAnalytics.Comments += (long)value;
                    break;
                case "share":
                    videoAnalytics.Shares += (long)value;
                    break;
            }

            // Update engagement analytics
            var engagementAnalytics = await _context.EngagementAnalytics
                .FirstOrDefaultAsync(ea => ea.UserId == userId && ea.Date == today);

            if (engagementAnalytics == null)
            {
                engagementAnalytics = new EngagementAnalytics
                {
                    UserId = userId,
                    Date = today
                };
                _context.EngagementAnalytics.Add(engagementAnalytics);
            }

            switch (engagementType.ToLower())
            {
                case "like":
                    engagementAnalytics.TotalLikes += (long)value;
                    break;
                case "comment":
                    engagementAnalytics.TotalComments += (long)value;
                    break;
                case "share":
                    engagementAnalytics.TotalShares += (long)value;
                    break;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking engagement for video {VideoId} and user {UserId}", videoId, userId);
        }
    }

    public async Task TrackRevenueAsync(string userId, decimal amount, string revenueType, int? videoId = null, int? channelId = null)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            var revenueAnalytics = await _context.RevenueAnalytics
                .FirstOrDefaultAsync(ra => ra.UserId == userId && ra.Date == today);

            if (revenueAnalytics == null)
            {
                revenueAnalytics = new RevenueAnalytics
                {
                    UserId = userId,
                    ChannelId = channelId,
                    Date = today
                };
                _context.RevenueAnalytics.Add(revenueAnalytics);
            }

            revenueAnalytics.TotalRevenue += amount;

            switch (revenueType.ToLower())
            {
                case "ad":
                    revenueAnalytics.AdRevenue += amount;
                    break;
                case "subscription":
                    revenueAnalytics.SubscriptionRevenue += amount;
                    break;
                case "premium":
                    revenueAnalytics.PremiumContentRevenue += amount;
                    break;
                case "merchandise":
                    revenueAnalytics.MerchandiseRevenue += amount;
                    break;
                case "donation":
                    revenueAnalytics.DonationRevenue += amount;
                    break;
                case "referral":
                    revenueAnalytics.ReferralRevenue += amount;
                    break;
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking revenue for user {UserId}", userId);
        }
    }

    public async Task<AnalyticsSummaryDto> GetRealTimeAnalyticsAsync(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            var todayData = await _context.AnalyticsDashboards
                .Where(ad => ad.UserId == userId && ad.Date == today)
                .FirstOrDefaultAsync();

            var yesterdayData = await _context.AnalyticsDashboards
                .Where(ad => ad.UserId == userId && ad.Date == yesterday)
                .FirstOrDefaultAsync();

            var summary = new AnalyticsSummaryDto
            {
                TotalViews = todayData?.TotalViews ?? 0,
                TotalLikes = todayData?.TotalLikes ?? 0,
                TotalComments = todayData?.TotalComments ?? 0,
                TotalShares = todayData?.TotalShares ?? 0,
                TotalSubscribers = todayData?.TotalSubscribers ?? 0,
                TotalRevenue = todayData?.TotalRevenue ?? 0,
                AverageWatchTime = todayData?.AverageWatchTime ?? 0,
                EngagementRate = todayData?.EngagementRate ?? 0,
                RevenueGrowth = CalculateGrowthRate(new List<decimal> { yesterdayData?.TotalRevenue ?? 0, todayData?.TotalRevenue ?? 0 }),
                SubscriberGrowth = CalculateGrowthRate(new List<decimal> { (decimal)(yesterdayData?.TotalSubscribers ?? 0), (decimal)(todayData?.TotalSubscribers ?? 0) })
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time analytics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<VideoAnalyticsDto>> GetRealTimeVideoAnalyticsAsync(string userId)
    {
        try
        {
            var today = DateTime.UtcNow.Date;

            var videoAnalytics = await _context.VideoAnalytics
                .Include(va => va.Video)
                .Where(va => va.UserId == userId && va.Date == today)
                .Select(va => new VideoAnalyticsDto
                {
                    VideoId = va.VideoId,
                    VideoTitle = va.Video.Title,
                    VideoThumbnailUrl = va.Video.ThumbnailUrl ?? "",
                    UserId = userId,
                    Date = va.Date,
                    Views = va.Views,
                    UniqueViews = va.UniqueViews,
                    Likes = va.Likes,
                    Dislikes = va.Dislikes,
                    Comments = va.Comments,
                    Shares = va.Shares,
                    Revenue = va.Revenue,
                    AverageWatchTime = va.AverageWatchTime,
                    EngagementRate = va.EngagementRate
                })
                .OrderByDescending(va => va.Views)
                .ToListAsync();

            return videoAnalytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time video analytics for user {UserId}", userId);
            throw;
        }
    }

    public async Task<byte[]> ExportAnalyticsReportAsync(string userId, DateTime startDate, DateTime endDate, string format = "csv")
    {
        try
        {
            var analytics = await _context.AnalyticsDashboards
                .Where(ad => ad.UserId == userId && ad.Date >= startDate && ad.Date <= endDate)
                .OrderBy(ad => ad.Date)
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                var csv = "Date,TotalViews,TotalLikes,TotalComments,TotalShares,TotalSubscribers,TotalRevenue,EngagementRate\n";
                csv += string.Join("\n", analytics.Select(a => $"{a.Date:yyyy-MM-dd},{a.TotalViews},{a.TotalLikes},{a.TotalComments},{a.TotalShares},{a.TotalSubscribers},{a.TotalRevenue:F2},{a.EngagementRate:F2}"));
                return System.Text.Encoding.UTF8.GetBytes(csv);
            }

            // For other formats, you would implement JSON, Excel, etc.
            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics report for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AnalyticsDashboardDto> GenerateDashboardReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.AnalyticsDashboards.Where(ad => ad.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(ad => ad.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(ad => ad.Date <= endDate.Value);

            var analytics = await query
                .OrderByDescending(ad => ad.Date)
                .FirstOrDefaultAsync();

            if (analytics == null)
            {
                return new AnalyticsDashboardDto
                {
                    UserId = userId,
                    Date = DateTime.UtcNow
                };
            }

            return _mapper.Map<AnalyticsDashboardDto>(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard report for user {UserId}", userId);
            throw;
        }
    }

    // Helper methods
    private decimal CalculateGrowthRate(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var current = values.Last();
        var previous = values[values.Count - 2];

        if (previous == 0) return current > 0 ? 100 : 0;

        return ((current - previous) / previous) * 100;
    }

    private decimal CalculateRevenuePerView(List<RevenueAnalytics> revenueData)
    {
        var totalRevenue = revenueData.Sum(r => r.TotalRevenue);
        var totalViews = revenueData.Sum(r => r.TotalRevenue); // This would need to be calculated from views data

        return totalViews > 0 ? totalRevenue / totalViews : 0;
    }

    private decimal CalculateRevenuePerSubscriber(List<RevenueAnalytics> revenueData)
    {
        var totalRevenue = revenueData.Sum(r => r.TotalRevenue);
        var totalSubscribers = revenueData.Sum(r => r.TotalRevenue); // This would need to be calculated from subscriber data

        return totalSubscribers > 0 ? totalRevenue / totalSubscribers : 0;
    }

    private static string GetCountryCode(string country)
    {
        // This would typically use a country code mapping service
        return country.ToUpper().Substring(0, Math.Min(2, country.Length));
    }
}
