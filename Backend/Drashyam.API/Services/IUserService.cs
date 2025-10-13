using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(string userId);
    Task<UserDto> GetUserByEmailAsync(string email);
    Task<UserDto> UpdateUserAsync(string userId, UserUpdateDto updateDto);
    Task<UserDto> UpdateProfilePictureAsync(string userId, IFormFile profilePicture);
    Task<bool> DeleteUserAsync(string userId);
    Task<PagedResult<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20);
    Task<UserDto> FollowUserAsync(string userId, string targetUserId);
    Task<UserDto> UnfollowUserAsync(string userId, string targetUserId);
    Task<PagedResult<UserDto>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20);
    Task<PagedResult<UserDto>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> IsFollowingAsync(string userId, string targetUserId);
}
