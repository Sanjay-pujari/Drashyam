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

    public async Task<List<MerchandiseDto>> GetMerchandiseAsync(MerchandiseFilterDto filter)
    {
        try
        {
            var query = _context.MerchandiseItems
                .Include(m => m.Channel)
                .AsQueryable();

            // Apply filters
            if (filter.ChannelId.HasValue)
            {
                query = query.Where(m => m.ChannelId == filter.ChannelId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Search))
            {
                query = query.Where(m => m.Name.Contains(filter.Search) || m.Description.Contains(filter.Search));
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(m => m.IsActive == filter.IsActive.Value);
            }

            var merchandiseItems = await query.ToListAsync();

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
            _logger.LogError(ex, "Error retrieving merchandise");
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

    public async Task<MerchandiseAnalyticsDto> GetMerchandiseAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .ThenInclude(m => m.Channel)
                .Where(o => o.MerchandiseItem.Channel.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(o => o.OrderedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderedAt <= endDate.Value);

            var orders = await query.ToListAsync();
            var items = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .Where(m => m.Channel.UserId == userId)
                .ToListAsync();

            var analytics = new MerchandiseAnalyticsDto
            {
                TotalItems = items.Count,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.Amount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.Amount) : 0,
                PendingOrders = orders.Count(o => o.Status == MerchandiseOrderStatus.Pending),
                ShippedOrders = orders.Count(o => o.Status == MerchandiseOrderStatus.Shipped),
                DeliveredOrders = orders.Count(o => o.Status == MerchandiseOrderStatus.Delivered),
                TopSellingItems = orders
                    .GroupBy(o => new { o.MerchandiseItemId, o.MerchandiseItem.Name })
                    .Select(g => new MerchandiseItemAnalyticsDto
                    {
                        ItemId = g.Key.MerchandiseItemId,
                        ItemName = g.Key.Name,
                        TotalSales = g.Sum(o => o.Quantity),
                        TotalRevenue = g.Sum(o => o.Amount)
                    })
                    .OrderByDescending(x => x.TotalSales)
                    .Take(5)
                    .ToList(),
                RecentOrders = orders
                    .OrderByDescending(o => o.OrderedAt)
                    .Take(10)
                    .Select(o => new MerchandiseOrderAnalyticsDto
                    {
                        OrderId = o.Id,
                        CustomerName = o.CustomerName,
                        Amount = o.Amount,
                        Status = o.Status,
                        OrderedAt = o.OrderedAt
                    })
                    .ToList()
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchandise analytics for user {UserId}", userId);
            throw;
        }
    }

    // Public methods for customers to browse merchandise
    public async Task<List<MerchandiseDto>> GetChannelMerchandiseAsync(int channelId)
    {
        try
        {
            var merchandiseItems = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .Where(m => m.ChannelId == channelId && m.IsActive)
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
            _logger.LogError(ex, "Error retrieving merchandise for channel {ChannelId}", channelId);
            throw;
        }
    }

    public async Task<MerchandiseDto?> GetMerchandiseDetailsAsync(int merchandiseId)
    {
        try
        {
            var merchandise = await _context.MerchandiseItems
                .Include(m => m.Channel)
                .FirstOrDefaultAsync(m => m.Id == merchandiseId && m.IsActive);

            if (merchandise == null) return null;

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
                Sizes = !string.IsNullOrEmpty(merchandise.Sizes) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(merchandise.Sizes) ?? new List<string>() : new List<string>(),
                Colors = !string.IsNullOrEmpty(merchandise.Colors) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(merchandise.Colors) ?? new List<string>() : new List<string>(),
                CreatedAt = merchandise.CreatedAt,
                UpdatedAt = merchandise.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchandise details for {MerchandiseId}", merchandiseId);
            throw;
        }
    }

    public async Task<MerchandiseOrderDto?> GetMerchandiseOrderAsync(int orderId, string userId)
    {
        try
        {
            var order = await _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .ThenInclude(m => m.Channel)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MerchandiseItem.Channel.UserId == userId);

            if (order == null) return null;

            return new MerchandiseOrderDto
            {
                Id = order.Id,
                MerchandiseItemId = order.MerchandiseItemId,
                MerchandiseName = order.MerchandiseItem.Name,
                Quantity = order.Quantity,
                Size = order.Size,
                Color = order.Color,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerAddress = order.CustomerAddress,
                Status = order.Status,
                TrackingNumber = order.TrackingNumber,
                Notes = order.Notes,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving merchandise order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<MerchandiseOrderDto> UpdateMerchandiseOrderAsync(int orderId, MerchandiseOrderUpdateDto update, string userId)
    {
        try
        {
            var order = await _context.MerchandiseOrders
                .Include(o => o.MerchandiseItem)
                .ThenInclude(m => m.Channel)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.MerchandiseItem.Channel.UserId == userId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found or access denied");
            }

            order.Status = update.Status;
            order.TrackingNumber = update.TrackingNumber;
            order.Notes = update.Notes;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MerchandiseOrderDto
            {
                Id = order.Id,
                MerchandiseItemId = order.MerchandiseItemId,
                MerchandiseName = order.MerchandiseItem.Name,
                Quantity = order.Quantity,
                Size = order.Size,
                Color = order.Color,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerAddress = order.CustomerAddress,
                Status = order.Status,
                TrackingNumber = order.TrackingNumber,
                Notes = order.Notes,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise order {OrderId}", orderId);
            throw;
        }
    }

    // Ad Campaign Management Methods
    public async Task<List<AdCampaignDto>> GetAdCampaignsAsync(string userId)
    {
        try
        {
            var campaigns = await _context.AdCampaigns
                .Include(c => c.Advertiser)
                .Include(c => c.Impressions)
                .Where(c => c.AdvertiserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return campaigns.Select(c => new AdCampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                AdvertiserId = c.AdvertiserId,
                Type = c.Type,
                Budget = c.Budget,
                CostPerClick = c.CostPerClick,
                CostPerView = c.CostPerView,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                TargetAudience = c.TargetAudience,
                AdContent = c.AdContent,
                AdUrl = c.AdUrl,
                ThumbnailUrl = c.ThumbnailUrl,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Advertiser = c.Advertiser != null ? new UserDto
                {
                    Id = c.Advertiser.Id,
                    FirstName = c.Advertiser.FirstName,
                    LastName = c.Advertiser.LastName,
                    Email = c.Advertiser.Email
                } : null,
                Spent = c.Spent,
                TotalImpressions = c.TotalImpressions,
                TotalClicks = c.TotalClicks,
                TotalRevenue = c.TotalRevenue
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaigns for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AdCampaignDto?> GetAdCampaignAsync(int id, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .Include(c => c.Advertiser)
                .Include(c => c.Impressions)
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return null;

            return new AdCampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                AdvertiserId = campaign.AdvertiserId,
                Type = campaign.Type,
                Budget = campaign.Budget,
                CostPerClick = campaign.CostPerClick,
                CostPerView = campaign.CostPerView,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                TargetAudience = campaign.TargetAudience,
                AdContent = campaign.AdContent,
                AdUrl = campaign.AdUrl,
                ThumbnailUrl = campaign.ThumbnailUrl,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                Advertiser = campaign.Advertiser != null ? new UserDto
                {
                    Id = campaign.Advertiser.Id,
                    FirstName = campaign.Advertiser.FirstName,
                    LastName = campaign.Advertiser.LastName,
                    Email = campaign.Advertiser.Email
                } : null,
                Spent = campaign.Spent,
                TotalImpressions = campaign.TotalImpressions,
                TotalClicks = campaign.TotalClicks,
                TotalRevenue = campaign.TotalRevenue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaign {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<AdCampaignDto> CreateAdCampaignAsync(AdCampaignCreateDto createDto, string userId)
    {
        try
        {
            var campaign = new AdCampaign
            {
                Name = createDto.Name,
                Description = createDto.Description,
                AdvertiserId = userId,
                Type = createDto.Type,
                Budget = createDto.Budget,
                CostPerClick = createDto.CostPerClick,
                CostPerView = createDto.CostPerView,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Status = DTOs.AdStatus.Draft,
                TargetAudience = createDto.TargetAudience,
                AdContent = createDto.AdContent,
                AdUrl = createDto.AdUrl,
                ThumbnailUrl = createDto.ThumbnailUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.AdCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return new AdCampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                AdvertiserId = campaign.AdvertiserId,
                Type = campaign.Type,
                Budget = campaign.Budget,
                CostPerClick = campaign.CostPerClick,
                CostPerView = campaign.CostPerView,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                TargetAudience = campaign.TargetAudience,
                AdContent = campaign.AdContent,
                AdUrl = campaign.AdUrl,
                ThumbnailUrl = campaign.ThumbnailUrl,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                Spent = campaign.Spent,
                TotalImpressions = campaign.TotalImpressions,
                TotalClicks = campaign.TotalClicks,
                TotalRevenue = campaign.TotalRevenue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ad campaign for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AdCampaignDto?> UpdateAdCampaignAsync(int id, AdCampaignUpdateDto updateDto, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return null;

            if (!string.IsNullOrEmpty(updateDto.Name))
                campaign.Name = updateDto.Name;

            if (updateDto.Description != null)
                campaign.Description = updateDto.Description;

            if (updateDto.Type.HasValue)
                campaign.Type = updateDto.Type.Value;

            if (updateDto.Budget.HasValue)
                campaign.Budget = updateDto.Budget.Value;

            if (updateDto.CostPerClick.HasValue)
                campaign.CostPerClick = updateDto.CostPerClick.Value;

            if (updateDto.CostPerView.HasValue)
                campaign.CostPerView = updateDto.CostPerView.Value;

            if (updateDto.StartDate.HasValue)
                campaign.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                campaign.EndDate = updateDto.EndDate.Value;

            if (updateDto.Status.HasValue)
                campaign.Status = updateDto.Status.Value;

            if (updateDto.TargetAudience != null)
                campaign.TargetAudience = updateDto.TargetAudience;

            if (updateDto.AdContent != null)
                campaign.AdContent = updateDto.AdContent;

            if (updateDto.AdUrl != null)
                campaign.AdUrl = updateDto.AdUrl;

            if (updateDto.ThumbnailUrl != null)
                campaign.ThumbnailUrl = updateDto.ThumbnailUrl;

            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new AdCampaignDto
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                AdvertiserId = campaign.AdvertiserId,
                Type = campaign.Type,
                Budget = campaign.Budget,
                CostPerClick = campaign.CostPerClick,
                CostPerView = campaign.CostPerView,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status,
                TargetAudience = campaign.TargetAudience,
                AdContent = campaign.AdContent,
                AdUrl = campaign.AdUrl,
                ThumbnailUrl = campaign.ThumbnailUrl,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt,
                Spent = campaign.Spent,
                TotalImpressions = campaign.TotalImpressions,
                TotalClicks = campaign.TotalClicks,
                TotalRevenue = campaign.TotalRevenue
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ad campaign {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAdCampaignAsync(int id, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return false;

            _context.AdCampaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ad campaign {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> PauseAdCampaignAsync(int id, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return false;

            campaign.Status = DTOs.AdStatus.Paused;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing ad campaign {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> ResumeAdCampaignAsync(int id, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return false;

            campaign.Status = DTOs.AdStatus.Active;
            campaign.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming ad campaign {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<AdCampaignAnalyticsDto?> GetAdCampaignAnalyticsAsync(int id, string userId)
    {
        try
        {
            var campaign = await _context.AdCampaigns
                .FirstOrDefaultAsync(c => c.Id == id && c.AdvertiserId == userId);

            if (campaign == null) return null;

            var clickThroughRate = campaign.TotalImpressions > 0 ? (decimal)campaign.TotalClicks / campaign.TotalImpressions * 100 : 0;
            var costPerClick = campaign.TotalClicks > 0 ? campaign.Spent / campaign.TotalClicks : 0;
            var costPerImpression = campaign.TotalImpressions > 0 ? campaign.Spent / campaign.TotalImpressions : 0;
            var returnOnInvestment = campaign.Spent > 0 ? (campaign.TotalRevenue - campaign.Spent) / campaign.Spent * 100 : 0;

            return new AdCampaignAnalyticsDto
            {
                CampaignId = campaign.Id,
                CampaignName = campaign.Name,
                Spent = campaign.Spent,
                TotalImpressions = campaign.TotalImpressions,
                TotalClicks = campaign.TotalClicks,
                TotalRevenue = campaign.TotalRevenue,
                ClickThroughRate = clickThroughRate,
                CostPerClick = costPerClick,
                CostPerImpression = costPerImpression,
                ReturnOnInvestment = returnOnInvestment,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaign analytics {CampaignId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<List<AdDto>> GetDisplayAdsAsync(int? channelId, string userId)
    {
        try
        {
            // Get active ad campaigns that are currently running
            var activeCampaigns = await _context.AdCampaigns
                .Include(c => c.Impressions)
                .Where(c => c.Status == DTOs.AdStatus.Active && 
                           c.StartDate <= DateTime.UtcNow && 
                           c.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            var displayAds = new List<AdDto>();

            foreach (var campaign in activeCampaigns)
            {
                // Create display ads from campaign data
                var ad = new AdDto
                {
                    Id = campaign.Id,
                    Type = campaign.Type,
                    Content = campaign.AdContent,
                    Url = campaign.AdUrl,
                    ThumbnailUrl = campaign.ThumbnailUrl,
                    CostPerClick = campaign.CostPerClick,
                    CostPerView = campaign.CostPerView,
                    Duration = 30, // Default 30 seconds for display ads
                    SkipAfter = 5, // Default 5 seconds before skip
                    Position = null // Display ads don't have specific positions
                };

                displayAds.Add(ad);
            }

            return displayAds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving display ads for channel {ChannelId} and user {UserId}", channelId, userId);
            throw;
        }
    }
}
