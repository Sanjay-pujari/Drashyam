using Microsoft.AspNetCore.SignalR;

namespace Drashyam.API.Hubs;

public class VideoHub : Hub
{
    public async Task JoinVideoGroup(int videoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Video_{videoId}");
    }

    public async Task LeaveVideoGroup(int videoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Video_{videoId}");
    }

    public async Task SendComment(int videoId, string comment)
    {
        await Clients.Group($"Video_{videoId}").SendAsync("ReceiveComment", videoId, comment);
    }

    public async Task SendLike(int videoId, int likeCount)
    {
        await Clients.Group($"Video_{videoId}").SendAsync("ReceiveLike", videoId, likeCount);
    }

    public async Task SendDislike(int videoId, int dislikeCount)
    {
        await Clients.Group($"Video_{videoId}").SendAsync("ReceiveDislike", videoId, dislikeCount);
    }

    public async Task SendCommentLike(int videoId, int commentId, int likeCount, int dislikeCount)
    {
        await Clients.Group($"Video_{videoId}").SendAsync("ReceiveCommentLike", commentId, likeCount, dislikeCount);
    }

    public async Task SendCommentDislike(int videoId, int commentId, int likeCount, int dislikeCount)
    {
        await Clients.Group($"Video_{videoId}").SendAsync("ReceiveCommentDislike", commentId, likeCount, dislikeCount);
    }
}
