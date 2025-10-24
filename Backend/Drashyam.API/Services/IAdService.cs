using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAdService
{
    Task<AdCampaignDto> CreateAdCampaignAsync(AdCampaignCreateDto createDto, string advertiserId);
    Task<AdCampaignDto> GetAdCampaignByIdAsync(int campaignId);
    Task<PagedResult<AdCampaignDto>> GetAdCampaignsAsync(string advertiserId, int page = 1, int pageSize = 20);
    Task<AdCampaignDto> UpdateAdCampaignAsync(int campaignId, AdCampaignUpdateDto updateDto, string advertiserId);
    Task<bool> DeleteAdCampaignAsync(int campaignId, string advertiserId);
    Task<bool> ActivateAdCampaignAsync(int campaignId, string advertiserId);
    Task<bool> PauseAdCampaignAsync(int campaignId, string advertiserId);
    
    // Ad serving for free users
    Task<AdDto?> GetAdForUserAsync(string userId, int? videoId = null, DTOs.AdType? preferredType = null);
    Task<List<AdDto>> GetDisplayAdsForUserAsync(string userId);
    Task<bool> RecordAdImpressionAsync(int campaignId, string userId, int? videoId = null);
    Task<bool> RecordAdClickAsync(int campaignId, string userId, int? videoId = null);
    
    // Revenue tracking
    Task<decimal> GetAdRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AdRevenueDto> GetAdRevenueBreakdownAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<PagedResult<AdImpressionDto>> GetAdImpressionsAsync(int campaignId, int page = 1, int pageSize = 20);
    
    // Analytics
    Task<AdAnalyticsDto> GetAdCampaignAnalyticsAsync(int campaignId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AdAnalyticsDto> GetAdvertiserAnalyticsAsync(string advertiserId, DateTime? startDate = null, DateTime? endDate = null);
}