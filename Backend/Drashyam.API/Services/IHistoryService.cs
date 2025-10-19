using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IHistoryService
{
    Task<List<HistoryDto>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20);
    Task<HistoryDto> AddToHistoryAsync(string userId, HistoryCreateDto historyDto);
    Task<HistoryDto> UpdateHistoryAsync(int historyId, string userId, HistoryUpdateDto historyDto);
    Task<bool> RemoveFromHistoryAsync(int historyId, string userId);
    Task<bool> ClearUserHistoryAsync(string userId);
    Task<HistoryDto?> GetHistoryItemAsync(int historyId, string userId);
    Task<bool> IsVideoInHistoryAsync(int videoId, string userId);
    Task<int> GetUserHistoryCountAsync(string userId);
}

