using Drashyam.API.Models;
using Drashyam.API.DTOs;
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
            var roles1 = new[] { "Admin", "User" };
            foreach (var role in roles1)
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
                Type = DTOs.ChannelType.Personal,
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
                    Status = DTOs.VideoProcessingStatus.Ready,
                    Type = DTOs.VideoType.Uploaded,
                    Visibility = DTOs.VideoVisibility.Public,
                    VideoUrl = "https://example.com/video1.mp4",
                    ThumbnailUrl = "https://example.com/thumb1.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(2),
                    Category = "Education",
                    Tags = "tutorial,beginner,guide"
                },
                new Video
                {
                    Title = "Second Video",
                    Description = "Another video",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = DTOs.VideoProcessingStatus.Ready,
                    Type = DTOs.VideoType.Uploaded,
                    Visibility = DTOs.VideoVisibility.Public,
                    VideoUrl = "https://example.com/video2.mp4",
                    ThumbnailUrl = "https://example.com/thumb2.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(3),
                    Category = "Entertainment",
                    Tags = "fun,comedy,entertainment"
                },
                new Video
                {
                    Title = "Music Video",
                    Description = "Amazing music performance",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = DTOs.VideoProcessingStatus.Ready,
                    Type = DTOs.VideoType.Uploaded,
                    Visibility = DTOs.VideoVisibility.Public,
                    VideoUrl = "https://example.com/video3.mp4",
                    ThumbnailUrl = "https://example.com/thumb3.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(4),
                    Category = "Music",
                    Tags = "music,song,performance"
                },
                new Video
                {
                    Title = "Gaming Tutorial",
                    Description = "Learn how to play this game",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = DTOs.VideoProcessingStatus.Ready,
                    Type = DTOs.VideoType.Uploaded,
                    Visibility = DTOs.VideoVisibility.Public,
                    VideoUrl = "https://example.com/video4.mp4",
                    ThumbnailUrl = "https://example.com/thumb4.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(5),
                    Category = "Gaming",
                    Tags = "gaming,tutorial,strategy"
                },
                new Video
                {
                    Title = "Tech Review",
                    Description = "Latest technology review",
                    UserId = demo.Id,
                    ChannelId = channel.Id,
                    Status = DTOs.VideoProcessingStatus.Ready,
                    Type = DTOs.VideoType.Uploaded,
                    Visibility = DTOs.VideoVisibility.Public,
                    VideoUrl = "https://example.com/video5.mp4",
                    ThumbnailUrl = "https://example.com/thumb5.jpg",
                    CreatedAt = DateTime.UtcNow,
                    Duration = TimeSpan.FromMinutes(6),
                    Category = "Technology",
                    Tags = "technology,review,gadgets"
                }
            );
            await context.SaveChangesAsync();

            // Add some sample likes for favorites
            var videos = await context.Videos.ToListAsync();
            if (videos.Any())
            {
                context.VideoLikes.AddRange(
                    new VideoLike
                    {
                        UserId = demo.Id,
                        VideoId = videos[0].Id,
                        Type = DTOs.LikeType.Like,
                        CreatedAt = DateTime.UtcNow
                    },
                    new VideoLike
                    {
                        UserId = demo.Id,
                        VideoId = videos[1].Id,
                        Type = DTOs.LikeType.Like,
                        CreatedAt = DateTime.UtcNow
                    },
                    new VideoLike
                    {
                        UserId = demo.Id,
                        VideoId = videos[2].Id,
                        Type = DTOs.LikeType.Like,
                        CreatedAt = DateTime.UtcNow
                    }
                );
                await context.SaveChangesAsync();
            }

            // Add sample ad campaigns and ads
            var adCampaigns = new[]
            {
                new AdCampaign
                {
                    Name = "Tech Gadgets Campaign",
                    Description = "Promote the latest tech gadgets",
                    AdvertiserId = admin.Id,
                    Type = DTOs.AdType.Video,
                    Budget = 1000.00m,
                    CostPerClick = 0.50m,
                    CostPerView = 0.10m,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(30),
                    Status = DTOs.AdStatus.Active,
                    TargetAudience = "Technology enthusiasts",
                    AdContent = "Check out the latest smartphones and gadgets!",
                    AdUrl = "https://example.com/tech-gadgets",
                    ThumbnailUrl = "https://via.placeholder.com/300x200/4CAF50/white?text=Tech+Gadgets",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new AdCampaign
                {
                    Name = "Fitness Equipment Campaign",
                    Description = "Promote fitness and health equipment",
                    AdvertiserId = admin.Id,
                    Type = DTOs.AdType.Video,
                    Budget = 800.00m,
                    CostPerClick = 0.40m,
                    CostPerView = 0.08m,
                    StartDate = DateTime.UtcNow.AddDays(-15),
                    EndDate = DateTime.UtcNow.AddDays(45),
                    Status = DTOs.AdStatus.Active,
                    TargetAudience = "Fitness enthusiasts",
                    AdContent = "Transform your fitness routine with our premium equipment!",
                    AdUrl = "https://example.com/fitness-equipment",
                    ThumbnailUrl = "https://via.placeholder.com/300x200/FF5722/white?text=Fitness+Equipment",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new AdCampaign
                {
                    Name = "Online Learning Campaign",
                    Description = "Promote online courses and education",
                    AdvertiserId = admin.Id,
                    Type = DTOs.AdType.Video,
                    Budget = 1200.00m,
                    CostPerClick = 0.60m,
                    CostPerView = 0.12m,
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow.AddDays(60),
                    Status = DTOs.AdStatus.Active,
                    TargetAudience = "Students and professionals",
                    AdContent = "Learn new skills with our comprehensive online courses!",
                    AdUrl = "https://example.com/online-learning",
                    ThumbnailUrl = "https://via.placeholder.com/300x200/2196F3/white?text=Online+Learning",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.AdCampaigns.AddRange(adCampaigns);
            await context.SaveChangesAsync();

            // Add sample ads for each campaign
            var campaigns = await context.AdCampaigns.ToListAsync();
            if (campaigns.Any())
            {
                var sampleAds = new[]
                {
                    new Ad
                    {
                        CampaignId = campaigns[0].Id,
                        Type = DTOs.AdType.Video,
                        Content = "Discover the latest iPhone 15 Pro with advanced camera features and 5G connectivity. Limited time offer - 20% off!",
                        Url = "https://example.com/iphone-15-pro",
                        ThumbnailUrl = "https://via.placeholder.com/300x200/4CAF50/white?text=iPhone+15+Pro",
                        CostPerClick = 0.50m,
                        CostPerView = 0.10m,
                        Duration = 30,
                        SkipAfter = 5,
                        Position = null
                    },
                    new Ad
                    {
                        CampaignId = campaigns[1].Id,
                        Type = DTOs.AdType.Video,
                        Content = "Get fit with our premium home gym equipment. Free shipping on orders over $100!",
                        Url = "https://example.com/home-gym",
                        ThumbnailUrl = "https://via.placeholder.com/300x200/FF5722/white?text=Home+Gym",
                        CostPerClick = 0.40m,
                        CostPerView = 0.08m,
                        Duration = 25,
                        SkipAfter = 5,
                        Position = null
                    },
                    new Ad
                    {
                        CampaignId = campaigns[2].Id,
                        Type = DTOs.AdType.Video,
                        Content = "Master Python programming in 30 days. Join 10,000+ students who have already transformed their careers!",
                        Url = "https://example.com/python-course",
                        ThumbnailUrl = "https://via.placeholder.com/300x200/2196F3/white?text=Python+Course",
                        CostPerClick = 0.60m,
                        CostPerView = 0.12m,
                        Duration = 35,
                        SkipAfter = 5,
                        Position = null
                    }
                };

                context.Ads.AddRange(sampleAds);
                await context.SaveChangesAsync();
            }
        }
    }
}


