using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ILiveStreamService
{
    Task<LiveStreamDto> CreateLiveStreamAsync(LiveStreamCreateDto createDto, string userId);
    Task<LiveStreamDto> GetLiveStreamByIdAsync(int streamId);
    Task<LiveStreamDto> GetLiveStreamByStreamKeyAsync(string streamKey);
    Task<LiveStreamDto> UpdateLiveStreamAsync(int streamId, LiveStreamUpdateDto updateDto, string userId);
    Task<bool> DeleteLiveStreamAsync(int streamId, string userId);
    Task<PagedResult<LiveStreamDto>> GetActiveLiveStreamsAsync(int page = 1, int pageSize = 20);
    Task<PagedResult<LiveStreamDto>> GetUserLiveStreamsAsync(string userId, int page = 1, int pageSize = 20);
    Task<LiveStreamDto> StartLiveStreamAsync(int streamId, string userId);
    Task<LiveStreamDto> StopLiveStreamAsync(int streamId, string userId);
    Task<string> GenerateStreamKeyAsync(int streamId, string userId);
    Task<bool> ValidateStreamKeyAsync(string streamKey);
    Task<PagedResult<LiveStreamDto>> GetChannelLiveStreamsAsync(int channelId, int page = 1, int pageSize = 20);
    Task<LiveStreamDto> JoinLiveStreamAsync(int streamId, string userId);
    Task<LiveStreamDto> LeaveLiveStreamAsync(int streamId, string userId);
    Task<int> GetLiveStreamViewerCountAsync(int streamId);
}
