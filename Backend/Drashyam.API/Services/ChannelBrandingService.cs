using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class ChannelBrandingService : IChannelBrandingService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ChannelBrandingService> _logger;
    private readonly IQuotaService _quotaService;

    public ChannelBrandingService(DrashyamDbContext context, IMapper mapper, ILogger<ChannelBrandingService> logger, IQuotaService quotaService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _quotaService = quotaService;
    }

    public async Task<ChannelBrandingDto> GetChannelBrandingAsync(int channelId)
    {
        var branding = await _context.ChannelBrandings
            .Include(cb => cb.Channel)
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId);

        if (branding == null)
        {
            // Return default branding if none exists
            return new ChannelBrandingDto
            {
                ChannelId = channelId,
                IsActive = false
            };
        }

        return _mapper.Map<ChannelBrandingDto>(branding);
    }

    public async Task<ChannelBrandingDto> CreateChannelBrandingAsync(ChannelBrandingCreateDto createDto, int channelId, string userId)
    {
        // Check if user has branding access
        if (!await CheckBrandingAccessAsync(channelId, userId))
        {
            throw new UnauthorizedAccessException("Branding features require a paid subscription");
        }

        // Check if channel belongs to user
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId && c.UserId == userId);

        if (channel == null)
        {
            throw new ArgumentException("Channel not found or access denied");
        }

        // Check if branding already exists
        var existingBranding = await _context.ChannelBrandings
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId);

        if (existingBranding != null)
        {
            throw new InvalidOperationException("Channel branding already exists. Use update instead.");
        }

        var branding = _mapper.Map<ChannelBranding>(createDto);
        branding.ChannelId = channelId;
        branding.CreatedAt = DateTime.UtcNow;

        _context.ChannelBrandings.Add(branding);
        await _context.SaveChangesAsync();

        return _mapper.Map<ChannelBrandingDto>(branding);
    }

    public async Task<ChannelBrandingDto> UpdateChannelBrandingAsync(int channelId, ChannelBrandingUpdateDto updateDto, string userId)
    {
        // Check if user has branding access
        if (!await CheckBrandingAccessAsync(channelId, userId))
        {
            throw new UnauthorizedAccessException("Branding features require a paid subscription");
        }

        var branding = await _context.ChannelBrandings
            .Include(cb => cb.Channel)
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId && cb.Channel.UserId == userId);

        if (branding == null)
        {
            throw new ArgumentException("Channel branding not found or access denied");
        }

        _mapper.Map(updateDto, branding);
        branding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<ChannelBrandingDto>(branding);
    }

    public async Task<bool> DeleteChannelBrandingAsync(int channelId, string userId)
    {
        var branding = await _context.ChannelBrandings
            .Include(cb => cb.Channel)
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId && cb.Channel.UserId == userId);

        if (branding == null)
            return false;

        _context.ChannelBrandings.Remove(branding);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateChannelBrandingAsync(int channelId, string userId)
    {
        // Check if user has branding access
        if (!await CheckBrandingAccessAsync(channelId, userId))
        {
            return false;
        }

        var branding = await _context.ChannelBrandings
            .Include(cb => cb.Channel)
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId && cb.Channel.UserId == userId);

        if (branding == null)
            return false;

        branding.IsActive = true;
        branding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateChannelBrandingAsync(int channelId, string userId)
    {
        var branding = await _context.ChannelBrandings
            .Include(cb => cb.Channel)
            .FirstOrDefaultAsync(cb => cb.ChannelId == channelId && cb.Channel.UserId == userId);

        if (branding == null)
            return false;

        branding.IsActive = false;
        branding.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckBrandingAccessAsync(int channelId, string userId)
    {
        try
        {
            // Check if user has branding features in their subscription
            return await _quotaService.CheckSubscriptionFeaturesAsync(userId, "branding");
        }
        catch
        {
            return false;
        }
    }
}
