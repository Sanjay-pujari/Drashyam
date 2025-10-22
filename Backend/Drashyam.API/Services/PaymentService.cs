using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class PaymentService : IPaymentService
{
    private readonly DrashyamDbContext _context;
    private readonly IStripeService _stripeService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(DrashyamDbContext context, IStripeService stripeService, ILogger<PaymentService> logger)
    {
        _context = context;
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<PaymentResultDto> ProcessPaymentAsync(PaymentDto paymentDto)
    {
        try
        {
            return await _stripeService.CreatePaymentIntentAsync(
                paymentDto.Amount, 
                paymentDto.Currency, 
                paymentDto.CustomerId, 
                paymentDto.PaymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for customer {CustomerId}", paymentDto.CustomerId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Amount = paymentDto.Amount,
                Currency = paymentDto.Currency,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> ProcessSubscriptionPaymentAsync(SubscriptionPaymentDto paymentDto)
    {
        try
        {
            // Get subscription plan to get price ID
            var plan = await _context.SubscriptionPlans.FindAsync(paymentDto.SubscriptionPlanId);
            if (plan == null)
            {
                return new PaymentResultDto
                {
                    Success = false,
                    ErrorMessage = "Subscription plan not found",
                    Status = "failed"
                };
            }

            // For now, we'll use a hardcoded price ID - in production, you'd store Stripe price IDs in your database
            var priceId = GetStripePriceId(plan.Name);
            
            return await _stripeService.CreateSubscriptionAsync(
                paymentDto.CustomerId, 
                priceId, 
                paymentDto.PaymentMethodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription payment for customer {CustomerId}", paymentDto.CustomerId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        try
        {
            return await _stripeService.RefundPaymentAsync(paymentIntentId, amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<PaymentResultDto> CreatePaymentIntentAsync(decimal amount, string currency, string customerId)
    {
        try
        {
            return await _stripeService.CreatePaymentIntentAsync(amount, currency, customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent for customer {CustomerId}", customerId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Amount = amount,
                Currency = currency,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            return await _stripeService.ConfirmPaymentIntentAsync(paymentIntentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment {PaymentIntentId}", paymentIntentId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> GetPaymentStatusAsync(string paymentIntentId)
    {
        try
        {
            return await _stripeService.GetPaymentIntentAsync(paymentIntentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment status for {PaymentIntentId}", paymentIntentId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<bool> CreateCustomerAsync(string userId, string email, string name)
    {
        try
        {
            var customerId = await _stripeService.CreateCustomerAsync(email, name);
            
            // Update user with Stripe customer ID
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.StripeCustomerId = customerId;
                await _context.SaveChangesAsync();
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateCustomerAsync(string customerId, CustomerUpdateDto updateDto)
    {
        try
        {
            return await _stripeService.UpdateCustomerAsync(customerId, updateDto.Email, updateDto.Name, updateDto.Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string customerId)
    {
        try
        {
            return await _stripeService.DeleteCustomerAsync(customerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<PagedResult<PaymentHistoryDto>> GetPaymentHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.StripeCustomerId == null)
            {
                return new PagedResult<PaymentHistoryDto>
                {
                    Items = new List<PaymentHistoryDto>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize
                };
            }

            var payments = await _stripeService.GetPaymentHistoryAsync(user.StripeCustomerId, pageSize);
            
            return new PagedResult<PaymentHistoryDto>
            {
                Items = payments,
                TotalCount = payments.Count,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment history for user {UserId}", userId);
            return new PagedResult<PaymentHistoryDto>
            {
                Items = new List<PaymentHistoryDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }
    }

    public async Task<decimal> CalculateRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.StripeCustomerId == null)
                return 0;

            var payments = await _stripeService.GetPaymentHistoryAsync(user.StripeCustomerId, 100);
            
            var filteredPayments = payments.Where(p => p.Status == "succeeded");
            
            if (startDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.CreatedAt >= startDate.Value);
            
            if (endDate.HasValue)
                filteredPayments = filteredPayments.Where(p => p.CreatedAt <= endDate.Value);

            return filteredPayments.Sum(p => p.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating revenue for user {UserId}", userId);
            return 0;
        }
    }

    private string GetStripePriceId(string planName)
    {
        // In production, you'd store these in your database
        return planName.ToLower() switch
        {
            "premium" => "price_premium_monthly", // Replace with actual Stripe price ID
            "pro" => "price_pro_monthly", // Replace with actual Stripe price ID
            _ => "price_free" // Replace with actual Stripe price ID
        };
    }
}
