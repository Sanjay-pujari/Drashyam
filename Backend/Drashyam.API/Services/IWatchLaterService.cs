using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IWatchLaterService
{
    Task<PagedResult<WatchLaterResponseDto>> GetUserWatchLaterAsync(string userId, int page = 1, int pageSize = 20);
    Task<WatchLaterResponseDto> AddToWatchLaterAsync(string userId, WatchLaterCreateDto createDto);
    Task<bool> RemoveFromWatchLaterAsync(string userId, int videoId);
    Task<bool> IsVideoInWatchLaterAsync(string userId, int videoId);
    Task ClearWatchLaterAsync(string userId);
    Task<int> GetWatchLaterCountAsync(string userId);
}
