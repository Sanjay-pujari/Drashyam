using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate password confirmation
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Password and confirmation password do not match");
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("A user with this email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = false // Explicitly set to false for email confirmation flow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                
                var autoConfirm = _configuration.GetValue<bool>("Auth:AutoConfirmOnRegister");
                if (autoConfirm)
                {
                    // Auto-confirm email in development or when explicitly enabled
                    _logger.LogInformation("Auto-confirming email for user: {Email}", user.Email);
                    var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmResult = await _userManager.ConfirmEmailAsync(user, confirmToken);
                    if (!confirmResult.Succeeded)
                    {
                        _logger.LogError("Failed to auto-confirm email for user: {Email}. Errors: {Errors}", 
                            user.Email, string.Join(", ", confirmResult.Errors.Select(e => e.Description)));
                        return BadRequest(confirmResult.Errors);
                    }

                    var jwt = await GenerateJwtTokenAsync(user);
                    _logger.LogInformation("User registered and auto-confirmed: {Email}", user.Email);
                    return Ok(new { token = jwt, user = MapToUserDto(user) });
                }
                else
                {
                    // Send email confirmation
                    _logger.LogInformation("Sending email confirmation to user: {Email}", user.Email);
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var emailSent = await _emailService.SendEmailVerificationAsync(user.Email, token);
                    
                    if (!emailSent)
                    {
                        _logger.LogError("Failed to send email confirmation to user: {Email}", user.Email);
                        return StatusCode(500, "Registration successful but failed to send confirmation email. Please contact support.");
                    }

                    _logger.LogInformation("Email confirmation sent successfully to user: {Email}", user.Email);
                    return Ok(new { 
                        message = "Registration successful. Please check your email to confirm your account.",
                        requiresEmailConfirmation = true
                    });
                }
            }

            _logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}", 
                model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", model.Email);
            return StatusCode(500, "An error occurred during registration");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto model)
    {
        try
        {
            Console.WriteLine($"Login attempt for email: {model.Email}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model state is invalid");
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            Console.WriteLine($"User found: {user != null}, IsActive: {user?.IsActive}, EmailConfirmed: {user?.EmailConfirmed}");
            
            if (user == null || !user.IsActive)
            {
                Console.WriteLine("User not found or not active");
                return Unauthorized("Invalid credentials");
            }

            Console.WriteLine($"Attempting password verification for user: {user.Email}");
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            Console.WriteLine($"Password verification result: {result.Succeeded}");
            
            if (!result.Succeeded)
            {
                Console.WriteLine("Password verification failed");
                if (result.IsLockedOut)
                    Console.WriteLine("User is locked out");
                if (result.IsNotAllowed)
                    Console.WriteLine("User is not allowed to sign in");
                if (result.RequiresTwoFactor)
                    Console.WriteLine("Two factor authentication required");
                return Unauthorized("Invalid credentials");
            }

            if (!user.EmailConfirmed)
            {
                Console.WriteLine("Email not confirmed");
                return BadRequest("Please confirm your email before logging in");
            }

            Console.WriteLine("Login successful, generating token");
            var token = await GenerateJwtTokenAsync(user);
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { token, user = MapToUserDto(user) });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpPost("reset-admin-password")]
    public async Task<IActionResult> ResetAdminPassword()
    {
        try
        {
            Console.WriteLine("Resetting admin password...");
            
            var adminEmail = "admin@drashyam.local";
            var user = await _userManager.FindByEmailAsync(adminEmail);
            
            if (user == null)
            {
                Console.WriteLine("Admin user not found, creating new one...");
                user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var createResult = await _userManager.CreateAsync(user, "Admin@12345");
                if (!createResult.Succeeded)
                {
                    Console.WriteLine($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    return BadRequest("Failed to create admin user");
                }
            }
            else
            {
                Console.WriteLine("Admin user found, resetting password...");
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, token, "Admin@12345");
                
                if (!resetResult.Succeeded)
                {
                    Console.WriteLine($"Failed to reset password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
                    return BadRequest("Failed to reset password");
                }
            }
            
            Console.WriteLine("Admin password reset successfully");
            return Ok(new { message = "Admin password reset to: Admin@12345" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reset password error: {ex.Message}");
            return StatusCode(500, "An error occurred while resetting password");
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
    {
        try
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
            {
                return BadRequest("User ID and token are required");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempted for non-existent user: {UserId}", model.UserId);
                return BadRequest("Invalid user");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Email already confirmed for user: {Email}", user.Email);
                return Ok(new { message = "Email is already confirmed" });
            }

            _logger.LogInformation("Attempting email confirmation for user: {Email}", user.Email);
            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user: {Email}", user.Email);
                
                // Generate JWT token for immediate login after confirmation
                var jwt = await GenerateJwtTokenAsync(user);
                return Ok(new { 
                    message = "Email confirmed successfully", 
                    token = jwt,
                    user = MapToUserDto(user)
                });
            }

            _logger.LogWarning("Email confirmation failed for user: {Email}. Errors: {Errors}", 
                user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest("Invalid or expired confirmation token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email confirmation for user: {UserId}", model.UserId);
            return StatusCode(500, "An error occurred during email confirmation");
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendConfirmationDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Resend confirmation requested for non-existent email: {Email}", model.Email);
                return Ok(new { message = "If the email exists, a confirmation email has been sent" });
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("Resend confirmation requested for already confirmed email: {Email}", model.Email);
                return Ok(new { message = "Email is already confirmed" });
            }

            _logger.LogInformation("Resending email confirmation to user: {Email}", user.Email);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailSent = await _emailService.SendEmailVerificationAsync(user.Email, token);
            
            if (!emailSent)
            {
                _logger.LogError("Failed to resend email confirmation to user: {Email}", user.Email);
                return StatusCode(500, "Failed to send confirmation email");
            }

            _logger.LogInformation("Email confirmation resent successfully to user: {Email}", user.Email);
            return Ok(new { message = "If the email exists, a confirmation email has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend confirmation for email: {Email}", model.Email);
            return StatusCode(500, "An error occurred while resending confirmation email");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetDto model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for non-existent email: {Email}", model.Email);
                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }

            _logger.LogInformation("Generating password reset token for user: {Email}", model.Email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            _logger.LogInformation("Sending password reset email to: {Email}", model.Email);
            var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            
            if (!emailSent)
            {
                _logger.LogError("Failed to send password reset email to: {Email}", model.Email);
                return StatusCode(500, "Failed to send password reset email");
            }

            _logger.LogInformation("Password reset email sent successfully to: {Email}", model.Email);
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password request for email: {Email}", model.Email);
            return StatusCode(500, "An error occurred during password reset request");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetConfirmDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate password confirmation
            if (model.NewPassword != model.ConfirmPassword)
            {
                return BadRequest("Password and confirmation password do not match");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid email");

            // Log the reset attempt for debugging
            _logger.LogInformation("Password reset attempt for user: {Email}", model.Email);

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for user: {Email}", model.Email);
                return Ok(new { message = "Password reset successfully" });
            }

            _logger.LogWarning("Password reset failed for user: {Email}. Errors: {Errors}", 
                model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", model.Email);
            return StatusCode(500, "An error occurred during password reset");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred during logout");
        }
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                return Unauthorized();

            var token = await GenerateJwtTokenAsync(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred during token refresh");
        }
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePictureUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            SubscriptionType = (Drashyam.API.DTOs.SubscriptionType)user.SubscriptionType,
            SubscriptionExpiresAt = user.SubscriptionExpiresAt
        };
    }
}

public class ConfirmEmailDto
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ResendConfirmationDto
{
    public string Email { get; set; } = string.Empty;
}

