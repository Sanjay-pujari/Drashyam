using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ILiveStreamMonetizationService
{
    Task<LiveStreamDonationDto> SendDonationAsync(SendDonationDto dto, string userId);
    Task<List<LiveStreamDonationDto>> GetDonationsAsync(int liveStreamId, int page = 1, int pageSize = 50);
    Task<LiveStreamSuperChatDto> SendSuperChatAsync(SendSuperChatDto dto, string userId);
    Task<List<LiveStreamSuperChatDto>> GetSuperChatsAsync(int liveStreamId, int page = 1, int pageSize = 50);
    Task<LiveStreamSubscriptionDto> SubscribeToLiveStreamAsync(SubscribeToLiveStreamDto dto, string userId);
    Task<List<LiveStreamSubscriptionDto>> GetSubscriptionsAsync(int liveStreamId, int page = 1, int pageSize = 50);
    Task<bool> UnsubscribeFromLiveStreamAsync(int liveStreamId, string userId);
    Task<LiveStreamRevenueDto> GetRevenueAsync(int liveStreamId);
    Task<LiveStreamMonetizationStatsDto> GetMonetizationStatsAsync(int liveStreamId);
    Task<List<LiveStreamDonationDto>> GetUserDonationsAsync(string userId, int page = 1, int pageSize = 50);
    Task<List<LiveStreamSuperChatDto>> GetUserSuperChatsAsync(string userId, int page = 1, int pageSize = 50);
    Task<List<LiveStreamSubscriptionDto>> GetUserSubscriptionsAsync(string userId, int page = 1, int pageSize = 50);
    Task<decimal> GetTotalRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetLiveStreamRevenueAsync(int liveStreamId);
    Task<bool> ProcessPaymentAsync(string userId, decimal amount, string currency, string paymentMethod);
    Task<bool> RefundPaymentAsync(int transactionId, string reason);
}
