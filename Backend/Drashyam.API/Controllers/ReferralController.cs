using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReferralController : ControllerBase
{
    private readonly IReferralService _referralService;
    private readonly ILogger<ReferralController> _logger;

    public ReferralController(IReferralService referralService, ILogger<ReferralController> logger)
    {
        _referralService = referralService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ReferralDto>> CreateReferral([FromBody] CreateReferralDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var referral = await _referralService.CreateReferralAsync(userId, createDto);
            return Ok(referral);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{referralId}")]
    public async Task<ActionResult<ReferralDto>> GetReferral(int referralId)
    {
        try
        {
            var referral = await _referralService.GetReferralByIdAsync(referralId);
            return Ok(referral);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("my-referrals")]
    public async Task<ActionResult<PagedResult<ReferralDto>>> GetMyReferrals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var referrals = await _referralService.GetUserReferralsAsync(userId, page, pageSize);
            return Ok(referrals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("referrals-by-user/{userId}")]
    public async Task<ActionResult<PagedResult<ReferralDto>>> GetReferralsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var referrals = await _referralService.GetReferralsByUserAsync(userId, page, pageSize);
            return Ok(referrals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ReferralStatsDto>> GetReferralStats()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var stats = await _referralService.GetReferralStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("code")]
    public async Task<ActionResult<ReferralCodeDto>> CreateReferralCode([FromBody] CreateReferralCodeDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var code = await _referralService.CreateReferralCodeAsync(userId, createDto);
            return Ok(code);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("code")]
    public async Task<ActionResult<ReferralCodeDto>> GetReferralCode()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var code = await _referralService.GetReferralCodeAsync(userId);
            return Ok(code);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("validate-code/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> ValidateReferralCode(string code)
    {
        try
        {
            var isValid = await _referralService.ValidateReferralCodeAsync(code);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("claim-reward")]
    public async Task<ActionResult<ReferralRewardDto>> ClaimReward([FromBody] ClaimRewardDto claimDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reward = await _referralService.ClaimRewardAsync(userId, claimDto);
            return Ok(reward);
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
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("rewards")]
    public async Task<ActionResult<PagedResult<ReferralRewardDto>>> GetUserRewards([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var rewards = await _referralService.GetUserRewardsAsync(userId, page, pageSize);
            return Ok(rewards);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{referralId}/process-reward")]
    public async Task<ActionResult> ProcessReferralReward(int referralId)
    {
        try
        {
            var result = await _referralService.ProcessReferralRewardAsync(referralId);
            if (!result)
                return NotFound("Referral not found or cannot be processed");

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{referralId}/status")]
    public async Task<ActionResult> UpdateReferralStatus(int referralId, [FromBody] ReferralStatus status)
    {
        try
        {
            var result = await _referralService.UpdateReferralStatusAsync(referralId, status);
            if (!result)
                return NotFound("Referral not found");

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}
