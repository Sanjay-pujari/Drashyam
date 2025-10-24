using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAzureCommunicationService
{
    Task<StreamingEndpointDto> CreateStreamingEndpointAsync(string userId, string title, string description);
    Task<StreamingEndpointDto> GetStreamingEndpointAsync(string endpointId);
    Task<bool> StartStreamingAsync(string endpointId);
    Task<bool> StopStreamingAsync(string endpointId);
    Task<bool> DeleteStreamingEndpointAsync(string endpointId);
    Task<StreamingAnalyticsDto> GetStreamingAnalyticsAsync(string endpointId);
    Task<List<StreamingEndpointDto>> GetUserStreamingEndpointsAsync(string userId);
    Task<bool> UpdateStreamingSettingsAsync(string endpointId, StreamingSettingsDto settings);
    Task<StreamingHealthDto> GetStreamingHealthAsync(string endpointId);
}
