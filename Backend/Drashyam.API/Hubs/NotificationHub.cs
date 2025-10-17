using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Drashyam.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task LeaveUserGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task JoinInviteGroup(string inviteId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"invite_{inviteId}");
    }

    public async Task LeaveInviteGroup(string inviteId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"invite_{inviteId}");
    }

    public async Task JoinReferralGroup(string referralId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"referral_{referralId}");
    }

    public async Task LeaveReferralGroup(string referralId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"referral_{referralId}");
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await JoinUserGroup(userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await LeaveUserGroup(userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
