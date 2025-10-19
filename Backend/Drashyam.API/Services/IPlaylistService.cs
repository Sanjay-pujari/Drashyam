using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IPlaylistService
{
    Task<PagedResult<PlaylistDto>> GetUserPlaylistsAsync(string userId, int page = 1, int pageSize = 20);
    Task<PlaylistDto> CreatePlaylistAsync(string userId, PlaylistCreateDto createDto);
    Task<PlaylistDto> UpdatePlaylistAsync(int playlistId, string userId, PlaylistUpdateDto updateDto);
    Task<bool> DeletePlaylistAsync(int playlistId, string userId);
    Task<PlaylistDto> GetPlaylistByIdAsync(int playlistId, string userId);
    Task<PagedResult<PlaylistVideoDto>> GetPlaylistVideosAsync(int playlistId, string userId, int page = 1, int pageSize = 20);
    Task<PlaylistVideoDto> AddVideoToPlaylistAsync(int playlistId, string userId, PlaylistVideoCreateDto createDto);
    Task<bool> RemoveVideoFromPlaylistAsync(int playlistId, int videoId, string userId);
    Task<bool> ReorderPlaylistVideosAsync(int playlistId, string userId, List<PlaylistVideoUpdateDto> updates);
}
