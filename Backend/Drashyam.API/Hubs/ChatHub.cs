using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Services;
using Drashyam.API.DTOs;

namespace Drashyam.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILiveStreamChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILiveStreamChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinChat(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Add user to chat group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{streamId}");
            
            // Get recent chat messages
            var recentMessages = await _chatService.GetChatMessagesAsync(streamId, 1, 50);
            
            // Send recent messages to the user
            await Clients.Caller.SendAsync("ChatHistory", recentMessages);

            // Notify others that user joined chat
            await Clients.Group($"chat_{streamId}").SendAsync("UserJoinedChat", new
            {
                UserId = userId,
                StreamId = streamId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation($"User {userId} joined chat for stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining chat for stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to join chat");
        }
    }

    public async Task LeaveChat(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Remove user from chat group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{streamId}");
            
            // Notify others that user left chat
            await Clients.Group($"chat_{streamId}").SendAsync("UserLeftChat", new
            {
                UserId = userId,
                StreamId = streamId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation($"User {userId} left chat for stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving chat for stream {streamId}");
        }
    }

    public async Task SendMessage(int streamId, string message, string messageType = "Text", string? emoji = null)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Create message DTO
            var messageDto = new SendChatMessageDto
            {
                LiveStreamId = streamId,
                Message = message,
                Type = Enum.Parse<ChatMessageType>(messageType),
                Emoji = emoji
            };

            // Save message to database
            var savedMessage = await _chatService.SendMessageAsync(messageDto, userId);

            // Broadcast message to all chat participants
            await Clients.Group($"chat_{streamId}").SendAsync("MessageReceived", savedMessage);

            _logger.LogInformation($"Message sent by user {userId} in stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message in stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to send message");
        }
    }

    public async Task SendReaction(int streamId, string reactionType)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Create reaction DTO
            var reactionDto = new SendReactionDto
            {
                LiveStreamId = streamId,
                Type = Enum.Parse<ReactionType>(reactionType)
            };

            // Save reaction to database
            var savedReaction = await _chatService.SendReactionAsync(reactionDto, userId);

            // Broadcast reaction to all chat participants
            await Clients.Group($"chat_{streamId}").SendAsync("ReactionReceived", savedReaction);

            _logger.LogInformation($"Reaction sent by user {userId} in stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending reaction in stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to send reaction");
        }
    }

    public async Task CreatePoll(int streamId, string question, string description, string[] options, bool allowMultipleChoices = false)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Create poll DTO
            var pollDto = new CreatePollDto
            {
                LiveStreamId = streamId,
                Question = question,
                Description = description,
                Options = options.ToList(),
                AllowMultipleChoices = allowMultipleChoices
            };

            // Save poll to database
            var savedPoll = await _chatService.CreatePollAsync(pollDto, userId);

            // Broadcast poll to all chat participants
            await Clients.Group($"chat_{streamId}").SendAsync("PollCreated", savedPoll);

            _logger.LogInformation($"Poll created by user {userId} in stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating poll in stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to create poll");
        }
    }

    public async Task VotePoll(int pollId, int[] optionIds)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Create vote DTO
            var voteDto = new VotePollDto
            {
                PollId = pollId,
                OptionIds = optionIds.ToList()
            };

            // Save vote to database
            var updatedPoll = await _chatService.VotePollAsync(voteDto, userId);

            // Get stream ID from poll
            var poll = await _chatService.GetPollAsync(pollId);
            
            // Broadcast updated poll to all chat participants
            await Clients.Group($"chat_{poll.LiveStreamId}").SendAsync("PollUpdated", updatedPoll);

            _logger.LogInformation($"Vote cast by user {userId} for poll {pollId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error voting on poll {pollId}");
            await Clients.Caller.SendAsync("Error", "Failed to vote on poll");
        }
    }

    public async Task PinMessage(int messageId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Pin message in database
            var pinnedMessage = await _chatService.PinMessageAsync(messageId, userId);

            // Broadcast pinned message to all chat participants
            await Clients.Group($"chat_{pinnedMessage.LiveStreamId}").SendAsync("MessagePinned", pinnedMessage);

            _logger.LogInformation($"Message {messageId} pinned by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error pinning message {messageId}");
            await Clients.Caller.SendAsync("Error", "Failed to pin message");
        }
    }

    public async Task DeleteMessage(int messageId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Delete message in database
            var deletedMessage = await _chatService.DeleteMessageAsync(messageId, userId);

            // Broadcast deleted message to all chat participants
            await Clients.Group($"chat_{deletedMessage.LiveStreamId}").SendAsync("MessageDeleted", deletedMessage);

            _logger.LogInformation($"Message {messageId} deleted by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting message {messageId}");
            await Clients.Caller.SendAsync("Error", "Failed to delete message");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} connected to ChatHub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} disconnected from ChatHub");
        await base.OnDisconnectedAsync(exception);
    }
}
