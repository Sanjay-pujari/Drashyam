using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IPrivacyService _privacyService;

    public UserController(IUserService userService, ILogger<UserController> logger, IPrivacyService privacyService)
    {
        _userService = userService;
        _logger = logger;
        _privacyService = privacyService;
    }

    [HttpGet("{userId}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetById([FromRoute] string userId)
    {
        var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        // Check if user can view this profile
        var canViewProfile = await _privacyService.CanViewUserProfileAsync(userId, requestingUserId);
        if (!canViewProfile)
        {
            return Forbid("Access denied: You don't have permission to view this profile");
        }

        var user = await _userService.GetUserByIdAsync(userId);
        
        // Check if email should be hidden
        var canViewEmail = await _privacyService.CanViewUserEmailAsync(userId, requestingUserId);
        if (!canViewEmail)
        {
            user.Email = "***@***.***"; // Hide email
        }

        return Ok(user);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var user = await _userService.GetUserByIdAsync(userId);
        return Ok(user);
    }

    [HttpGet("by-email")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetByEmail([FromQuery] string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        return Ok(user);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UserUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var updated = await _userService.UpdateUserAsync(userId, updateDto);
        return Ok(updated);
    }

    [HttpPost("me/profile-picture")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfilePicture([FromForm] IFormFile profilePicture)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var updated = await _userService.UpdateProfilePictureAsync(userId, profilePicture);
        return Ok(updated);
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.DeleteUserAsync(userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<ActionResult<PagedResult<UserDto>>> Search([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var users = await _userService.SearchUsersAsync(query, page, pageSize);
        return Ok(users);
    }

    [HttpPost("{targetUserId}/follow")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Follow([FromRoute] string targetUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.FollowUserAsync(userId, targetUserId);
        return Ok(result);
    }

    [HttpPost("{targetUserId}/unfollow")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Unfollow([FromRoute] string targetUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.UnfollowUserAsync(userId, targetUserId);
        return Ok(result);
    }

    [HttpGet("{userId}/followers")]
    [Authorize]
    public async Task<ActionResult<PagedResult<UserDto>>> GetFollowers([FromRoute] string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetFollowersAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{userId}/following")]
    [Authorize]
    public async Task<ActionResult<PagedResult<UserDto>>> GetFollowing([FromRoute] string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _userService.GetFollowingAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{targetUserId}/is-following")]
    [Authorize]
    public async Task<ActionResult<bool>> IsFollowing([FromRoute] string targetUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.IsFollowingAsync(userId, targetUserId);
        return Ok(result);
    }
}


