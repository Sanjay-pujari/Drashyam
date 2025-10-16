using Drashyam.API.Models;
using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IVideoService
{
    Task<VideoDto> UploadVideoAsync(VideoUploadDto uploadDto, string userId);
    Task<VideoDto> GetVideoByIdAsync(int id);
    Task<VideoDto> GetVideoByShareTokenAsync(string shareToken);
    Task<PagedResult<VideoDto>> GetVideosAsync(VideoFilterDto filter);
    Task<PagedResult<VideoDto>> GetUserVideosAsync(string userId, VideoFilterDto filter);
    Task<PagedResult<VideoDto>> GetChannelVideosAsync(int channelId, VideoFilterDto filter);
    Task<VideoDto> UpdateVideoAsync(int id, VideoUpdateDto updateDto, string userId);
    Task DeleteVideoAsync(int id, string userId);
    Task<VideoDto> LikeVideoAsync(int videoId, string userId, Drashyam.API.DTOs.LikeType type);
    Task<VideoDto> UnlikeVideoAsync(int videoId, string userId);
    Task RecordVideoViewAsync(int videoId, string userId, TimeSpan watchDuration);
    Task<string> GenerateShareLinkAsync(int videoId, string userId);
    Task<PagedResult<VideoDto>> SearchVideosAsync(string query, VideoFilterDto filter);
    Task<PagedResult<VideoDto>> GetTrendingVideosAsync(VideoFilterDto filter);
    Task<PagedResult<VideoDto>> GetRecommendedVideosAsync(string userId, VideoFilterDto filter);
}
