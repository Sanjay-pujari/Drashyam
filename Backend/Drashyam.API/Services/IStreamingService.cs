using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IStreamingService
{
    // Stream Management
    Task<StreamInfoDto> StartStreamAsync(int streamId, string userId);
    Task<StreamInfoDto> StopStreamAsync(int streamId, string userId);
    Task<StreamInfoDto> PauseStreamAsync(int streamId, string userId);
    Task<StreamInfoDto> ResumeStreamAsync(int streamId, string userId);
    
    // Stream Quality Management
    Task<StreamQualityDto> GetStreamQualityAsync(int streamId);
    Task<StreamQualityDto> UpdateStreamQualityAsync(int streamId, StreamQualitySettingsDto settings);
    Task<List<StreamQualityDto>> GetAvailableQualitiesAsync();
    
    // Stream Recording
    Task<RecordingInfoDto> StartRecordingAsync(int streamId, string userId);
    Task<RecordingInfoDto> StopRecordingAsync(int streamId, string userId);
    Task<RecordingInfoDto> GetRecordingStatusAsync(int streamId);
    
    // Stream Analytics
    Task<StreamAnalyticsDto> GetStreamAnalyticsAsync(int streamId);
    Task<StreamHealthDto> GetStreamHealthAsync(int streamId);
    Task UpdateStreamMetricsAsync(int streamId, StreamMetricsDto metrics);
    
    // RTMP/WebRTC Integration
    Task<StreamEndpointDto> GetStreamEndpointAsync(int streamId);
    Task<bool> ValidateStreamKeyAsync(string streamKey);
    Task<StreamConfigDto> GetStreamConfigurationAsync(int streamId);
    
    // Stream Events
    Task NotifyStreamStartedAsync(int streamId);
    Task NotifyStreamEndedAsync(int streamId);
    Task NotifyViewerJoinedAsync(int streamId, string userId);
    Task NotifyViewerLeftAsync(int streamId, string userId);
}
