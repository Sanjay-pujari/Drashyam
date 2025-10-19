using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserSettingsService _userSettingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(IUserService userService, IUserSettingsService userSettingsService, ILogger<SettingsController> logger)
    {
        _userService = userService;
        _userSettingsService = userSettingsService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving profile");
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UserUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var updatedUser = await _userService.UpdateUserAsync(userId, updateDto);
            
            return Ok(updatedUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating profile");
        }
    }

    [HttpGet("privacy")]
    public async Task<ActionResult<PrivacySettingsDto>> GetPrivacySettings()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var settings = await _userSettingsService.GetPrivacySettingsAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving privacy settings");
        }
    }

    [HttpPut("privacy")]
    public async Task<ActionResult<PrivacySettingsDto>> UpdatePrivacySettings([FromBody] PrivacySettingsDto settings)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var updatedSettings = await _userSettingsService.UpdatePrivacySettingsAsync(userId, settings);
            return Ok(updatedSettings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating privacy settings");
        }
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<NotificationSettingsDto>> GetNotificationSettings()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var settings = await _userSettingsService.GetNotificationSettingsAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving notification settings");
        }
    }

    [HttpPut("notifications")]
    public async Task<ActionResult<NotificationSettingsDto>> UpdateNotificationSettings([FromBody] NotificationSettingsDto settings)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var updatedSettings = await _userSettingsService.UpdateNotificationSettingsAsync(userId, settings);
            return Ok(updatedSettings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating notification settings");
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // TODO: Implement password change
            
            return Ok(new { message = "Password changed successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while changing password");
        }
    }

    [HttpPost("export-data")]
    public async Task<ActionResult> ExportData()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // TODO: Implement data export
            
            return Ok(new { message = "Data export initiated. You will receive an email when ready." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while exporting data");
        }
    }

    [HttpDelete("account")]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // TODO: Implement account deletion
            
            return Ok(new { message = "Account deletion initiated. You will receive a confirmation email." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while deleting account");
        }
    }
}
