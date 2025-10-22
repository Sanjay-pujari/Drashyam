using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ISuperChatService
{
    Task<SuperChatDto> ProcessSuperChatAsync(SuperChatRequestDto request, string userId);
    Task<List<SuperChatDisplayDto>> GetLiveStreamSuperChatsAsync(int liveStreamId);
    Task<SuperChatDto> GetSuperChatByIdAsync(int superChatId);
    Task<bool> UpdateSuperChatStatusAsync(int superChatId, string status);
    Task<List<SuperChatDto>> GetUserSuperChatsAsync(string userId, int page = 1, int pageSize = 20);
    Task<List<SuperChatDto>> GetCreatorSuperChatsAsync(string creatorId, int page = 1, int pageSize = 20);
    Task<decimal> GetTotalSuperChatRevenueAsync(string creatorId, DateTime? startDate = null, DateTime? endDate = null);
}
