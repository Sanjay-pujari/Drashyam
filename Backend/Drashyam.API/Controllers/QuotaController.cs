using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota status for user");
            return StatusCode(500, "Error retrieving quota status");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting quota warnings for user");
            return StatusCode(500, "Error retrieving quota warnings");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription benefits for user");
            return StatusCode(500, "Error retrieving subscription benefits");
        }
    }

    [HttpPost("can-upload")]
    public async Task<ActionResult<bool>> CanUploadVideo([FromBody] CanUploadRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var canUpload = await _quotaService.CheckStorageQuotaAsync(userId, request.FileSize);
            return Ok(canUpload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking upload quota for user");
            return StatusCode(500, "Error checking upload quota");
        }
    }

    [HttpGet("can-create-channel")]
    public async Task<ActionResult<bool>> CanCreateChannel()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var canCreate = await _quotaService.CheckChannelQuotaAsync(userId);
            return Ok(canCreate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking channel quota for user");
            return StatusCode(500, "Error checking channel quota");
        }
    }
}

public class CanUploadRequest
{
    public long FileSize { get; set; }
}