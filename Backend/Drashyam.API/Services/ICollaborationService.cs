using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface ICollaborationService
{
    // Collaboration Management
    Task<CollaborationDto> CreateCollaborationAsync(CollaborationCreateDto createDto, string userId);
    Task<CollaborationDto> GetCollaborationByIdAsync(int id, string userId);
    Task<PagedResult<CollaborationDto>> GetUserCollaborationsAsync(string userId, CollaborationFilterDto filter);
    Task<PagedResult<CollaborationDto>> GetPendingCollaborationsAsync(string userId, CollaborationFilterDto filter);
    Task<CollaborationDto> UpdateCollaborationAsync(int id, CollaborationUpdateDto updateDto, string userId);
    Task<bool> RespondToCollaborationAsync(CollaborationResponseDto responseDto, string userId);
    Task<bool> CancelCollaborationAsync(int id, string userId);
    Task<bool> CompleteCollaborationAsync(int id, string userId);
    
    // Collaboration Messages
    Task<CollaborationMessageDto> SendMessageAsync(CollaborationMessageCreateDto createDto, string userId);
    Task<PagedResult<CollaborationMessageDto>> GetCollaborationMessagesAsync(int collaborationId, string userId, int page = 1, int pageSize = 50);
    Task<bool> MarkMessagesAsReadAsync(int collaborationId, string userId);
    Task<int> GetUnreadMessageCountAsync(string userId);
    
    // Collaboration Assets
    Task<CollaborationAssetDto> UploadAssetAsync(CollaborationAssetCreateDto createDto, string userId);
    Task<List<CollaborationAssetDto>> GetCollaborationAssetsAsync(int collaborationId, string userId);
    Task<bool> DeleteAssetAsync(int assetId, string userId);
    
    // Collaboration Discovery
    Task<List<CollaborationDto>> GetSuggestedCollaboratorsAsync(string userId, int count = 10);
    Task<List<CollaborationDto>> GetPublicCollaborationsAsync(CollaborationFilterDto filter);
    Task<bool> IsUserAvailableForCollaborationAsync(string userId, DateTime startDate, DateTime endDate);
    
    // Analytics
    Task<CollaborationAnalyticsDto> GetCollaborationAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}

public class CollaborationAnalyticsDto
{
    public int TotalCollaborations { get; set; }
    public int ActiveCollaborations { get; set; }
    public int CompletedCollaborations { get; set; }
    public int PendingInvitations { get; set; }
    public decimal TotalRevenueShared { get; set; }
    public List<CollaborationTypeStats> TypeStats { get; set; } = new();
    public List<MonthlyCollaborationStats> MonthlyStats { get; set; } = new();
}

public class CollaborationTypeStats
{
    public CollaborationType Type { get; set; }
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyCollaborationStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Collaborations { get; set; }
    public decimal Revenue { get; set; }
}
