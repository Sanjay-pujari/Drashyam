using Microsoft.AspNetCore.SignalR;

namespace Drashyam.API.Hubs;

public class LiveStreamHub : Hub
{
    public async Task JoinLiveStream(int streamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"LiveStream_{streamId}");
    }

    public async Task LeaveLiveStream(int streamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"LiveStream_{streamId}");
    }

    public async Task SendLiveComment(int streamId, string comment)
    {
        await Clients.Group($"LiveStream_{streamId}").SendAsync("ReceiveLiveComment", comment);
    }

    public async Task UpdateViewerCount(int streamId, int viewerCount)
    {
        await Clients.Group($"LiveStream_{streamId}").SendAsync("UpdateViewerCount", viewerCount);
    }

    public async Task SendSuperChat(int streamId, object superChat)
    {
        await Clients.Group($"LiveStream_{streamId}").SendAsync("ReceiveSuperChat", superChat);
    }
}
