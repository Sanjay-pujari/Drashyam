using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class MonetizationService : IMonetizationService
{
    private readonly DrashyamDbContext _context;
    private readonly ILogger<MonetizationService> _logger;

    public MonetizationService(DrashyamDbContext context, ILogger<MonetizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MonetizationStatusDto> GetMonetizationStatusAsync(string userId)
    {
        // Implement monetization status check
        return new MonetizationStatusDto();
    }

    public async Task<MonetizationStatusDto> EnableMonetizationAsync(string userId, MonetizationRequestDto request)
    {
        // Implement monetization enablement
        return new MonetizationStatusDto();
    }

    public async Task<MonetizationStatusDto> DisableMonetizationAsync(string userId)
    {
        // Implement monetization disablement
        return new MonetizationStatusDto();
    }

    public async Task<List<AdPlacementDto>> GetAdPlacementsAsync(int videoId, string userId)
    {
        // Implement ad placements retrieval
        return new List<AdPlacementDto>();
    }

    public async Task<AdPlacementDto> CreateAdPlacementAsync(int videoId, AdPlacementRequestDto request, string userId)
    {
        // Implement ad placement creation
        return new AdPlacementDto();
    }

    public async Task<AdPlacementDto> UpdateAdPlacementAsync(int placementId, AdPlacementRequestDto request, string userId)
    {
        // Implement ad placement update
        return new AdPlacementDto();
    }

    public async Task DeleteAdPlacementAsync(int placementId, string userId)
    {
        // Implement ad placement deletion
    }

    public async Task<List<SponsorDto>> GetSponsorsAsync(string userId)
    {
        // Implement sponsors retrieval
        return new List<SponsorDto>();
    }

    public async Task<SponsorDto> CreateSponsorAsync(SponsorRequestDto request, string userId)
    {
        // Implement sponsor creation
        return new SponsorDto();
    }

    public async Task<SponsorDto> UpdateSponsorAsync(int sponsorId, SponsorRequestDto request, string userId)
    {
        // Implement sponsor update
        return new SponsorDto();
    }

    public async Task DeleteSponsorAsync(int sponsorId, string userId)
    {
        // Implement sponsor deletion
    }

    public async Task<List<DonationDto>> GetDonationsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Implement donations retrieval
        return new List<DonationDto>();
    }

    public async Task<DonationDto> ProcessDonationAsync(DonationRequestDto request)
    {
        // Implement donation processing
        return new DonationDto();
    }

    public async Task<List<MerchandiseDto>> GetMerchandiseAsync(string userId)
    {
        try
        {
            var merchandiseItems = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .Where(m => m.Channel.UserId == userId)
                .ToListAsync();

            var merchandise = merchandiseItems.Select(m => new MerchandiseDto
            {
                Id = m.Id,
                ChannelId = m.ChannelId,
                ChannelName = m.Channel.Name,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price,
                Currency = m.Currency,
                ImageUrl = m.ImageUrl,
                StockQuantity = m.StockQuantity,
                IsActive = m.IsActive,
                Category = m.Category,
                Sizes = !string.IsNullOrEmpty(m.Sizes) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(m.Sizes) ?? new List<string>() : new List<string>(),
                Colors = !string.IsNullOrEmpty(m.Colors) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(m.Colors) ?? new List<string>() : new List<string>(),
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList();

            return merchandise;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchandise for user {UserId}", userId);
            throw;
        }
    }

    public async Task<MerchandiseDto> CreateMerchandiseAsync(MerchandiseRequestDto request, string userId)
    {
        try
        {
            // Verify channel ownership
            var channel = await _context.Channels
                .FirstOrDefaultAsync(c => c.Id == request.ChannelId && c.UserId == userId);

            if (channel == null)
            {
                throw new UnauthorizedAccessException("You don't have permission to add merchandise to this channel");
            }

            var merchandise = new MerchandiseItem
            {
                ChannelId = request.ChannelId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency,
                StockQuantity = request.StockQuantity,
                IsActive = request.IsActive,
                Category = request.Category,
                Sizes = System.Text.Json.JsonSerializer.Serialize(request.Sizes),
                Colors = System.Text.Json.JsonSerializer.Serialize(request.Colors),
                CreatedAt = DateTime.UtcNow
            };

            _context.MerchandiseItems.Add(merchandise);
            await _context.SaveChangesAsync();

            return new MerchandiseDto
            {
                Id = merchandise.Id,
                ChannelId = merchandise.ChannelId,
                ChannelName = channel.Name,
                Name = merchandise.Name,
                Description = merchandise.Description,
                Price = merchandise.Price,
                Currency = merchandise.Currency,
                ImageUrl = merchandise.ImageUrl,
                StockQuantity = merchandise.StockQuantity,
                IsActive = merchandise.IsActive,
                Category = merchandise.Category,
                Sizes = request.Sizes,
                Colors = request.Colors,
                CreatedAt = merchandise.CreatedAt,
                UpdatedAt = merchandise.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchandise for user {UserId}", userId);
            throw;
        }
    }

    public async Task<MerchandiseDto> UpdateMerchandiseAsync(int merchandiseId, MerchandiseRequestDto request, string userId)
    {
        try
        {
            var merchandise = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .FirstOrDefaultAsync(m => m.Id == merchandiseId && m.Channel.UserId == userId);

            if (merchandise == null)
            {
                throw new UnauthorizedAccessException("Merchandise not found or you don't have permission to update it");
            }

            merchandise.Name = request.Name;
            merchandise.Description = request.Description;
            merchandise.Price = request.Price;
            merchandise.Currency = request.Currency;
            merchandise.StockQuantity = request.StockQuantity;
            merchandise.IsActive = request.IsActive;
            merchandise.Category = request.Category;
            merchandise.Sizes = System.Text.Json.JsonSerializer.Serialize(request.Sizes);
            merchandise.Colors = System.Text.Json.JsonSerializer.Serialize(request.Colors);
            merchandise.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MerchandiseDto
            {
                Id = merchandise.Id,
                ChannelId = merchandise.ChannelId,
                ChannelName = merchandise.Channel.Name,
                Name = merchandise.Name,
                Description = merchandise.Description,
                Price = merchandise.Price,
                Currency = merchandise.Currency,
                ImageUrl = merchandise.ImageUrl,
                StockQuantity = merchandise.StockQuantity,
                IsActive = merchandise.IsActive,
                Category = merchandise.Category,
                Sizes = request.Sizes,
                Colors = request.Colors,
                CreatedAt = merchandise.CreatedAt,
                UpdatedAt = merchandise.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise {MerchandiseId} for user {UserId}", merchandiseId, userId);
            throw;
        }
    }

    public async Task DeleteMerchandiseAsync(int merchandiseId, string userId)
    {
        try
        {
            var merchandise = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .FirstOrDefaultAsync(m => m.Id == merchandiseId && m.Channel.UserId == userId);

            if (merchandise == null)
            {
                throw new UnauthorizedAccessException("Merchandise not found or you don't have permission to delete it");
            }

            _context.MerchandiseItems.Remove(merchandise);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting merchandise {MerchandiseId} for user {UserId}", merchandiseId, userId);
            throw;
        }
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Implement revenue report generation
        return new RevenueReportDto();
    }

    public async Task ProcessAdRevenueAsync(int videoId, decimal amount, string adType)
    {
        // Implement ad revenue processing
    }

    public async Task ProcessSponsorRevenueAsync(int sponsorId, decimal amount)
    {
        // Implement sponsor revenue processing
    }

    public async Task ProcessDonationRevenueAsync(int donationId, decimal amount)
    {
        // Implement donation revenue processing
    }

    public async Task ProcessMerchandiseRevenueAsync(int merchandiseId, decimal amount)
    {
        // Implement merchandise revenue processing
    }

    // Merchandise Order Methods
    public async Task<List<MerchandiseOrderDto>> GetMerchandiseOrdersAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var orders = await _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .ThenInclude(m => m.Channel)
                .Where(o => o.MerchandiseItem.Channel.UserId == userId)
                .OrderByDescending(o => o.OrderedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new MerchandiseOrderDto
                {
                    Id = o.Id,
                    MerchandiseItemId = o.MerchandiseItemId,
                    MerchandiseName = o.MerchandiseItem.Name,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    CustomerAddress = o.CustomerAddress,
                    Amount = o.Amount,
                    Currency = o.Currency,
                    Quantity = o.Quantity,
                    Size = o.Size,
                    Color = o.Color,
                    PaymentIntentId = o.PaymentIntentId,
                    Status = o.Status,
                    OrderedAt = o.OrderedAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    TrackingNumber = o.TrackingNumber
                })
                .ToListAsync();

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchandise orders for user {UserId}", userId);
            throw;
        }
    }

    public async Task<MerchandiseOrderDto> CreateMerchandiseOrderAsync(MerchandiseOrderRequestDto request, string customerId)
    {
        try
        {
            var merchandise = await _context.MerchandiseItems
                .FirstOrDefaultAsync(m => m.Id == request.MerchandiseItemId && m.IsActive);

            if (merchandise == null)
            {
                throw new ArgumentException("Merchandise item not found or not available");
            }

            if (merchandise.StockQuantity < request.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock quantity");
            }

            var order = new MerchandiseOrder
            {
                MerchandiseItemId = request.MerchandiseItemId,
                CustomerId = customerId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerAddress = request.CustomerAddress,
                Amount = merchandise.Price * request.Quantity,
                Currency = merchandise.Currency,
                Quantity = request.Quantity,
                Size = request.Size,
                Color = request.Color,
                PaymentIntentId = request.PaymentMethodId,
                Status = MerchandiseOrderStatus.Pending,
                OrderedAt = DateTime.UtcNow
            };

            _context.MerchandiseOrders.Add(order);
            
            // Update stock quantity
            merchandise.StockQuantity -= request.Quantity;
            
            await _context.SaveChangesAsync();

            return new MerchandiseOrderDto
            {
                Id = order.Id,
                MerchandiseItemId = order.MerchandiseItemId,
                MerchandiseName = merchandise.Name,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerAddress = order.CustomerAddress,
                Amount = order.Amount,
                Currency = order.Currency,
                Quantity = order.Quantity,
                Size = order.Size,
                Color = order.Color,
                PaymentIntentId = order.PaymentIntentId,
                Status = order.Status,
                OrderedAt = order.OrderedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                TrackingNumber = order.TrackingNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchandise order for customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<MerchandiseOrderDto> UpdateMerchandiseOrderStatusAsync(int orderId, MerchandiseOrderStatus status, string userId, string? trackingNumber = null)
    {
        try
        {
            var order = await _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .ThenInclude(m => m.Channel)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MerchandiseItem.Channel.UserId == userId);

            if (order == null)
            {
                throw new UnauthorizedAccessException("Order not found or you don't have permission to update it");
            }

            order.Status = status;
            
            if (status == MerchandiseOrderStatus.Shipped)
            {
                order.ShippedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(trackingNumber))
                {
                    order.TrackingNumber = trackingNumber;
                }
            }
            else if (status == MerchandiseOrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new MerchandiseOrderDto
            {
                Id = order.Id,
                MerchandiseItemId = order.MerchandiseItemId,
                MerchandiseName = order.MerchandiseItem.Name,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerAddress = order.CustomerAddress,
                Amount = order.Amount,
                Currency = order.Currency,
                Quantity = order.Quantity,
                Size = order.Size,
                Color = order.Color,
                PaymentIntentId = order.PaymentIntentId,
                Status = order.Status,
                OrderedAt = order.OrderedAt,
                ShippedAt = order.ShippedAt,
                DeliveredAt = order.DeliveredAt,
                TrackingNumber = order.TrackingNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise order {OrderId} for user {UserId}", orderId, userId);
            throw;
        }
    }

    public async Task<List<MerchandiseOrderDto>> GetCustomerOrdersAsync(string customerId, int page = 1, int pageSize = 20)
    {
        try
        {
            var orders = await _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new MerchandiseOrderDto
                {
                    Id = o.Id,
                    MerchandiseItemId = o.MerchandiseItemId,
                    MerchandiseName = o.MerchandiseItem.Name,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    CustomerAddress = o.CustomerAddress,
                    Amount = o.Amount,
                    Currency = o.Currency,
                    Quantity = o.Quantity,
                    Size = o.Size,
                    Color = o.Color,
                    PaymentIntentId = o.PaymentIntentId,
                    Status = o.Status,
                    OrderedAt = o.OrderedAt,
                    ShippedAt = o.ShippedAt,
                    DeliveredAt = o.DeliveredAt,
                    TrackingNumber = o.TrackingNumber
                })
                .ToListAsync();

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer orders for {CustomerId}", customerId);
            throw;
        }
    }
}
