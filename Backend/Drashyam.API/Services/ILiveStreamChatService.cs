using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ILiveStreamChatService
{
    Task<LiveStreamChatDto> SendMessageAsync(SendChatMessageDto dto, string userId);
    Task<List<LiveStreamChatDto>> GetChatMessagesAsync(int liveStreamId, int page = 1, int pageSize = 50);
    Task<LiveStreamChatDto> DeleteMessageAsync(int messageId, string userId);
    Task<LiveStreamChatDto> PinMessageAsync(int messageId, string userId);
    Task<LiveStreamChatDto> UnpinMessageAsync(int messageId, string userId);
    Task<LiveStreamReactionDto> SendReactionAsync(SendReactionDto dto, string userId);
    Task<List<LiveStreamReactionDto>> GetReactionsAsync(int liveStreamId, int page = 1, int pageSize = 50);
    Task<LiveStreamPollDto> CreatePollAsync(CreatePollDto dto, string userId);
    Task<LiveStreamPollDto> GetPollAsync(int pollId);
    Task<LiveStreamPollDto> VotePollAsync(VotePollDto dto, string userId);
    Task<LiveStreamPollDto> EndPollAsync(int pollId, string userId);
    Task<List<LiveStreamPollDto>> GetPollsAsync(int liveStreamId, int page = 1, int pageSize = 20);
    Task<LiveStreamAnalyticsDto> GetAnalyticsAsync(int liveStreamId);
    Task<List<LiveStreamAnalyticsDto>> GetAnalyticsHistoryAsync(int liveStreamId, DateTime? startDate = null, DateTime? endDate = null);
    Task<LiveStreamQualityDto> AddQualityAsync(int liveStreamId, LiveStreamQualityDto dto);
    Task<List<LiveStreamQualityDto>> GetQualitiesAsync(int liveStreamId);
    Task<bool> RemoveQualityAsync(int qualityId);
    Task<bool> SetActiveQualityAsync(int qualityId, bool isActive);
}
