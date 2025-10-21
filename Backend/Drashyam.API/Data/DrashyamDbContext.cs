using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Data;

public class DrashyamDbContext : IdentityDbContext<ApplicationUser>
{
    public DrashyamDbContext(DbContextOptions<DrashyamDbContext> options) : base(options)
    {
    }

    public DbSet<Video> Videos { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<VideoLike> VideoLikes { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<VideoView> VideoViews { get; set; }
    public DbSet<WatchLater> WatchLater { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<ChannelSubscription> ChannelSubscriptions { get; set; }
    public DbSet<LiveStream> LiveStreams { get; set; }
    public DbSet<Analytics> Analytics { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistVideo> PlaylistVideos { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AdCampaign> AdCampaigns { get; set; }
    public DbSet<AdImpression> AdImpressions { get; set; }
    public DbSet<UserInvite> UserInvites { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<ReferralReward> ReferralRewards { get; set; }
    public DbSet<InviteAnalytics> InviteAnalytics { get; set; }
    public DbSet<ReferralAnalytics> ReferralAnalytics { get; set; }
    public DbSet<InviteEvent> InviteEvents { get; set; }
    public DbSet<ReferralEvent> ReferralEvents { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }
    public DbSet<PremiumVideo> PremiumVideos { get; set; }
    public DbSet<PremiumPurchase> PremiumPurchases { get; set; }
    public DbSet<MerchandiseStore> MerchandiseStores { get; set; }
    public DbSet<MerchandiseItem> MerchandiseItems { get; set; }
    public DbSet<MerchandiseOrder> MerchandiseOrders { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<UserInteraction> UserInteractions { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<TrendingVideo> TrendingVideos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Video entity
        builder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.ShareToken).IsUnique();
            entity.Property(e => e.Duration).HasConversion(
                v => v.Ticks,
                v => TimeSpan.FromTicks(v));
        });

        // Configure Channel entity
        builder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CustomUrl).IsUnique();
        });

        // Configure Comment entity
        builder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ParentCommentId);
        });

        // Configure VideoLike entity
        builder.Entity<VideoLike>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.VideoId }).IsUnique();
        });

        // Configure CommentLike entity
        builder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique();
        });

        // Configure VideoView entity
        builder.Entity<VideoView>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.WatchDuration).HasConversion(
                v => v.Ticks,
                v => TimeSpan.FromTicks(v));
        });

        // Configure WatchLater entity
        builder.Entity<WatchLater>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.VideoId }).IsUnique();
        });

        // Configure Subscription entity
        builder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
        });

        // Configure SubscriptionPlan entity
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Configure ChannelSubscription entity
        builder.Entity<ChannelSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ChannelId }).IsUnique();
        });

        // Configure LiveStream entity
        builder.Entity<LiveStream>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.StreamKey).IsUnique();
        });

        // Configure Analytics entity
        builder.Entity<Analytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.Date);
        });

        // Configure Playlist entity
        builder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ChannelId);
        });

        // Configure PlaylistVideo entity
        builder.Entity<PlaylistVideo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PlaylistId);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => new { e.PlaylistId, e.VideoId }).IsUnique();
        });

        // Configure Notification entity
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure AdCampaign entity
        builder.Entity<AdCampaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AdvertiserId);
            entity.HasIndex(e => e.Status);
        });

        // Configure AdImpression entity
        builder.Entity<AdImpression>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AdCampaignId);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.UserId);
        });

        // Configure UserInvite entity
        builder.Entity<UserInvite>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InviterId);
            entity.HasIndex(e => e.InviteeEmail);
            entity.HasIndex(e => e.InviteToken).IsUnique();
            entity.HasIndex(e => e.AcceptedUserId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Referral entity
        builder.Entity<Referral>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReferrerId);
            entity.HasIndex(e => e.ReferredUserId);
            entity.HasIndex(e => e.ReferralCode);
            entity.HasIndex(e => e.Status);
        });

        // Configure ReferralReward entity
        builder.Entity<ReferralReward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ReferralId);
            entity.HasIndex(e => e.Status);
        });

        // Configure InviteAnalytics entity
        builder.Entity<InviteAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
        });

        // Configure ReferralAnalytics entity
        builder.Entity<ReferralAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.UserId, e.Date }).IsUnique();
        });

        // Configure InviteEvent entity
        builder.Entity<InviteEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InviteId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
        });

        // Configure ReferralEvent entity
        builder.Entity<ReferralEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ReferralId);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
        });

        // Seed data
        SeedData(builder);
    }

    private void SeedData(ModelBuilder builder)
    {
        // Seed Subscription Plans
        builder.Entity<SubscriptionPlan>().HasData(
            new SubscriptionPlan
            {
                Id = 1,
                Name = "Free",
                Description = "Basic features with ads",
                Price = 0,
                BillingCycle = BillingCycle.Monthly,
                MaxChannels = 1,
                MaxVideosPerChannel = 10,
                MaxStorageGB = 1,
                HasAds = true,
                HasAnalytics = false,
                HasMonetization = false,
                HasLiveStreaming = false,
                IsActive = true,
                CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new SubscriptionPlan
            {
                Id = 2,
                Name = "Premium",
                Description = "Ad-free experience with enhanced features",
                Price = 9.99m,
                BillingCycle = BillingCycle.Monthly,
                MaxChannels = 3,
                MaxVideosPerChannel = 100,
                MaxStorageGB = 50,
                HasAds = false,
                HasAnalytics = true,
                HasMonetization = false,
                HasLiveStreaming = true,
                IsActive = true,
                CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            },
            new SubscriptionPlan
            {
                Id = 3,
                Name = "Pro",
                Description = "Full features with monetization",
                Price = 19.99m,
                BillingCycle = BillingCycle.Monthly,
                MaxChannels = 10,
                MaxVideosPerChannel = 1000,
                MaxStorageGB = 500,
                HasAds = false,
                HasAnalytics = true,
                HasMonetization = true,
                HasLiveStreaming = true,
                IsActive = true,
                CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Configure UserSettings entity
        builder.Entity<UserSettings>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.ProfilePublic).HasDefaultValue(true);
            entity.Property(e => e.ShowEmail).HasDefaultValue(false);
            entity.Property(e => e.AllowDataSharing).HasDefaultValue(true);
            entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
            entity.Property(e => e.PushNotifications).HasDefaultValue(true);
            entity.Property(e => e.NewVideoNotifications).HasDefaultValue(true);
            entity.Property(e => e.CommentNotifications).HasDefaultValue(true);
        });

        // Configure PremiumVideo entity
        builder.Entity<PremiumVideo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId).IsUnique();
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
        });

        // Configure PremiumPurchase entity
        builder.Entity<PremiumPurchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PremiumVideoId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.PaymentIntentId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
        });

        // Configure MerchandiseStore entity
        builder.Entity<MerchandiseStore>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.Platform);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsFeatured);
            entity.Property(e => e.StoreName).HasMaxLength(100);
            entity.Property(e => e.StoreUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
        });

        // Configure MerchandiseItem entity
        builder.Entity<MerchandiseItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChannelId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Sizes).HasMaxLength(500);
            entity.Property(e => e.Colors).HasMaxLength(500);
        });

        // Configure MerchandiseOrder entity
        builder.Entity<MerchandiseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MerchandiseItemId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.PaymentIntentId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).HasMaxLength(200);
            entity.Property(e => e.CustomerAddress).HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.PaymentIntentId).HasMaxLength(200);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
        });

        // Configure UserPreference entity
        builder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Tag);
            entity.Property(e => e.Weight).HasPrecision(5, 2);
        });

        // Configure UserInteraction entity
        builder.Entity<UserInteraction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Score).HasPrecision(5, 2);
            entity.Property(e => e.WatchDuration).HasConversion(
                v => v.HasValue ? v.Value.Ticks : (long?)null,
                v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null);
        });

        // Configure Recommendation entity
        builder.Entity<Recommendation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.Score).HasPrecision(5, 2);
        });

        // Configure TrendingVideo entity
        builder.Entity<TrendingVideo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VideoId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.Position);
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.TrendingScore).HasPrecision(10, 4);
        });
    }
}
