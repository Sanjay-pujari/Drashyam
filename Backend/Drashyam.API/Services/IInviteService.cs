using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IInviteService
{
    Task<UserInviteDto> CreateInviteAsync(string inviterId, CreateInviteDto createDto);
    Task<UserInviteDto> GetInviteByTokenAsync(string token);
    Task<UserInviteDto> AcceptInviteAsync(string token, AcceptInviteDto acceptDto);
    Task<PagedResult<UserInviteDto>> GetUserInvitesAsync(string userId, int page = 1, int pageSize = 20);
    Task<PagedResult<UserInviteDto>> GetInvitesByEmailAsync(string email, int page = 1, int pageSize = 20);
    Task<bool> CancelInviteAsync(int inviteId, string userId);
    Task<bool> ResendInviteAsync(int inviteId, string userId);
    Task<InviteStatsDto> GetInviteStatsAsync(string userId);
    Task<InviteLinkDto> CreateInviteLinkAsync(string userId, int? maxUsage = null, DateTime? expiresAt = null);
    Task<bool> ValidateInviteTokenAsync(string token);
    Task<List<UserInviteDto>> BulkCreateInvitesAsync(string inviterId, BulkInviteDto bulkDto);
    Task<bool> ExpireInviteAsync(int inviteId);
}
