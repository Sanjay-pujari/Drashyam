using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class LiveStreamChatService : ILiveStreamChatService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<LiveStreamChatService> _logger;

    public LiveStreamChatService(DrashyamDbContext context, IMapper mapper, ILogger<LiveStreamChatService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamChatDto> SendMessageAsync(SendChatMessageDto dto, string userId)
    {
        var chatMessage = new LiveStreamChat
        {
            LiveStreamId = dto.LiveStreamId,
            UserId = userId,
            Message = dto.Message,
            Type = dto.Type,
            Emoji = dto.Emoji,
            Timestamp = DateTime.UtcNow
        };

        _context.LiveStreamChats.Add(chatMessage);
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamChats
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == chatMessage.Id);

        return _mapper.Map<LiveStreamChatDto>(result);
    }

    public async Task<List<LiveStreamChatDto>> GetChatMessagesAsync(int liveStreamId, int page = 1, int pageSize = 50)
    {
        var messages = await _context.LiveStreamChats
            .Include(c => c.User)
            .Where(c => c.LiveStreamId == liveStreamId && !c.IsDeleted)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamChatDto>>(messages);
    }

    public async Task<LiveStreamChatDto> DeleteMessageAsync(int messageId, string userId)
    {
        var message = await _context.LiveStreamChats
            .FirstOrDefaultAsync(c => c.Id == messageId && c.UserId == userId);

        if (message == null)
            throw new ArgumentException("Message not found or you don't have permission to delete it");

        message.IsDeleted = true;
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamChatDto>(message);
    }

    public async Task<LiveStreamChatDto> PinMessageAsync(int messageId, string userId)
    {
        var message = await _context.LiveStreamChats
            .FirstOrDefaultAsync(c => c.Id == messageId);

        if (message == null)
            throw new ArgumentException("Message not found");

        message.IsPinned = true;
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamChatDto>(message);
    }

    public async Task<LiveStreamChatDto> UnpinMessageAsync(int messageId, string userId)
    {
        var message = await _context.LiveStreamChats
            .FirstOrDefaultAsync(c => c.Id == messageId);

        if (message == null)
            throw new ArgumentException("Message not found");

        message.IsPinned = false;
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamChatDto>(message);
    }

    public async Task<LiveStreamReactionDto> SendReactionAsync(SendReactionDto dto, string userId)
    {
        var reaction = new LiveStreamReaction
        {
            LiveStreamId = dto.LiveStreamId,
            UserId = userId,
            Type = dto.Type,
            Timestamp = DateTime.UtcNow
        };

        _context.LiveStreamReactions.Add(reaction);
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamReactions
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reaction.Id);

        return _mapper.Map<LiveStreamReactionDto>(result);
    }

    public async Task<List<LiveStreamReactionDto>> GetReactionsAsync(int liveStreamId, int page = 1, int pageSize = 50)
    {
        var reactions = await _context.LiveStreamReactions
            .Include(r => r.User)
            .Where(r => r.LiveStreamId == liveStreamId)
            .OrderByDescending(r => r.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamReactionDto>>(reactions);
    }

    public async Task<LiveStreamPollDto> CreatePollAsync(CreatePollDto dto, string userId)
    {
        var poll = new LiveStreamPoll
        {
            LiveStreamId = dto.LiveStreamId,
            Question = dto.Question,
            Description = dto.Description,
            AllowMultipleChoices = dto.AllowMultipleChoices,
            CreatedAt = DateTime.UtcNow
        };

        _context.LiveStreamPolls.Add(poll);
        await _context.SaveChangesAsync();

        // Add poll options
        for (int i = 0; i < dto.Options.Count; i++)
        {
            var option = new LiveStreamPollOption
            {
                PollId = poll.Id,
                Text = dto.Options[i],
                Order = i
            };
            _context.LiveStreamPollOptions.Add(option);
        }

        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == poll.Id);

        return _mapper.Map<LiveStreamPollDto>(result);
    }

    public async Task<LiveStreamPollDto> GetPollAsync(int pollId)
    {
        var poll = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new ArgumentException("Poll not found");

        return _mapper.Map<LiveStreamPollDto>(poll);
    }

    public async Task<LiveStreamPollDto> VotePollAsync(VotePollDto dto, string userId)
    {
        var poll = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == dto.PollId);

        if (poll == null)
            throw new ArgumentException("Poll not found");

        if (!poll.IsActive)
            throw new InvalidOperationException("Poll is not active");

        // Check if user already voted
        var existingVote = await _context.LiveStreamPollVotes
            .FirstOrDefaultAsync(v => v.PollId == dto.PollId && v.UserId == userId);

        if (existingVote != null)
            throw new InvalidOperationException("You have already voted on this poll");

        // Add votes
        foreach (var optionId in dto.OptionIds)
        {
            var option = poll.Options.FirstOrDefault(o => o.Id == optionId);
            if (option == null)
                continue;

            var vote = new LiveStreamPollVote
            {
                PollId = dto.PollId,
                OptionId = optionId,
                UserId = userId,
                VotedAt = DateTime.UtcNow
            };

            _context.LiveStreamPollVotes.Add(vote);
            option.VoteCount++;
        }

        await _context.SaveChangesAsync();

        var updatedPoll = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == dto.PollId);

        return _mapper.Map<LiveStreamPollDto>(updatedPoll);
    }

    public async Task<LiveStreamPollDto> EndPollAsync(int pollId, string userId)
    {
        var poll = await _context.LiveStreamPolls
            .FirstOrDefaultAsync(p => p.Id == pollId);

        if (poll == null)
            throw new ArgumentException("Poll not found");

        poll.IsActive = false;
        poll.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .FirstOrDefaultAsync(p => p.Id == pollId);

        return _mapper.Map<LiveStreamPollDto>(result);
    }

    public async Task<List<LiveStreamPollDto>> GetPollsAsync(int liveStreamId, int page = 1, int pageSize = 20)
    {
        var polls = await _context.LiveStreamPolls
            .Include(p => p.Options)
            .Where(p => p.LiveStreamId == liveStreamId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamPollDto>>(polls);
    }

    public async Task<LiveStreamAnalyticsDto> GetAnalyticsAsync(int liveStreamId)
    {
        var analytics = await _context.LiveStreamAnalytics
            .Where(a => a.LiveStreamId == liveStreamId)
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefaultAsync();

        if (analytics == null)
        {
            // Create default analytics if none exist
            analytics = new LiveStreamAnalytics
            {
                LiveStreamId = liveStreamId,
                Timestamp = DateTime.UtcNow
            };
            _context.LiveStreamAnalytics.Add(analytics);
            await _context.SaveChangesAsync();
        }

        return _mapper.Map<LiveStreamAnalyticsDto>(analytics);
    }

    public async Task<List<LiveStreamAnalyticsDto>> GetAnalyticsHistoryAsync(int liveStreamId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.LiveStreamAnalytics
            .Where(a => a.LiveStreamId == liveStreamId);

        if (startDate.HasValue)
            query = query.Where(a => a.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.Timestamp <= endDate.Value);

        var analytics = await query
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamAnalyticsDto>>(analytics);
    }

    public async Task<LiveStreamQualityDto> AddQualityAsync(int liveStreamId, LiveStreamQualityDto dto)
    {
        var quality = new LiveStreamQuality
        {
            LiveStreamId = liveStreamId,
            Quality = dto.Quality,
            Bitrate = dto.Bitrate,
            FrameRate = dto.FrameRate,
            Resolution = dto.Resolution,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.LiveStreamQualities.Add(quality);
        await _context.SaveChangesAsync();

        return _mapper.Map<LiveStreamQualityDto>(quality);
    }

    public async Task<List<LiveStreamQualityDto>> GetQualitiesAsync(int liveStreamId)
    {
        var qualities = await _context.LiveStreamQualities
            .Where(q => q.LiveStreamId == liveStreamId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamQualityDto>>(qualities);
    }

    public async Task<bool> RemoveQualityAsync(int qualityId)
    {
        var quality = await _context.LiveStreamQualities
            .FirstOrDefaultAsync(q => q.Id == qualityId);

        if (quality == null)
            return false;

        _context.LiveStreamQualities.Remove(quality);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetActiveQualityAsync(int qualityId, bool isActive)
    {
        var quality = await _context.LiveStreamQualities
            .FirstOrDefaultAsync(q => q.Id == qualityId);

        if (quality == null)
            return false;

        quality.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }
}
