using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public interface IMonetizationService
{
    Task<MonetizationStatusDto> GetMonetizationStatusAsync(string userId);
    Task<MonetizationStatusDto> EnableMonetizationAsync(string userId, MonetizationRequestDto request);
    Task<MonetizationStatusDto> DisableMonetizationAsync(string userId);
    Task<List<AdPlacementDto>> GetAdPlacementsAsync(int videoId, string userId);
    Task<AdPlacementDto> CreateAdPlacementAsync(int videoId, AdPlacementRequestDto request, string userId);
    Task<AdPlacementDto> UpdateAdPlacementAsync(int placementId, AdPlacementRequestDto request, string userId);
    Task DeleteAdPlacementAsync(int placementId, string userId);
    Task<List<SponsorDto>> GetSponsorsAsync(string userId);
    Task<SponsorDto> CreateSponsorAsync(SponsorRequestDto request, string userId);
    Task<SponsorDto> UpdateSponsorAsync(int sponsorId, SponsorRequestDto request, string userId);
    Task DeleteSponsorAsync(int sponsorId, string userId);
    Task<List<DonationDto>> GetDonationsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<DonationDto> ProcessDonationAsync(DonationRequestDto request);
    Task<List<MerchandiseDto>> GetMerchandiseAsync(MerchandiseFilterDto filter);
    Task<MerchandiseDto> CreateMerchandiseAsync(MerchandiseRequestDto request, string userId);
    Task<MerchandiseDto> UpdateMerchandiseAsync(int merchandiseId, MerchandiseRequestDto request, string userId);
    Task DeleteMerchandiseAsync(int merchandiseId, string userId);
    
    // Public methods for customers to browse merchandise
    Task<List<MerchandiseDto>> GetChannelMerchandiseAsync(int channelId);
    Task<MerchandiseDto?> GetMerchandiseDetailsAsync(int merchandiseId);
    Task<RevenueReportDto> GetRevenueReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task ProcessAdRevenueAsync(int videoId, decimal amount, string adType);
    
    // Merchandise Order Management
    Task<MerchandiseOrderDto?> GetMerchandiseOrderAsync(int orderId, string userId);
    Task<MerchandiseOrderDto> UpdateMerchandiseOrderAsync(int orderId, MerchandiseOrderUpdateDto update, string userId);
    Task ProcessSponsorRevenueAsync(int sponsorId, decimal amount);
    Task ProcessDonationRevenueAsync(int donationId, decimal amount);
    Task ProcessMerchandiseRevenueAsync(int merchandiseId, decimal amount);
    
    // Merchandise Order Methods
    Task<List<MerchandiseOrderDto>> GetMerchandiseOrdersAsync(string userId, int page = 1, int pageSize = 20);
    Task<MerchandiseOrderDto> CreateMerchandiseOrderAsync(MerchandiseOrderRequestDto request, string customerId);
    Task<MerchandiseOrderDto> UpdateMerchandiseOrderStatusAsync(int orderId, MerchandiseOrderStatus status, string userId, string? trackingNumber = null);
    Task<List<MerchandiseOrderDto>> GetCustomerOrdersAsync(string customerId, int page = 1, int pageSize = 20);
    Task<MerchandiseAnalyticsDto> GetMerchandiseAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}
