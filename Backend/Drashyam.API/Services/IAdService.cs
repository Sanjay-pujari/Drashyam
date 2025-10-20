using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAdService
{
    Task<AdCampaignDto> CreateCampaignAsync(AdCampaignCreateDto campaignDto);
    Task<AdCampaignDto> UpdateCampaignAsync(int campaignId, AdCampaignUpdateDto campaignDto);
    Task<bool> DeleteCampaignAsync(int campaignId);
    Task<AdCampaignDto> GetCampaignAsync(int campaignId);
    Task<PagedResult<AdCampaignDto>> GetCampaignsAsync(string advertiserId, int page = 1, int pageSize = 20);
    Task<bool> ActivateCampaignAsync(int campaignId);
    Task<bool> PauseCampaignAsync(int campaignId);
    
    // Ad serving
    Task<AdServeDto> ServeAdAsync(AdRequestDto request);
    Task<bool> RecordImpressionAsync(int campaignId, string? userId, int? videoId);
    Task<bool> RecordClickAsync(int campaignId, string? userId, int? videoId);
    
    // Analytics
    Task<AdAnalyticsDto> GetCampaignAnalyticsAsync(int campaignId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> CalculateRevenueAsync(string advertiserId, DateTime? startDate = null, DateTime? endDate = null);
}
