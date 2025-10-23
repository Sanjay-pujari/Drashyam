using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollaborationController : ControllerBase
{
    private readonly ICollaborationService _collaborationService;
    private readonly ILogger<CollaborationController> _logger;

    public CollaborationController(ICollaborationService collaborationService, ILogger<CollaborationController> logger)
    {
        _collaborationService = collaborationService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CollaborationDto>> CreateCollaboration([FromBody] CollaborationCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var collaboration = await _collaborationService.CreateCollaborationAsync(createDto, userId);
            return CreatedAtAction(nameof(GetCollaboration), new { id = collaboration.Id }, collaboration);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating collaboration");
            return StatusCode(500, "An error occurred while creating the collaboration");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CollaborationDto>> GetCollaboration([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var collaboration = await _collaborationService.GetCollaborationByIdAsync(id, userId);
            return Ok(collaboration);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collaboration {CollaborationId}", id);
            return StatusCode(500, "An error occurred while retrieving the collaboration");
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CollaborationDto>>> GetUserCollaborations([FromQuery] CollaborationFilterDto filter)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var collaborations = await _collaborationService.GetUserCollaborationsAsync(userId, filter);
            return Ok(collaborations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user collaborations");
            return StatusCode(500, "An error occurred while retrieving collaborations");
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<PagedResult<CollaborationDto>>> GetPendingCollaborations([FromQuery] CollaborationFilterDto filter)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var collaborations = await _collaborationService.GetPendingCollaborationsAsync(userId, filter);
            return Ok(collaborations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending collaborations");
            return StatusCode(500, "An error occurred while retrieving pending collaborations");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CollaborationDto>> UpdateCollaboration([FromRoute] int id, [FromBody] CollaborationUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var collaboration = await _collaborationService.UpdateCollaborationAsync(id, updateDto, userId);
            return Ok(collaboration);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating collaboration {CollaborationId}", id);
            return StatusCode(500, "An error occurred while updating the collaboration");
        }
    }

    [HttpPost("respond")]
    public async Task<ActionResult> RespondToCollaboration([FromBody] CollaborationResponseDto responseDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _collaborationService.RespondToCollaborationAsync(responseDto, userId);
            
            if (success)
            {
                return Ok(new { message = "Collaboration response sent successfully" });
            }
            
            return BadRequest("Failed to respond to collaboration");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to collaboration");
            return StatusCode(500, "An error occurred while responding to the collaboration");
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult> CancelCollaboration([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _collaborationService.CancelCollaborationAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Collaboration cancelled successfully" });
            }
            
            return BadRequest("Failed to cancel collaboration");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling collaboration {CollaborationId}", id);
            return StatusCode(500, "An error occurred while cancelling the collaboration");
        }
    }

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult> CompleteCollaboration([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _collaborationService.CompleteCollaborationAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Collaboration completed successfully" });
            }
            
            return BadRequest("Failed to complete collaboration");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing collaboration {CollaborationId}", id);
            return StatusCode(500, "An error occurred while completing the collaboration");
        }
    }

    [HttpPost("messages")]
    public async Task<ActionResult<CollaborationMessageDto>> SendMessage([FromBody] CollaborationMessageCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var message = await _collaborationService.SendMessageAsync(createDto, userId);
            return CreatedAtAction(nameof(GetMessages), new { collaborationId = createDto.CollaborationId }, message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending collaboration message");
            return StatusCode(500, "An error occurred while sending the message");
        }
    }

    [HttpGet("{collaborationId:int}/messages")]
    public async Task<ActionResult<PagedResult<CollaborationMessageDto>>> GetMessages([FromRoute] int collaborationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var messages = await _collaborationService.GetCollaborationMessagesAsync(collaborationId, userId, page, pageSize);
            return Ok(messages);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collaboration messages for {CollaborationId}", collaborationId);
            return StatusCode(500, "An error occurred while retrieving messages");
        }
    }

    [HttpPost("{collaborationId:int}/messages/read")]
    public async Task<ActionResult> MarkMessagesAsRead([FromRoute] int collaborationId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _collaborationService.MarkMessagesAsReadAsync(collaborationId, userId);
            
            if (success)
            {
                return Ok(new { message = "Messages marked as read" });
            }
            
            return BadRequest("Failed to mark messages as read");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for {CollaborationId}", collaborationId);
            return StatusCode(500, "An error occurred while marking messages as read");
        }
    }

    [HttpGet("messages/unread-count")]
    public async Task<ActionResult<int>> GetUnreadMessageCount()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var count = await _collaborationService.GetUnreadMessageCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread message count");
            return StatusCode(500, "An error occurred while retrieving unread message count");
        }
    }

    [HttpPost("assets")]
    public async Task<ActionResult<CollaborationAssetDto>> UploadAsset([FromBody] CollaborationAssetCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var asset = await _collaborationService.UploadAssetAsync(createDto, userId);
            return CreatedAtAction(nameof(GetAssets), new { collaborationId = createDto.CollaborationId }, asset);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading collaboration asset");
            return StatusCode(500, "An error occurred while uploading the asset");
        }
    }

    [HttpGet("{collaborationId:int}/assets")]
    public async Task<ActionResult<List<CollaborationAssetDto>>> GetAssets([FromRoute] int collaborationId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var assets = await _collaborationService.GetCollaborationAssetsAsync(collaborationId, userId);
            return Ok(assets);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collaboration assets for {CollaborationId}", collaborationId);
            return StatusCode(500, "An error occurred while retrieving assets");
        }
    }

    [HttpDelete("assets/{assetId:int}")]
    public async Task<ActionResult> DeleteAsset([FromRoute] int assetId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _collaborationService.DeleteAssetAsync(assetId, userId);
            
            if (success)
            {
                return Ok(new { message = "Asset deleted successfully" });
            }
            
            return BadRequest("Failed to delete asset");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting collaboration asset {AssetId}", assetId);
            return StatusCode(500, "An error occurred while deleting the asset");
        }
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<List<CollaborationDto>>> GetSuggestedCollaborators([FromQuery] int count = 10)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var suggestions = await _collaborationService.GetSuggestedCollaboratorsAsync(userId, count);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collaboration suggestions");
            return StatusCode(500, "An error occurred while retrieving collaboration suggestions");
        }
    }

    [HttpGet("public")]
    public async Task<ActionResult<List<CollaborationDto>>> GetPublicCollaborations([FromQuery] CollaborationFilterDto filter)
    {
        try
        {
            var collaborations = await _collaborationService.GetPublicCollaborationsAsync(filter);
            return Ok(collaborations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public collaborations");
            return StatusCode(500, "An error occurred while retrieving public collaborations");
        }
    }

    [HttpGet("availability")]
    public async Task<ActionResult<bool>> CheckAvailability([FromQuery] string userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var isAvailable = await _collaborationService.IsUserAvailableForCollaborationAsync(userId, startDate, endDate);
            return Ok(isAvailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user availability");
            return StatusCode(500, "An error occurred while checking availability");
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<CollaborationAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var analytics = await _collaborationService.GetCollaborationAnalyticsAsync(userId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collaboration analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }
}
