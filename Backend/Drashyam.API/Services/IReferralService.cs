using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public interface IReferralService
{
    Task<ReferralDto> CreateReferralAsync(string referrerId, CreateReferralDto createDto);
    Task<ReferralDto> GetReferralByIdAsync(int referralId);
    Task<PagedResult<ReferralDto>> GetUserReferralsAsync(string userId, int page = 1, int pageSize = 20);
    Task<PagedResult<ReferralDto>> GetReferralsByUserAsync(string userId, int page = 1, int pageSize = 20);
    Task<ReferralStatsDto> GetReferralStatsAsync(string userId);
    Task<ReferralCodeDto> CreateReferralCodeAsync(string userId, CreateReferralCodeDto createDto);
    Task<ReferralCodeDto> GetReferralCodeAsync(string userId);
    Task<bool> ValidateReferralCodeAsync(string code);
    Task<ReferralRewardDto> ClaimRewardAsync(string userId, ClaimRewardDto claimDto);
    Task<PagedResult<ReferralRewardDto>> GetUserRewardsAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> ProcessReferralRewardAsync(int referralId);
    Task<bool> UpdateReferralStatusAsync(int referralId, ReferralStatus status);
}
