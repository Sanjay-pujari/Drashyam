using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.Services;
using Drashyam.API.DTOs;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotaController : ControllerBase
{
    private readonly IQuotaService _quotaService;
    private readonly ILogger<QuotaController> _logger;

    public QuotaController(IQuotaService quotaService, ILogger<QuotaController> logger)
    {
        _quotaService = quotaService;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<ActionResult<QuotaStatusDto>> GetQuotaStatus()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var quotaStatus = await _quotaService.GetUserQuotaStatusAsync(userId);
            return Ok(quotaStatus);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota status");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("warnings")]
    public async Task<ActionResult<QuotaWarningDto>> GetQuotaWarnings()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var warnings = await _quotaService.GetQuotaWarningsAsync(userId);
            return Ok(warnings);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota warnings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("benefits")]
    public async Task<ActionResult<SubscriptionBenefitsDto>> GetSubscriptionBenefits()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var benefits = await _quotaService.GetSubscriptionBenefitsAsync(userId);
            return Ok(benefits);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription benefits");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("check-upload")]
    public async Task<ActionResult<QuotaCheckDto>> CheckVideoUpload([FromBody] VideoUploadCheckDto checkDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var canUpload = await _quotaService.CanUploadVideoAsync(userId, checkDto.ChannelId, checkDto.FileSize);
            var canCreateChannel = await _quotaService.CanCreateChannelAsync(userId);
            var warnings = await _quotaService.GetQuotaWarningsAsync(userId);

            return Ok(new QuotaCheckDto
            {
                CanUpload = canUpload,
                CanCreateChannel = canCreateChannel,
                CanUseFeature = true,
                Reason = canUpload ? null : "Quota exceeded",
                Warnings = warnings.HasWarnings ? warnings : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video upload quota");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("check-channel")]
    public async Task<ActionResult<QuotaCheckDto>> CheckChannelCreation()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var canCreateChannel = await _quotaService.CanCreateChannelAsync(userId);
            var warnings = await _quotaService.GetQuotaWarningsAsync(userId);

            return Ok(new QuotaCheckDto
            {
                CanUpload = true,
                CanCreateChannel = canCreateChannel,
                CanUseFeature = canCreateChannel,
                Reason = canCreateChannel ? null : "Channel quota exceeded",
                Warnings = warnings.HasWarnings ? warnings : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking channel creation quota");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("feature/{feature}")]
    public async Task<ActionResult<bool>> CheckFeatureAccess(string feature)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var hasAccess = await _quotaService.CheckSubscriptionFeaturesAsync(userId, feature);
            return Ok(hasAccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking feature access");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class VideoUploadCheckDto
{
    public int ChannelId { get; set; }
    public long FileSize { get; set; }
}
