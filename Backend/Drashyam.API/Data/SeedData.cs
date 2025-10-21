using Drashyam.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Data;

public static class SeedData
{
    public static async Task Initialize(
        DrashyamDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Only run migrations, don't recreate database
        await context.Database.MigrateAsync();

        // Check if database already has users - if so, skip seeding to preserve existing data
        var existingUsersCount = await context.Users.CountAsync();
        Console.WriteLine($"Database seeding: Found {existingUsersCount} existing users");
        
        if (existingUsersCount > 0)
        {
            Console.WriteLine("Database already has users - skipping seeding to preserve existing data");
            // Database already has data, only ensure roles exist
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    Console.WriteLine($"Creating missing role: {role}");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            return; // Exit early to preserve existing data
        }
        
        Console.WriteLine("Database is empty - proceeding with initial seeding");

        // Only seed if database is empty
        // Roles
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Admin user
        var adminEmail = "admin@drashyam.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(admin, "Admin@12345");
            await userManager.AddToRolesAsync(admin, roles);
        }

        // Demo user
        var demoEmail = "demo@drashyam.local";
        var demo = await userManager.FindByEmailAsync(demoEmail);
        if (demo == null)
        {
            demo = new ApplicationUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                FirstName = "Demo",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(demo, "Demo@12345");
            await userManager.AddToRoleAsync(demo, "User");
        }

        // Subscription Plans
        if (!context.SubscriptionPlans.Any())
        {
            context.SubscriptionPlans.AddRange(
                new SubscriptionPlan { Name = "Free", Price = 0, BillingCycle = Drashyam.API.DTOs.BillingCycle.Monthly, MaxChannels = 1, MaxVideosPerChannel = 10, MaxStorageGB = 1, HasAds = true, HasAnalytics = false, HasMonetization = false, HasLiveStreaming = false, IsActive = true },
                new SubscriptionPlan { Name = "Premium", Price = 9.99m, BillingCycle = Drashyam.API.DTOs.BillingCycle.Monthly, MaxChannels = 3, MaxVideosPerChannel = 100, MaxStorageGB = 100, HasAds = false, HasAnalytics = true, HasMonetization = true, HasLiveStreaming = true, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // Demo channel
        if (!context.Channels.Any())
        {
            var channel = new Channel
            {
                Name = "Demo Channel",
                Description = "Welcome to the demo channel",
                UserId = demo!.Id,
                CreatedAt = DateTime.UtcNow,
                Type = ChannelType.Personal,
                IsVerified = true,
                IsMonetized = false
            };
            context.Channels.Add(channel);
            await context.SaveChangesAsync();

            // Demo videos
            context.Videos.AddRange(
                new Video
                {
                    Title = "Getting Started",
                    Description = "Intro video",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = VideoProcessingStatus.Ready,
                    Type = VideoType.Uploaded,
                    Visibility = VideoVisibility.Public,
                    VideoUrl = "https://example.com/video1.mp4",
                    ThumbnailUrl = "https://example.com/thumb1.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(2)
                },
                new Video
                {
                    Title = "Second Video",
                    Description = "Another video",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = VideoProcessingStatus.Ready,
                    Type = VideoType.Uploaded,
                    Visibility = VideoVisibility.Public,
                    VideoUrl = "https://example.com/video2.mp4",
                    ThumbnailUrl = "https://example.com/thumb2.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(3)
                }
            );
            await context.SaveChangesAsync();
        }
    }
}


