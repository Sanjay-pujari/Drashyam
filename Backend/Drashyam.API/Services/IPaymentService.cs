using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IPaymentService
{
    Task<PaymentResultDto> ProcessPaymentAsync(PaymentDto paymentDto);
    Task<PaymentResultDto> ProcessSubscriptionPaymentAsync(SubscriptionPaymentDto paymentDto);
    Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
    Task<PaymentResultDto> CreatePaymentIntentAsync(decimal amount, string currency, string customerId);
    Task<PaymentResultDto> ConfirmPaymentAsync(string paymentIntentId);
    Task<PaymentResultDto> GetPaymentStatusAsync(string paymentIntentId);
    Task<bool> CreateCustomerAsync(string userId, string email, string name);
    Task<bool> UpdateCustomerAsync(string customerId, CustomerUpdateDto updateDto);
    Task<bool> DeleteCustomerAsync(string customerId);
    Task<PagedResult<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 20);
    Task<decimal> CalculateRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}
