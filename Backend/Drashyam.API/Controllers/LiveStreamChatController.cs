using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiveStreamChatController : ControllerBase
{
    private readonly ILiveStreamChatService _chatService;
    private readonly ILogger<LiveStreamChatController> _logger;

    public LiveStreamChatController(ILiveStreamChatService chatService, ILogger<LiveStreamChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<ActionResult<LiveStreamChatDto>> SendMessage([FromBody] SendChatMessageDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = await _chatService.SendMessageAsync(dto, userId);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat message");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("messages/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamChatDto>>> GetChatMessages(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var messages = await _chatService.GetChatMessagesAsync(liveStreamId, page, pageSize);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat messages");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("message/{messageId}")]
    public async Task<ActionResult<LiveStreamChatDto>> DeleteMessage(int messageId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = await _chatService.DeleteMessageAsync(messageId, userId);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chat message");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("message/{messageId}/pin")]
    public async Task<ActionResult<LiveStreamChatDto>> PinMessage(int messageId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = await _chatService.PinMessageAsync(messageId, userId);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning chat message");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("message/{messageId}/pin")]
    public async Task<ActionResult<LiveStreamChatDto>> UnpinMessage(int messageId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var message = await _chatService.UnpinMessageAsync(messageId, userId);
            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpinning chat message");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reaction")]
    public async Task<ActionResult<LiveStreamReactionDto>> SendReaction([FromBody] SendReactionDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reaction = await _chatService.SendReactionAsync(dto, userId);
            return Ok(reaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reaction");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("reactions/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamReactionDto>>> GetReactions(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var reactions = await _chatService.GetReactionsAsync(liveStreamId, page, pageSize);
            return Ok(reactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reactions");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("poll")]
    public async Task<ActionResult<LiveStreamPollDto>> CreatePoll([FromBody] CreatePollDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var poll = await _chatService.CreatePollAsync(dto, userId);
            return Ok(poll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating poll");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("poll/{pollId}")]
    public async Task<ActionResult<LiveStreamPollDto>> GetPoll(int pollId)
    {
        try
        {
            var poll = await _chatService.GetPollAsync(pollId);
            return Ok(poll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting poll");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("poll/vote")]
    public async Task<ActionResult<LiveStreamPollDto>> VotePoll([FromBody] VotePollDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var poll = await _chatService.VotePollAsync(dto, userId);
            return Ok(poll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on poll");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("poll/{pollId}/end")]
    public async Task<ActionResult<LiveStreamPollDto>> EndPoll(int pollId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var poll = await _chatService.EndPollAsync(pollId, userId);
            return Ok(poll);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending poll");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("polls/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamPollDto>>> GetPolls(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var polls = await _chatService.GetPollsAsync(liveStreamId, page, pageSize);
            return Ok(polls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting polls");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("analytics/{liveStreamId}")]
    public async Task<ActionResult<LiveStreamAnalyticsDto>> GetAnalytics(int liveStreamId)
    {
        try
        {
            var analytics = await _chatService.GetAnalyticsAsync(liveStreamId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("analytics/{liveStreamId}/history")]
    public async Task<ActionResult<List<LiveStreamAnalyticsDto>>> GetAnalyticsHistory(int liveStreamId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _chatService.GetAnalyticsHistoryAsync(liveStreamId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics history");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("quality")]
    public async Task<ActionResult<LiveStreamQualityDto>> AddQuality([FromBody] LiveStreamQualityDto dto)
    {
        try
        {
            var quality = await _chatService.AddQualityAsync(dto.LiveStreamId, dto);
            return Ok(quality);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding quality");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("qualities/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamQualityDto>>> GetQualities(int liveStreamId)
    {
        try
        {
            var qualities = await _chatService.GetQualitiesAsync(liveStreamId);
            return Ok(qualities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting qualities");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("quality/{qualityId}")]
    public async Task<ActionResult<bool>> RemoveQuality(int qualityId)
    {
        try
        {
            var result = await _chatService.RemoveQualityAsync(qualityId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing quality");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("quality/{qualityId}/active")]
    public async Task<ActionResult<bool>> SetActiveQuality(int qualityId, [FromBody] bool isActive)
    {
        try
        {
            var result = await _chatService.SetActiveQualityAsync(qualityId, isActive);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active quality");
            return BadRequest(ex.Message);
        }
    }
}
