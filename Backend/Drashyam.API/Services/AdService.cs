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
    private readonly ISubscriptionService _subscriptionService;

    public AdService(DrashyamDbContext context, IMapper mapper, ILogger<AdService> logger, ISubscriptionService subscriptionService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _subscriptionService = subscriptionService;
    }

    public async Task<AdCampaignDto> CreateAdCampaignAsync(AdCampaignCreateDto createDto, string advertiserId)
    {
        var campaign = _mapper.Map<AdCampaign>(createDto);
        campaign.AdvertiserId = advertiserId;
        campaign.Status = AdStatus.Draft;
        campaign.CreatedAt = DateTime.UtcNow;

        _context.AdCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<AdCampaignDto> GetAdCampaignByIdAsync(int campaignId)
    {
        var campaign = await _context.AdCampaigns
            .Include(a => a.Advertiser)
            .Include(a => a.Impressions)
            .FirstOrDefaultAsync(a => a.Id == campaignId);

        if (campaign == null)
            throw new ArgumentException("Ad campaign not found");

        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<PagedResult<AdCampaignDto>> GetAdCampaignsAsync(string advertiserId, int page = 1, int pageSize = 20)
    {
        var campaigns = await _context.AdCampaigns
            .Where(a => a.AdvertiserId == advertiserId)
            .Include(a => a.Impressions)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.AdCampaigns
            .Where(a => a.AdvertiserId == advertiserId)
            .CountAsync();

        return new PagedResult<AdCampaignDto>
        {
            Items = _mapper.Map<List<AdCampaignDto>>(campaigns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdCampaignDto> UpdateAdCampaignAsync(int campaignId, AdCampaignUpdateDto updateDto, string advertiserId)
    {
        var campaign = await _context.AdCampaigns
            .FirstOrDefaultAsync(a => a.Id == campaignId && a.AdvertiserId == advertiserId);

        if (campaign == null)
            throw new ArgumentException("Ad campaign not found");

        _mapper.Map(updateDto, campaign);
        campaign.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<AdCampaignDto>(campaign);
    }

    public async Task<bool> DeleteAdCampaignAsync(int campaignId, string advertiserId)
    {
        var campaign = await _context.AdCampaigns
            .FirstOrDefaultAsync(a => a.Id == campaignId && a.AdvertiserId == advertiserId);

        if (campaign == null)
            return false;

        _context.AdCampaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAdCampaignAsync(int campaignId, string advertiserId)
    {
        var campaign = await _context.AdCampaigns
            .FirstOrDefaultAsync(a => a.Id == campaignId && a.AdvertiserId == advertiserId);

        if (campaign == null)
            return false;

        campaign.Status = AdStatus.Active;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PauseAdCampaignAsync(int campaignId, string advertiserId)
    {
        var campaign = await _context.AdCampaigns
            .FirstOrDefaultAsync(a => a.Id == campaignId && a.AdvertiserId == advertiserId);

        if (campaign == null)
            return false;

        campaign.Status = AdStatus.Paused;
        campaign.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdDto?> GetAdForUserAsync(string userId, int? videoId = null, Models.AdType? preferredType = null)
    {
        // Check if user has a paid subscription (no ads for paid users)
        try
        {
            var subscription = await _subscriptionService.GetUserSubscriptionAsync(userId);
            if (subscription.Plan != null && !subscription.Plan.HasAds)
            {
                return null; // No ads for paid users
            }
        }
        catch
        {
            // User has no subscription, show ads
        }

        // Get active campaigns that match criteria
        var query = _context.AdCampaigns
            .Where(a => a.Status == AdStatus.Active && 
                       a.StartDate <= DateTime.UtcNow && 
                       a.EndDate >= DateTime.UtcNow);

        if (preferredType.HasValue)
        {
            query = query.Where(a => a.Type == preferredType.Value);
        }

        var campaigns = await query.ToListAsync();

        if (!campaigns.Any())
            return null;

        // Simple random selection (in production, use more sophisticated targeting)
        var random = new Random();
        var selectedCampaign = campaigns[random.Next(campaigns.Count)];

        return new AdDto
        {
            Id = selectedCampaign.Id,
            Type = (DTOs.AdType)selectedCampaign.Type,
            Content = selectedCampaign.AdContent,
            Url = selectedCampaign.AdUrl,
            ThumbnailUrl = selectedCampaign.ThumbnailUrl,
            CostPerClick = selectedCampaign.CostPerClick,
            CostPerView = selectedCampaign.CostPerView
        };
    }

    public async Task<bool> RecordAdImpressionAsync(int campaignId, string userId, int? videoId = null)
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

    public async Task<bool> RecordAdClickAsync(int campaignId, string userId, int? videoId = null)
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

    public async Task<decimal> GetAdRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AdImpressions
            .Where(i => i.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.ViewedAt <= endDate.Value);

        return await query.SumAsync(i => i.Revenue);
    }

    public async Task<AdRevenueDto> GetAdRevenueBreakdownAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AdImpressions
            .Where(i => i.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.ViewedAt <= endDate.Value);

        var impressions = await query.Include(i => i.AdCampaign).ToListAsync();

        var totalRevenue = impressions.Sum(i => i.Revenue);
        var totalImpressions = impressions.Count;
        var totalClicks = impressions.Count(i => i.WasClicked);

        return new AdRevenueDto
        {
            TotalRevenue = totalRevenue,
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            ClickThroughRate = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions : 0,
            RevenuePerImpression = totalImpressions > 0 ? totalRevenue / totalImpressions : 0,
            RevenuePerClick = totalClicks > 0 ? totalRevenue / totalClicks : 0
        };
    }

    public async Task<PagedResult<AdImpressionDto>> GetAdImpressionsAsync(int campaignId, int page = 1, int pageSize = 20)
    {
        var impressions = await _context.AdImpressions
            .Where(i => i.AdCampaignId == campaignId)
            .Include(i => i.User)
            .Include(i => i.Video)
            .OrderByDescending(i => i.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.AdImpressions
            .Where(i => i.AdCampaignId == campaignId)
            .CountAsync();

        return new PagedResult<AdImpressionDto>
        {
            Items = _mapper.Map<List<AdImpressionDto>>(impressions),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdAnalyticsDto> GetAdCampaignAnalyticsAsync(int campaignId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.AdImpressions
            .Where(i => i.AdCampaignId == campaignId);

        if (startDate.HasValue)
            query = query.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(i => i.ViewedAt <= endDate.Value);

        var impressions = await query.ToListAsync();

        var totalImpressions = impressions.Count;
        var totalClicks = impressions.Count(i => i.WasClicked);
        var totalRevenue = impressions.Sum(i => i.Revenue);

        return new AdAnalyticsDto
        {
            CampaignId = campaignId,
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            TotalRevenue = totalRevenue,
            ClickThroughRate = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions : 0,
            CostPerClick = totalClicks > 0 ? totalRevenue / totalClicks : 0,
            CostPerImpression = totalImpressions > 0 ? totalRevenue / totalImpressions : 0
        };
    }

    public async Task<AdAnalyticsDto> GetAdvertiserAnalyticsAsync(string advertiserId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var baseQuery = _context.AdImpressions
            .Include(i => i.AdCampaign)
            .Where(i => i.AdCampaign.AdvertiserId == advertiserId);

        if (startDate.HasValue)
            baseQuery = baseQuery.Where(i => i.ViewedAt >= startDate.Value);

        if (endDate.HasValue)
            baseQuery = baseQuery.Where(i => i.ViewedAt <= endDate.Value);

        var impressions = await baseQuery.ToListAsync();

        var totalImpressions = impressions.Count;
        var totalClicks = impressions.Count(i => i.WasClicked);
        var totalRevenue = impressions.Sum(i => i.Revenue);

        return new AdAnalyticsDto
        {
            CampaignId = 0, // Not applicable for advertiser analytics
            TotalImpressions = totalImpressions,
            TotalClicks = totalClicks,
            TotalRevenue = totalRevenue,
            ClickThroughRate = totalImpressions > 0 ? (decimal)totalClicks / totalImpressions : 0,
            CostPerClick = totalClicks > 0 ? totalRevenue / totalClicks : 0,
            CostPerImpression = totalImpressions > 0 ? totalRevenue / totalImpressions : 0
        };
    }
}