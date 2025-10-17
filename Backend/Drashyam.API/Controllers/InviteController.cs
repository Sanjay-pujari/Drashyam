using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Drashyam.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InviteController : ControllerBase
{
    private readonly IInviteService _inviteService;
    private readonly ILogger<InviteController> _logger;

    public InviteController(IInviteService inviteService, ILogger<InviteController> logger)
    {
        _inviteService = inviteService;
        _logger = logger;
    }

    [HttpPost]
    [RateLimit(10, 60)] // 10 invites per minute
    public async Task<ActionResult<UserInviteDto>> CreateInvite([FromBody] CreateInviteDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var invite = await _inviteService.CreateInviteAsync(userId, createDto);
            return Ok(invite);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invite");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("bulk")]
    [RateLimit(5, 300)] // 5 bulk operations per 5 minutes
    public async Task<ActionResult<List<UserInviteDto>>> BulkCreateInvites([FromBody] BulkInviteDto bulkDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var invites = await _inviteService.BulkCreateInvitesAsync(userId, bulkDto);
            return Ok(invites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk invites");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("token/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserInviteDto>> GetInviteByToken(string token)
    {
        try
        {
            var invite = await _inviteService.GetInviteByTokenAsync(token);
            return Ok(invite);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invite by token");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("accept/{token}")]
    [AllowAnonymous]
    [RateLimit(3, 300)] // 3 attempts per 5 minutes for anonymous users
    public async Task<ActionResult<UserInviteDto>> AcceptInvite(string token, [FromBody] AcceptInviteDto acceptDto)
    {
        try
        {
            var invite = await _inviteService.AcceptInviteAsync(token, acceptDto);
            return Ok(invite);
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
            _logger.LogError(ex, "Error accepting invite");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("my-invites")]
    public async Task<ActionResult<PagedResult<UserInviteDto>>> GetMyInvites([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var invites = await _inviteService.GetUserInvitesAsync(userId, page, pageSize);
            return Ok(invites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user invites");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<PagedResult<UserInviteDto>>> GetInvitesByEmail(string email, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var invites = await _inviteService.GetInvitesByEmailAsync(email, page, pageSize);
            return Ok(invites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invites by email");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{inviteId}")]
    public async Task<ActionResult> CancelInvite(int inviteId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.CancelInviteAsync(inviteId, userId);
            if (!result)
                return NotFound("Invite not found or cannot be cancelled");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invite");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{inviteId}/resend")]
    public async Task<ActionResult> ResendInvite(int inviteId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _inviteService.ResendInviteAsync(inviteId, userId);
            if (!result)
                return NotFound("Invite not found or cannot be resent");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invite");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<InviteStatsDto>> GetInviteStats()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var stats = await _inviteService.GetInviteStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invite stats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("link")]
    public async Task<ActionResult<InviteLinkDto>> CreateInviteLink([FromQuery] int? maxUsage = null, [FromQuery] DateTime? expiresAt = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var link = await _inviteService.CreateInviteLinkAsync(userId, maxUsage, expiresAt);
            return Ok(link);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invite link");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("validate/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> ValidateInviteToken(string token)
    {
        try
        {
            var isValid = await _inviteService.ValidateInviteTokenAsync(token);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating invite token");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{inviteId}/expire")]
    public async Task<ActionResult> ExpireInvite(int inviteId)
    {
        try
        {
            var result = await _inviteService.ExpireInviteAsync(inviteId);
            if (!result)
                return NotFound("Invite not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error expiring invite");
            return StatusCode(500, "Internal server error");
        }
    }
}
