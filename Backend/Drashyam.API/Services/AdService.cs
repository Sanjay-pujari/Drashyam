using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class AdService : IAdService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AdService> _logger;

    public AdService(DrashyamDbContext context, IMapper mapper, ILogger<AdService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AdCampaignDto> CreateCampaignAsync(AdCampaignCreateDto campaignDto)
    {
        // Ensure advertiser exists to satisfy FK and avoid inner exception
        var advertiser = await _context.Users.FirstOrDefaultAsync(u => u.Id == campaignDto.AdvertiserId);
        if (advertiser == null)
        {
            throw new ArgumentException("Advertiser not found for the current user");
        }

        var campaign = _mapper.Map<AdCampaign>(campaignDto);
        // Set navigation to avoid null FK issues during save
        campaign.Advertiser = advertiser;
        // Also set FK explicitly
        campaign.AdvertiserId = advertiser.Id;
        // Normalize fields
        campaign.Status = AdStatus.Draft;
        campaign.CreatedAt = DateTime.UtcNow;
        // Ensure UTC DateTimes for PostgreSQL 'timestamptz'
        if (campaign.StartDate.Kind == DateTimeKind.Unspecified)
            campaign.StartDate = DateTime.SpecifyKind(campaign.StartDate, DateTimeKind.Utc);
        if (campaign.EndDate.Kind == DateTimeKind.Unspecified)
            campaign.EndDate = DateTime.SpecifyKind(campaign.EndDate, DateTimeKind.Utc);

        _context.AdCampaigns.Add(campaign);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Surface the DB error clearly to the controller
            var inner = ex.InnerException?.Message ?? ex.Message;
            throw new ArgumentException($"Failed to create campaign: {inner}");
        }

        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<AdCampaignDto> UpdateCampaignAsync(int campaignId, AdCampaignUpdateDto campaignDto)
    {
        var campaign = await _context.AdCampaigns.FindAsync(campaignId);
        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        _mapper.Map(campaignDto, campaign);
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<bool> DeleteCampaignAsync(int campaignId)
    {
        var campaign = await _context.AdCampaigns.FindAsync(campaignId);
        if (campaign == null)
            return false;

        _context.AdCampaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdCampaignDto> GetCampaignAsync(int campaignId)
    {
        var campaign = await _context.AdCampaigns
            .Include(c => c.Advertiser)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
            throw new ArgumentException("Campaign not found");

        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<PagedResult<AdCampaignDto>> GetCampaignsAsync(string advertiserId, int page = 1, int pageSize = 20)
    {
        var campaigns = await _context.AdCampaigns
            .Where(c => c.AdvertiserId == advertiserId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.AdCampaigns
            .Where(c => c.AdvertiserId == advertiserId)
            .CountAsync();

        return new PagedResult<AdCampaignDto>
        {
            Items = _mapper.Map<List<AdCampaignDto>>(campaigns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ActivateCampaignAsync(int campaignId)
    {
        var campaign = await _context.AdCampaigns.FindAsync(campaignId);
        if (campaign == null)
            return false;

        campaign.Status = AdStatus.Active;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PauseCampaignAsync(int campaignId)
    {
        var campaign = await _context.AdCampaigns.FindAsync(campaignId);
        if (campaign == null)
            return false;

        campaign.Status = AdStatus.Paused;
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdServeDto> ServeAdAsync(AdRequestDto request)
    {
        // Get active campaigns that match targeting criteria
        var campaigns = await _context.AdCampaigns
            .Where(c => c.Status == AdStatus.Active && 
                       c.StartDate <= DateTime.UtcNow && 
                       c.EndDate >= DateTime.UtcNow)
            .ToListAsync();

        if (!campaigns.Any())
        {
            return new AdServeDto { HasAd = false };
        }

        // Simple random selection (can be enhanced with more sophisticated algorithms)
        var random = new Random();
        var selectedCampaign = campaigns[random.Next(campaigns.Count)];

        // Record impression
        await RecordImpressionAsync(selectedCampaign.Id, request.UserId, request.VideoId);

        return new AdServeDto
        {
            HasAd = true,
            CampaignId = selectedCampaign.Id,
            AdType = (DTOs.AdType?)selectedCampaign.Type,
            AdContent = selectedCampaign.AdContent,
            AdUrl = selectedCampaign.AdUrl,
            ThumbnailUrl = selectedCampaign.ThumbnailUrl,
            CostPerClick = selectedCampaign.CostPerClick,
            CostPerView = selectedCampaign.CostPerView
        };
    }

    public async Task<bool> RecordImpressionAsync(int campaignId, string? userId, int? videoId)
    {
        try
        {
            var campaign = await _context.AdCampaigns.FindAsync(campaignId);
            if (campaign == null)
                return false;

            var impression = new AdImpression
            {
                AdCampaignId = campaignId,
                UserId = userId,
                VideoId = videoId,
                ViewedAt = DateTime.UtcNow,
                Revenue = campaign.CostPerView
            };

            _context.AdImpressions.Add(impression);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad impression for campaign {CampaignId}", campaignId);
            return false;
        }
    }

    public async Task<bool> RecordClickAsync(int campaignId, string? userId, int? videoId)
    {
        try
        {
            var impression = await _context.AdImpressions
                .FirstOrDefaultAsync(i => i.AdCampaignId == campaignId && 
                                         i.UserId == userId && 
                                         i.VideoId == videoId);

            if (impression == null)
                return false;

            impression.WasClicked = true;
            impression.ClickedAt = DateTime.UtcNow;
            
            var campaign = await _context.AdCampaigns.FindAsync(campaignId);
            if (campaign != null)
            {
                impression.Revenue = campaign.CostPerClick;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad click for campaign {CampaignId}", campaignId);
            return false;
        }
    }

    public async Task<AdAnalyticsDto> GetCampaignAnalyticsAsync(int campaignId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AdImpressions.Where(i => i.AdCampaignId == campaignId);

        if (startDate.HasValue)
            query = query.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.ViewedAt <= endDate.Value);

        var impressions = await query.ToListAsync();

        return new AdAnalyticsDto
        {
            CampaignId = campaignId,
            TotalImpressions = impressions.Count,
            TotalClicks = impressions.Count(i => i.WasClicked),
            ClickThroughRate = impressions.Count > 0 ? (decimal)impressions.Count(i => i.WasClicked) / impressions.Count : 0,
            TotalRevenue = impressions.Sum(i => i.Revenue),
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<decimal> CalculateRevenueAsync(string advertiserId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AdImpressions
            .Where(i => i.AdCampaign.AdvertiserId == advertiserId);

        if (startDate.HasValue)
            query = query.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.ViewedAt <= endDate.Value);

        return await query.SumAsync(i => i.Revenue);
    }

    public async Task<VideoAdResponseDto> ServeVideoAdAsync(VideoAdRequestDto request)
    {
        // Get active campaigns that match targeting criteria
        var campaigns = await _context.AdCampaigns
            .Where(c => c.Status == AdStatus.Active && 
                       c.StartDate <= DateTime.UtcNow && 
                       c.EndDate >= DateTime.UtcNow)
            .ToListAsync();

        if (!campaigns.Any())
        {
            return new VideoAdResponseDto { HasAd = false };
        }

        // Simple random selection (can be enhanced with more sophisticated algorithms)
        var random = new Random();
        var selectedCampaign = campaigns[random.Next(campaigns.Count)];

        // Record impression
        await RecordImpressionAsync(selectedCampaign.Id, request.UserId, request.VideoId);

        return new VideoAdResponseDto
        {
            HasAd = true,
            Ad = new VideoAdDto
            {
                Id = selectedCampaign.Id,
                CampaignId = selectedCampaign.Id,
                Type = selectedCampaign.Type.ToString().ToLower(),
                Content = selectedCampaign.AdContent ?? "Advertisement",
                Url = selectedCampaign.AdUrl,
                ThumbnailUrl = selectedCampaign.ThumbnailUrl,
                Duration = GetAdDuration(selectedCampaign.Type),
                SkipAfter = GetSkipAfterTime(selectedCampaign.Type),
                Position = null // Will be set by frontend for mid-roll ads
            }
        };
    }

    public async Task<bool> RecordAdCompletionAsync(int campaignId, string? userId, int? videoId, int? watchedDuration)
    {
        try
        {
            var impression = await _context.AdImpressions
                .FirstOrDefaultAsync(i => i.AdCampaignId == campaignId && 
                                         i.UserId == userId && 
                                         i.VideoId == videoId);

            if (impression == null)
                return false;

            // Update impression with completion data
            impression.WasClicked = true; // Mark as completed
            impression.ClickedAt = DateTime.UtcNow;
            
            // Calculate revenue based on watched duration
            var campaign = await _context.AdCampaigns.FindAsync(campaignId);
            if (campaign != null && watchedDuration.HasValue)
            {
                var completionRate = Math.Min(1.0m, (decimal)watchedDuration.Value / GetAdDuration(campaign.Type));
                impression.Revenue = campaign.CostPerView * completionRate;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad completion for campaign {CampaignId}", campaignId);
            return false;
        }
    }

    private int GetAdDuration(Models.AdType adType)
    {
        return adType switch
        {
            Models.AdType.Video => 30, // 30 seconds for video ads
            Models.AdType.Banner => 15, // 15 seconds for banner ads
            Models.AdType.Overlay => 10, // 10 seconds for overlay ads
            Models.AdType.Sponsored => 20, // 20 seconds for sponsored content
            _ => 15
        };
    }

    private int GetSkipAfterTime(Models.AdType adType)
    {
        return adType switch
        {
            Models.AdType.Video => 5, // Can skip after 5 seconds
            Models.AdType.Banner => 0, // Can skip immediately
            Models.AdType.Overlay => 0, // Can skip immediately
            Models.AdType.Sponsored => 3, // Can skip after 3 seconds
            _ => 5
        };
    }
}
