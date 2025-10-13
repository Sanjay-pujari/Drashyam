using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto createDto, string userId);
    Task<SubscriptionDto> GetSubscriptionByIdAsync(int subscriptionId);
    Task<SubscriptionDto> GetUserSubscriptionAsync(string userId);
    Task<SubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, SubscriptionUpdateDto updateDto, string userId);
    Task<bool> CancelSubscriptionAsync(int subscriptionId, string userId);
    Task<bool> RenewSubscriptionAsync(int subscriptionId, string userId);
    Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(int page = 1, int pageSize = 20);
    Task<SubscriptionPlanDto> GetSubscriptionPlanByIdAsync(int planId);
    Task<SubscriptionDto> UpgradeSubscriptionAsync(string userId, int newPlanId);
    Task<SubscriptionDto> DowngradeSubscriptionAsync(string userId, int newPlanId);
    Task<bool> CheckSubscriptionStatusAsync(string userId);
    Task<SubscriptionDto> ProcessPaymentAsync(string userId, PaymentDto paymentDto);
    Task<PagedResult<SubscriptionDto>> GetExpiringSubscriptionsAsync(int daysAhead = 7, int page = 1, int pageSize = 20);
}
