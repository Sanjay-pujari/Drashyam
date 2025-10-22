using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IQuotaService
{
    Task<QuotaStatusDto> GetUserQuotaStatusAsync(string userId);
    Task<bool> CheckStorageQuotaAsync(string userId, long fileSize);
    Task<bool> CheckVideoQuotaAsync(int channelId);
    Task<bool> CheckChannelQuotaAsync(string userId);
    Task<bool> CheckSubscriptionFeaturesAsync(string userId, string feature);
    Task<QuotaWarningDto> GetQuotaWarningsAsync(string userId);
    Task<bool> CanUploadVideoAsync(string userId, int channelId, long fileSize);
    Task<bool> CanCreateChannelAsync(string userId);
    Task<SubscriptionBenefitsDto> GetSubscriptionBenefitsAsync(string userId);
}
