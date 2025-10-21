using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IPremiumContentService
{
    Task<PremiumVideoDto> CreatePremiumVideoAsync(PremiumVideoCreateDto premiumVideoDto);
    Task<PremiumVideoDto> UpdatePremiumVideoAsync(int premiumVideoId, PremiumVideoUpdateDto premiumVideoDto);
    Task<bool> DeletePremiumVideoAsync(int premiumVideoId);
    Task<PremiumVideoDto> GetPremiumVideoAsync(int premiumVideoId);
    Task<PremiumVideoDto?> GetPremiumVideoByVideoIdAsync(int videoId);
    Task<PagedResult<PremiumVideoDto>> GetPremiumVideosAsync(int page = 1, int pageSize = 20);
    Task<bool> IsVideoPremiumAsync(int videoId);
    Task<bool> HasUserPurchasedAsync(int premiumVideoId, string userId);
    
    // Purchase handling
    Task<PremiumPurchaseDto> CreatePurchaseAsync(PremiumPurchaseCreateDto purchaseDto);
    Task<bool> CompletePurchaseAsync(string paymentIntentId);
    Task<bool> RefundPurchaseAsync(int purchaseId);
    Task<PagedResult<PremiumPurchaseDto>> GetUserPurchasesAsync(string userId, int page = 1, int pageSize = 20);
    
    // Analytics
    Task<PremiumContentAnalyticsDto> GetPremiumContentAnalyticsAsync(int premiumVideoId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> CalculatePremiumRevenueAsync(string creatorId, DateTime? startDate = null, DateTime? endDate = null);
}
