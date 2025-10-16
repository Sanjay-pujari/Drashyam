using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class LiveStreamService : ILiveStreamService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<LiveStreamService> _logger;

    public LiveStreamService(DrashyamDbContext context, IMapper mapper, ILogger<LiveStreamService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDto> CreateLiveStreamAsync(LiveStreamCreateDto createDto, string userId)
    {
        var stream = _mapper.Map<LiveStream>(createDto);
        stream.UserId = userId;
        stream.Status = Models.LiveStreamStatus.Scheduled;
        stream.StreamKey = GenerateStreamKey();
        stream.CreatedAt = DateTime.UtcNow;

        _context.LiveStreams.Add(stream);
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<LiveStreamDto> GetLiveStreamByIdAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .Include(s => s.User)
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Live stream not found");

        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<LiveStreamDto> GetLiveStreamByStreamKeyAsync(string streamKey)
    {
        var stream = await _context.LiveStreams
            .Include(s => s.User)
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.StreamKey == streamKey);

        if (stream == null)
            throw new ArgumentException("Live stream not found");

        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<LiveStreamDto> UpdateLiveStreamAsync(int streamId, LiveStreamUpdateDto updateDto, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId && s.UserId == userId);

        if (stream == null)
            throw new ArgumentException("Live stream not found or access denied");

        _mapper.Map(updateDto, stream);
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<bool> DeleteLiveStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId && s.UserId == userId);

        if (stream == null)
            return false;

        _context.LiveStreams.Remove(stream);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<LiveStreamDto>> GetActiveLiveStreamsAsync(int page = 1, int pageSize = 20)
    {
        var streams = await _context.LiveStreams
            .Where(s => s.Status == Models.LiveStreamStatus.Live)
            .Include(s => s.User)
            .Include(s => s.Channel)
            .OrderByDescending(s => s.ActualStartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.LiveStreams
            .Where(s => s.Status == Models.LiveStreamStatus.Live)
            .CountAsync();

        return new PagedResult<LiveStreamDto>
        {
            Items = _mapper.Map<List<LiveStreamDto>>(streams),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LiveStreamDto>> GetUserLiveStreamsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var streams = await _context.LiveStreams
            .Where(s => s.UserId == userId)
            .Include(s => s.User)
            .Include(s => s.Channel)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.LiveStreams
            .Where(s => s.UserId == userId)
            .CountAsync();

        return new PagedResult<LiveStreamDto>
        {
            Items = _mapper.Map<List<LiveStreamDto>>(streams),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LiveStreamDto> StartLiveStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId && s.UserId == userId);

        if (stream == null)
            throw new ArgumentException("Live stream not found or access denied");

        stream.Status = Models.LiveStreamStatus.Live;
        stream.ActualStartTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<LiveStreamDto> StopLiveStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId && s.UserId == userId);

        if (stream == null)
            throw new ArgumentException("Live stream not found or access denied");

        stream.Status = Models.LiveStreamStatus.Ended;
        stream.EndTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<string> GenerateStreamKeyAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId && s.UserId == userId);

        if (stream == null)
            throw new ArgumentException("Live stream not found or access denied");

        stream.StreamKey = GenerateStreamKey();
        await _context.SaveChangesAsync();

        return stream.StreamKey;
    }

    public async Task<bool> ValidateStreamKeyAsync(string streamKey)
    {
        return await _context.LiveStreams
            .AnyAsync(s => s.StreamKey == streamKey);
    }

    public async Task<PagedResult<LiveStreamDto>> GetChannelLiveStreamsAsync(int channelId, int page = 1, int pageSize = 20)
    {
        var streams = await _context.LiveStreams
            .Where(s => s.ChannelId == channelId)
            .Include(s => s.User)
            .Include(s => s.Channel)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.LiveStreams
            .Where(s => s.ChannelId == channelId)
            .CountAsync();

        return new PagedResult<LiveStreamDto>
        {
            Items = _mapper.Map<List<LiveStreamDto>>(streams),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LiveStreamDto> JoinLiveStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams.FindAsync(streamId);
        if (stream == null)
            throw new ArgumentException("Live stream not found");

        // Add viewer count logic here
        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<LiveStreamDto> LeaveLiveStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams.FindAsync(streamId);
        if (stream == null)
            throw new ArgumentException("Live stream not found");

        // Remove viewer count logic here
        return _mapper.Map<LiveStreamDto>(stream);
    }

    public async Task<int> GetLiveStreamViewerCountAsync(int streamId)
    {
        // Get viewer count logic here
        return 0;
    }

    private string GenerateStreamKey()
    {
        return Guid.NewGuid().ToString("N");
    }
}
