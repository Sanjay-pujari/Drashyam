using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IChannelService
{
    Task<ChannelDto> CreateChannelAsync(ChannelCreateDto createDto, string userId);
    Task<ChannelDto> GetChannelByIdAsync(int channelId);
    Task<ChannelDto> GetChannelByCustomUrlAsync(string customUrl);
    Task<ChannelDto> UpdateChannelAsync(int channelId, ChannelUpdateDto updateDto, string userId);
    Task<bool> DeleteChannelAsync(int channelId, string userId);
    Task<PagedResult<ChannelDto>> GetChannelsAsync(int page = 1, int pageSize = 20);
    Task<PagedResult<ChannelDto>> GetUserChannelsAsync(string userId, int page = 1, int pageSize = 20);
    Task<PagedResult<ChannelDto>> SearchChannelsAsync(string query, int page = 1, int pageSize = 20);
    Task<ChannelDto> SubscribeToChannelAsync(int channelId, string userId);
    Task<ChannelDto> UnsubscribeFromChannelAsync(int channelId, string userId);
    Task<bool> IsSubscribedAsync(int channelId, string userId);
    Task<PagedResult<ChannelDto>> GetSubscribedChannelsAsync(string userId, int page = 1, int pageSize = 20);
    Task<PagedResult<UserDto>> GetChannelSubscribersAsync(int channelId, int page = 1, int pageSize = 20);
    Task<ChannelDto> UpdateChannelBannerAsync(int channelId, IFormFile bannerFile, string userId);
    Task<ChannelDto> UpdateChannelProfilePictureAsync(int channelId, IFormFile profilePicture, string userId);
    Task UpdateNotificationPreferenceAsync(int channelId, string userId, bool notificationsEnabled);
    Task<bool> GetNotificationPreferenceAsync(int channelId, string userId);
}
