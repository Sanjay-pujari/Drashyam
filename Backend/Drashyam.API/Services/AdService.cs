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
        var campaign = _mapper.Map<AdCampaign>(campaignDto);
        _context.AdCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

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
}
