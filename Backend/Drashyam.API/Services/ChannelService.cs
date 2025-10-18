using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class ChannelService : IChannelService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<ChannelService> _logger;

    public ChannelService(
        DrashyamDbContext context,
        IMapper mapper,
        IFileStorageService fileStorage,
        ILogger<ChannelService> logger)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ChannelDto> CreateChannelAsync(ChannelCreateDto createDto, string userId)
    {
        var channel = _mapper.Map<Channel>(createDto);
        channel.UserId = userId;
        channel.CreatedAt = DateTime.UtcNow;

        _context.Channels.Add(channel);
        await _context.SaveChangesAsync();

        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> GetChannelByIdAsync(int channelId)
    {
        var channel = await _context.Channels
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null)
            throw new ArgumentException("Channel not found");

        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> GetChannelByCustomUrlAsync(string customUrl)
    {
        var channel = await _context.Channels
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CustomUrl == customUrl);

        if (channel == null)
            throw new ArgumentException("Channel not found");

        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> UpdateChannelAsync(int channelId, ChannelUpdateDto updateDto, string userId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.UserId == userId);

        if (channel == null)
            throw new ArgumentException("Channel not found or access denied");

        _mapper.Map(updateDto, channel);
        await _context.SaveChangesAsync();

        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<bool> DeleteChannelAsync(int channelId, string userId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.UserId == userId);

        if (channel == null)
            return false;

        _context.Channels.Remove(channel);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<ChannelDto>> GetChannelsAsync(int page = 1, int pageSize = 20)
    {
        var channels = await _context.Channels
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Channels.CountAsync();

        return new PagedResult<ChannelDto>
        {
            Items = _mapper.Map<List<ChannelDto>>(channels),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ChannelDto>> GetUserChannelsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var channels = await _context.Channels
            .Where(c => c.UserId == userId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Channels
            .Where(c => c.UserId == userId)
            .CountAsync();

        return new PagedResult<ChannelDto>
        {
            Items = _mapper.Map<List<ChannelDto>>(channels),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ChannelDto>> SearchChannelsAsync(string query, int page = 1, int pageSize = 20)
    {
        var channels = await _context.Channels
            .Where(c => c.Name.Contains(query) || c.Description.Contains(query))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Channels
            .Where(c => c.Name.Contains(query) || c.Description.Contains(query))
            .CountAsync();

        return new PagedResult<ChannelDto>
        {
            Items = _mapper.Map<List<ChannelDto>>(channels),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ChannelDto> SubscribeToChannelAsync(int channelId, string userId)
    {
        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null)
            throw new ArgumentException("Channel not found");

        // Add subscription logic here
        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> UnsubscribeFromChannelAsync(int channelId, string userId)
    {
        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null)
            throw new ArgumentException("Channel not found");

        // Remove subscription logic here
        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<bool> IsSubscribedAsync(int channelId, string userId)
    {
        // Check subscription logic here
        return false;
    }

    public async Task<PagedResult<ChannelDto>> GetSubscribedChannelsAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Get subscribed channels logic here
        return new PagedResult<ChannelDto>
        {
            Items = new List<ChannelDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<UserDto>> GetChannelSubscribersAsync(int channelId, int page = 1, int pageSize = 20)
    {
        // Get channel subscribers logic here
        return new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ChannelDto> UpdateChannelBannerAsync(int channelId, IFormFile bannerFile, string userId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.UserId == userId);

        if (channel == null)
            throw new ArgumentException("Channel not found or access denied");

        var imageUrl = await _fileStorage.UploadBannerAsync(bannerFile);
        channel.BannerUrl = imageUrl;
        await _context.SaveChangesAsync();

        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> UpdateChannelProfilePictureAsync(int channelId, IFormFile profilePicture, string userId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.UserId == userId);

        if (channel == null)
            throw new ArgumentException("Channel not found or access denied");

        var imageUrl = await _fileStorage.UploadProfilePictureAsync(profilePicture);
        channel.ProfilePictureUrl = imageUrl;
        await _context.SaveChangesAsync();

        return _mapper.Map<ChannelDto>(channel);
    }
}
