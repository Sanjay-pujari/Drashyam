using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IStripeService
{
    Task<PaymentResultDto> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string? paymentMethodId = null);
    Task<PaymentResultDto> ConfirmPaymentIntentAsync(string paymentIntentId);
    Task<PaymentResultDto> CreateSubscriptionAsync(string customerId, string priceId, string? paymentMethodId = null);
    Task<PaymentResultDto> UpdateSubscriptionAsync(string subscriptionId, string? priceId = null, string? paymentMethodId = null);
    Task<bool> CancelSubscriptionAsync(string subscriptionId);
    Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
    Task<string> CreateCustomerAsync(string email, string name, string? description = null);
    Task<bool> UpdateCustomerAsync(string customerId, string? email = null, string? name = null, string? description = null);
    Task<bool> DeleteCustomerAsync(string customerId);
    Task<PaymentResultDto> GetPaymentIntentAsync(string paymentIntentId);
    Task<PaymentResultDto> GetSubscriptionAsync(string subscriptionId);
    Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(string customerId, int limit = 100);
}
