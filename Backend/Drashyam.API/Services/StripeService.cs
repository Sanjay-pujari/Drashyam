using Drashyam.API.DTOs;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using StripeSubscriptionService = Stripe.SubscriptionService;

namespace Drashyam.API.Services;

public class StripeService : IStripeService
{
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripeService> _logger;

    public StripeService(IOptions<StripeSettings> stripeSettings, ILogger<StripeService> logger)
    {
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
        
        // Configure Stripe API key
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<PaymentResultDto> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, string? paymentMethodId = null)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                ConfirmationMethod = "manual",
                Confirm = true,
                ReturnUrl = "https://your-app.com/payment/return"
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new PaymentResultDto
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = amount,
                Currency = currency,
                Status = paymentIntent.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating payment intent for customer {CustomerId}", customerId);
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

    public async Task<PaymentResultDto> ConfirmPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return new PaymentResultDto
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                Amount = (decimal)paymentIntent.Amount / 100,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment intent {PaymentIntentId}", paymentIntentId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> CreateSubscriptionAsync(string customerId, string priceId, string? paymentMethodId = null)
    {
        try
        {
            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId
                    }
                },
                DefaultPaymentMethod = paymentMethodId,
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" }
            };

            var service = new StripeSubscriptionService();
            var subscription = await service.CreateAsync(options);

            return new PaymentResultDto
            {
                Success = subscription.Status == "active",
                PaymentIntentId = subscription.LatestInvoice?.PaymentIntent?.Id,
                ClientSecret = subscription.LatestInvoice?.PaymentIntent?.ClientSecret,
                Amount = (decimal)(subscription.LatestInvoice?.AmountPaid ?? 0) / 100,
                Currency = subscription.Currency,
                Status = subscription.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for customer {CustomerId}", customerId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> UpdateSubscriptionAsync(string subscriptionId, string? priceId = null, string? paymentMethodId = null)
    {
        try
        {
            var options = new SubscriptionUpdateOptions();
            
            if (!string.IsNullOrEmpty(priceId))
            {
                options.Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId
                    }
                };
            }

            if (!string.IsNullOrEmpty(paymentMethodId))
            {
                options.DefaultPaymentMethod = paymentMethodId;
            }

            var service = new StripeSubscriptionService();
            var subscription = await service.UpdateAsync(subscriptionId, options);

            return new PaymentResultDto
            {
                Success = subscription.Status == "active",
                Amount = (decimal)(subscription.LatestInvoice?.AmountPaid ?? 0) / 100,
                Currency = subscription.Currency,
                Status = subscription.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error updating subscription {SubscriptionId}", subscriptionId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new StripeSubscriptionService();
            var subscription = await service.CancelAsync(subscriptionId);
            return subscription.Status == "canceled";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error canceling subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amount.HasValue ? (long)(amount.Value * 100) : null
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);
            return refund.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error refunding payment {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<string> CreateCustomerAsync(string email, string name, string? description = null)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Description = description
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating customer for email {Email}", email);
            throw;
        }
    }

    public async Task<bool> UpdateCustomerAsync(string customerId, string? email = null, string? name = null, string? description = null)
    {
        try
        {
            var options = new CustomerUpdateOptions();
            
            if (!string.IsNullOrEmpty(email))
                options.Email = email;
            
            if (!string.IsNullOrEmpty(name))
                options.Name = name;
            
            if (!string.IsNullOrEmpty(description))
                options.Description = description;

            var service = new CustomerService();
            await service.UpdateAsync(customerId, options);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error updating customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string customerId)
    {
        try
        {
            var service = new CustomerService();
            await service.DeleteAsync(customerId);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error deleting customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<PaymentResultDto> GetPaymentIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return new PaymentResultDto
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                Amount = (decimal)paymentIntent.Amount / 100,
                Currency = paymentIntent.Currency,
                Status = paymentIntent.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting payment intent {PaymentIntentId}", paymentIntentId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<PaymentResultDto> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new StripeSubscriptionService();
            var subscription = await service.GetAsync(subscriptionId);

            return new PaymentResultDto
            {
                Success = subscription.Status == "active",
                Amount = (decimal)(subscription.LatestInvoice?.AmountPaid ?? 0) / 100,
                Currency = subscription.Currency,
                Status = subscription.Status
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting subscription {SubscriptionId}", subscriptionId);
            return new PaymentResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
    }

    public async Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(string customerId, int limit = 100)
    {
        try
        {
            var options = new PaymentIntentListOptions
            {
                Customer = customerId,
                Limit = limit
            };

            var service = new PaymentIntentService();
            var paymentIntents = await service.ListAsync(options);

            return paymentIntents.Data.Select(pi => new PaymentHistoryDto
            {
                Id = pi.Id,
                Amount = (decimal)pi.Amount / 100,
                Currency = pi.Currency,
                Status = pi.Status,
                Description = pi.Description,
                CreatedAt = pi.Created,
                PaymentMethodId = pi.PaymentMethod?.Id
            }).ToList();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting payment history for customer {CustomerId}", customerId);
            return new List<PaymentHistoryDto>();
        }
    }
}

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
