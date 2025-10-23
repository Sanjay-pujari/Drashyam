namespace Drashyam.API.DTOs;

public enum VideoStatus
{
    Draft,
    Processing,
    Published,
    Private,
    Unlisted,
    Deleted
}

public enum VideoType
{
    Uploaded,
    Short,
    Live,
    Premier
}

public enum VideoVisibility
{
    Public,
    Private,
    Unlisted
}

public enum ChannelType
{
    Personal,
    Business,
    Creator,
    Brand
}

public enum SubscriptionType
{
    Free,
    Premium,
    Pro
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Yearly
}

public enum VideoProcessingStatus
{
    Processing,
    Ready,
    Failed,
    Deleted
}

public enum LikeType
{
    Like,
    Dislike
}

public enum PlaylistVisibility
{
    Public,
    Private,
    Unlisted
}

public enum NotificationType
{
    VideoUploaded,
    CommentAdded,
    LikeReceived,
    Subscription,
    System,
    LiveStreamStarted,
    LiveStreamEnded,
    PaymentReceived,
    PaymentFailed,
    SubscriptionExpired,
    SubscriptionRenewed,
    ReferralReward,
    InviteAccepted,
    Email,
    Push,
    Video,
    Comment
}

public enum AdType
{
    Banner,
    Video,
    Overlay,
    Sponsored
}

public enum AdStatus
{
    Draft,
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum SubscriptionStatus
{
    Active,
    Expired,
    Cancelled,
    Suspended
}

public enum InteractionType
{
    View,
    Like,
    Dislike,
    Comment,
    Share,
    Subscribe,
    Unsubscribe
}

public enum RecommendationType
{
    SimilarVideos,
    Trending,
    Personalized,
    ChannelBased,
    CategoryBased
}

public enum MerchandiseOrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned
}

public enum StorePlatform
{
    Shopify,
    WooCommerce,
    Amazon,
    Etsy,
    Custom
}

public enum PremiumPurchaseStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}

public enum InviteEventType
{
    Created,
    Sent,
    Opened,
    Clicked,
    Accepted,
    Expired,
    Cancelled,
    Resent
}

public enum ReferralEventType
{
    Created,
    CodeGenerated,
    CodeUsed,
    Completed,
    Rewarded,
    Claimed,
    Expired
}

public enum RewardStatus
{
    Pending,
    Claimed,
    Expired,
    Cancelled
}

public enum InviteStatus
{
    Pending,
    Accepted,
    Expired,
    Cancelled
}

public enum InviteType
{
    Email,
    Link,
    Code,
    DirectLink
}

public enum ReferralStatus
{
    Pending,
    Completed,
    Rewarded,
    Cancelled
}

public enum LiveStreamStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled
}