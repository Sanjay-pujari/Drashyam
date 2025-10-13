using Drashyam.API.DTOs;

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
    Task<List<MerchandiseDto>> GetMerchandiseAsync(string userId);
    Task<MerchandiseDto> CreateMerchandiseAsync(MerchandiseRequestDto request, string userId);
    Task<MerchandiseDto> UpdateMerchandiseAsync(int merchandiseId, MerchandiseRequestDto request, string userId);
    Task DeleteMerchandiseAsync(int merchandiseId, string userId);
    Task<RevenueReportDto> GetRevenueReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task ProcessAdRevenueAsync(int videoId, decimal amount, string adType);
    Task ProcessSponsorRevenueAsync(int sponsorId, decimal amount);
    Task ProcessDonationRevenueAsync(int donationId, decimal amount);
    Task ProcessMerchandiseRevenueAsync(int merchandiseId, decimal amount);
}
