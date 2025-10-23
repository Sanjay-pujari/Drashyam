using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class PlaylistService : IPlaylistService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<PlaylistService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<PlaylistDto>> GetUserPlaylistsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.Playlists
            .Include(p => p.User)
            .Include(p => p.Channel)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PlaylistDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                UserId = p.UserId,
                UserName = p.User.FirstName + " " + p.User.LastName,
                ChannelId = p.ChannelId,
                ChannelName = p.Channel != null ? p.Channel.Name : null,
                Visibility = (DTOs.PlaylistVisibility)p.Visibility,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                VideoCount = p.VideoCount,
                ThumbnailUrl = p.ThumbnailUrl
            })
            .ToListAsync();

        return new PagedResult<PlaylistDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PlaylistDto> CreatePlaylistAsync(string userId, PlaylistCreateDto createDto)
    {
        var playlist = new Playlist
        {
            Name = createDto.Name,
            Description = createDto.Description,
            UserId = userId,
            ChannelId = createDto.ChannelId,
            Visibility = (DTOs.PlaylistVisibility)createDto.Visibility,
            CreatedAt = DateTime.UtcNow,
            VideoCount = 0
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<PlaylistDto> UpdatePlaylistAsync(int playlistId, string userId, PlaylistUpdateDto updateDto)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            throw new ArgumentException("Playlist not found or access denied");
        }

        playlist.Name = updateDto.Name;
        playlist.Description = updateDto.Description;
        playlist.Visibility = (DTOs.PlaylistVisibility)updateDto.Visibility;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<bool> DeletePlaylistAsync(int playlistId, string userId)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            return false;
        }

        // Remove all playlist videos first
        var playlistVideos = await _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .ToListAsync();

        _context.PlaylistVideos.RemoveRange(playlistVideos);
        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PlaylistDto> GetPlaylistByIdAsync(int playlistId, string userId)
    {
        var playlist = await _context.Playlists
            .Include(p => p.User)
            .Include(p => p.Channel)
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            throw new ArgumentException("Playlist not found or access denied");
        }

        return _mapper.Map<PlaylistDto>(playlist);
    }

    public async Task<PagedResult<PlaylistVideoDto>> GetPlaylistVideosAsync(int playlistId, string userId, int page = 1, int pageSize = 20)
    {
        // Verify playlist ownership
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            throw new ArgumentException("Playlist not found or access denied");
        }

        var query = _context.PlaylistVideos
            .Include(pv => pv.Video)
                .ThenInclude(v => v.User)
            .Include(pv => pv.Video)
                .ThenInclude(v => v.Channel)
            .Where(pv => pv.PlaylistId == playlistId)
            .OrderBy(pv => pv.Order);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pv => new PlaylistVideoDto
            {
                Id = pv.Id,
                PlaylistId = pv.PlaylistId,
                VideoId = pv.VideoId,
                Order = pv.Order,
                AddedAt = pv.AddedAt,
                VideoTitle = pv.Video.Title,
                VideoThumbnailUrl = pv.Video.ThumbnailUrl,
                VideoDuration = pv.Video.Duration.ToString(@"hh\:mm\:ss"),
                VideoViewCount = pv.Video.ViewCount,
                ChannelName = pv.Video.Channel != null ? pv.Video.Channel.Name : 
                             (pv.Video.User.FirstName + " " + pv.Video.User.LastName)
            })
            .ToListAsync();

        return new PagedResult<PlaylistVideoDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PlaylistVideoDto> AddVideoToPlaylistAsync(int playlistId, string userId, PlaylistVideoCreateDto createDto)
    {
        // Verify playlist ownership
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            throw new ArgumentException("Playlist not found or access denied");
        }

        // Check if video already exists in playlist
        var existing = await _context.PlaylistVideos
            .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == createDto.VideoId);

        if (existing != null)
        {
            throw new InvalidOperationException("Video is already in playlist");
        }

        // Check if video exists
        var video = await _context.Videos
            .Include(v => v.User)
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == createDto.VideoId);

        if (video == null)
        {
            throw new ArgumentException("Video not found");
        }

        // Get next order number
        var maxOrder = await _context.PlaylistVideos
            .Where(pv => pv.PlaylistId == playlistId)
            .MaxAsync(pv => (int?)pv.Order) ?? 0;

        var playlistVideo = new PlaylistVideo
        {
            PlaylistId = playlistId,
            VideoId = createDto.VideoId,
            Order = createDto.Order ?? (maxOrder + 1),
            AddedAt = DateTime.UtcNow
        };

        _context.PlaylistVideos.Add(playlistVideo);
        
        // Update playlist video count
        playlist.VideoCount++;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new PlaylistVideoDto
        {
            Id = playlistVideo.Id,
            PlaylistId = playlistVideo.PlaylistId,
            VideoId = playlistVideo.VideoId,
            Order = playlistVideo.Order,
            AddedAt = playlistVideo.AddedAt,
            VideoTitle = video.Title,
            VideoThumbnailUrl = video.ThumbnailUrl,
            VideoDuration = video.Duration.ToString(@"hh\:mm\:ss"),
            VideoViewCount = video.ViewCount,
            ChannelName = video.Channel != null ? video.Channel.Name : 
                         (video.User.FirstName + " " + video.User.LastName)
        };
    }

    public async Task<bool> RemoveVideoFromPlaylistAsync(int playlistId, int videoId, string userId)
    {
        // Verify playlist ownership
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            return false;
        }

        var playlistVideo = await _context.PlaylistVideos
            .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == videoId);

        if (playlistVideo == null)
        {
            return false;
        }

        _context.PlaylistVideos.Remove(playlistVideo);
        
        // Update playlist video count
        playlist.VideoCount--;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderPlaylistVideosAsync(int playlistId, string userId, List<PlaylistVideoUpdateDto> updates)
    {
        // Verify playlist ownership
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

        if (playlist == null)
        {
            return false;
        }

        foreach (var update in updates)
        {
            var playlistVideo = await _context.PlaylistVideos
                .FirstOrDefaultAsync(pv => pv.PlaylistId == playlistId && pv.VideoId == update.VideoId);

            if (playlistVideo != null)
            {
                playlistVideo.Order = update.Order;
            }
        }

        playlist.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
