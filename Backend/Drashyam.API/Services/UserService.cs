using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Drashyam.API.Services;

public class UserService : IUserService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        DrashyamDbContext context,
        IMapper mapper,
        IFileStorageService fileStorage,
        ILogger<UserService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Channels)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ArgumentException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetUserByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.Channels)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            throw new ArgumentException("User not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UserUpdateDto updateDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        _mapper.Map(updateDto, user);
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateProfilePictureAsync(string userId, IFormFile profilePicture)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        var imageUrl = await _fileStorage.UploadProfilePictureAsync(profilePicture);
        user.ProfilePictureUrl = imageUrl;
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<UserDto>> SearchUsersAsync(string query, int page = 1, int pageSize = 20)
    {
        var users = await _context.Users
            .Where(u => u.IsActive && (u.FirstName.Contains(query) || u.LastName.Contains(query) || u.Email.Contains(query)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Users
            .Where(u => u.IsActive && (u.FirstName.Contains(query) || u.LastName.Contains(query) || u.Email.Contains(query)))
            .CountAsync();

        return new PagedResult<UserDto>
        {
            Items = _mapper.Map<List<UserDto>>(users),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> FollowUserAsync(string userId, string targetUserId)
    {
        // Implementation for following users
        var user = await _context.Users.FindAsync(userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UnfollowUserAsync(string userId, string targetUserId)
    {
        // Implementation for unfollowing users
        var user = await _context.Users.FindAsync(userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResult<UserDto>> GetFollowersAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Implementation for getting followers
        return new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<UserDto>> GetFollowingAsync(string userId, int page = 1, int pageSize = 20)
    {
        // Implementation for getting following
        return new PagedResult<UserDto>
        {
            Items = new List<UserDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsFollowingAsync(string userId, string targetUserId)
    {
        // Implementation for checking if following
        return false;
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        // Use ASP.NET Core Identity's UserManager for password change
        // This ensures consistency with the login authentication system
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ArgumentException($"Password change failed: {errors}");
        }
    }
}
