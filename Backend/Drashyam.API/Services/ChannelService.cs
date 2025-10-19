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

        var existingSubscription = await _context.ChannelSubscriptions
            .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.UserId == userId);

        if (existingSubscription != null)
        {
            if (existingSubscription.IsActive)
                throw new InvalidOperationException("Already subscribed to this channel");
            
            existingSubscription.IsActive = true;
            existingSubscription.SubscribedAt = DateTime.UtcNow;
        }
        else
        {
            var subscription = new ChannelSubscription
            {
                UserId = userId,
                ChannelId = channelId,
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            };
            _context.ChannelSubscriptions.Add(subscription);
        }

        await _context.SaveChangesAsync();
        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<ChannelDto> UnsubscribeFromChannelAsync(int channelId, string userId)
    {
        var subscription = await _context.ChannelSubscriptions
            .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.UserId == userId);

        if (subscription == null)
            throw new ArgumentException("Subscription not found");

        subscription.IsActive = false;
        await _context.SaveChangesAsync();

        var channel = await _context.Channels.FindAsync(channelId);
        return _mapper.Map<ChannelDto>(channel);
    }

    public async Task<bool> IsSubscribedAsync(int channelId, string userId)
    {
        return await _context.ChannelSubscriptions
            .AnyAsync(cs => cs.ChannelId == channelId && cs.UserId == userId && cs.IsActive);
    }

    public async Task<PagedResult<ChannelDto>> GetSubscribedChannelsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.ChannelSubscriptions
            .Where(cs => cs.UserId == userId && cs.IsActive)
            .Include(cs => cs.Channel)
            .Select(cs => cs.Channel);

        var totalCount = await query.CountAsync();
        var channels = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var channelDtos = _mapper.Map<List<ChannelDto>>(channels);

        return new PagedResult<ChannelDto>
        {
            Items = channelDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<UserDto>> GetChannelSubscribersAsync(int channelId, int page = 1, int pageSize = 20)
    {
        var query = _context.ChannelSubscriptions
            .Where(cs => cs.ChannelId == channelId && cs.IsActive)
            .Include(cs => cs.User)
            .Select(cs => cs.User);

        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = _mapper.Map<List<UserDto>>(users);

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
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

    public async Task UpdateNotificationPreferenceAsync(int channelId, string userId, bool notificationsEnabled)
    {
        var subscription = await _context.ChannelSubscriptions
            .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.UserId == userId && cs.IsActive);

        if (subscription == null)
            throw new ArgumentException("Subscription not found");

        subscription.NotificationsEnabled = notificationsEnabled;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> GetNotificationPreferenceAsync(int channelId, string userId)
    {
        var subscription = await _context.ChannelSubscriptions
            .FirstOrDefaultAsync(cs => cs.ChannelId == channelId && cs.UserId == userId && cs.IsActive);

        if (subscription == null)
            throw new ArgumentException("Subscription not found");

        return subscription.NotificationsEnabled;
    }

}
