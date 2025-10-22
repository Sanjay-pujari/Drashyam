using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IChannelBrandingService
{
    Task<ChannelBrandingDto> GetChannelBrandingAsync(int channelId);
    Task<ChannelBrandingDto> CreateChannelBrandingAsync(ChannelBrandingCreateDto createDto, int channelId, string userId);
    Task<ChannelBrandingDto> UpdateChannelBrandingAsync(int channelId, ChannelBrandingUpdateDto updateDto, string userId);
    Task<bool> DeleteChannelBrandingAsync(int channelId, string userId);
    Task<bool> ActivateChannelBrandingAsync(int channelId, string userId);
    Task<bool> DeactivateChannelBrandingAsync(int channelId, string userId);
    Task<bool> CheckBrandingAccessAsync(int channelId, string userId);
}
