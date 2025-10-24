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
    Comment,
    CollaborationRequest,
    CollaborationResponse,
    CollaborationCancelled,
    CollaborationCompleted,
    CollaborationMessage,
    ChallengeCreated,
    ChallengeJoined,
    ChallengeSubmission,
    ChallengeVote,
    MentorshipApplication,
    MentorshipAccepted,
    MentorshipSession,
    SocialShare,
    ViralContent
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
    Paused,
    Ended,
    Cancelled
}

// Enhanced LiveStreaming Enums
public enum ChatMessageType
{
    Text,
    Emoji,
    Donation,
    SuperChat,
    System,
    Moderator
}


public enum SuperChatTier
{
    Bronze,
    Silver,
    Gold,
    Diamond,
    Platinum
}

public enum SubscriptionTier
{
    Basic,
    Premium,
    VIP,
    Elite
}

public enum EventType
{
    LiveStream,
    Webinar,
    Workshop,
    QnA,
    Gaming,
    Music,
    Art,
    Education,
    Entertainment,
    Sports,
    News,
    Other
}

public enum EventStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled,
    Postponed
}

public enum AttendanceStatus
{
    Registered,
    CheckedIn,
    CheckedOut,
    NoShow,
    Cancelled
}



// Collaboration System Enums
public enum CollaborationType
{
    VideoCreation,
    LiveStream,
    ChannelPartnership,
    ContentSeries,
    EventCollaboration,
    CrossPromotion
}

public enum CollaborationStatus
{
    Pending,
    Accepted,
    Declined,
    InProgress,
    Completed,
    Cancelled,
    Expired
}

public enum CollaborationRole
{
    Creator,
    Editor,
    Producer,
    CoHost,
    Guest,
    Sponsor,
    Partner
}

public enum MessageType
{
    Text,
    Image,
    Video,
    Audio,
    Document,
    Link
}

public enum AssetType
{
    Video,
    Audio,
    Image,
    Document,
    Script,
    Storyboard,
    Reference,
    Other
}

// Community Challenges Enums
public enum ChallengeType
{
    HashtagChallenge,
    VideoContest,
    LiveStreamEvent,
    CommunityEvent,
    SeasonalCampaign,
    BrandPartnership,
    EducationalSeries,
    CreativeChallenge
}

public enum ChallengeStatus
{
    Draft,
    Published,
    Active,
    Voting,
    Completed,
    Cancelled,
    Expired
}

public enum ChallengeVisibility
{
    Public,
    Private,
    InviteOnly,
    CommunityOnly
}

public enum ParticipationStatus
{
    Joined,
    Active,
    Completed,
    Withdrawn,
    Disqualified
}

public enum SubmissionStatus
{
    Pending,
    Approved,
    Rejected,
    UnderReview,
    Published
}

public enum VoteType
{
    Upvote,
    Downvote,
    Favorite,
    Report
}

// Enhanced Comment System Enums
public enum ReactionType
{
    Like,
    Love,
    Laugh,
    Angry,
    Sad,
    Wow,
    Dislike
}

public enum CommentVisibility
{
    Public,
    Private,
    Hidden,
    Deleted
}

public enum CommentStatus
{
    Pending,
    Approved,
    Rejected,
    Flagged,
    UnderReview
}

public enum ModerationAction
{
    Approve,
    Reject,
    Hide,
    Delete,
    Flag,
    Unflag,
    Pin,
    Unpin,
    Highlight,
    Unhighlight
}

// Mentorship Program Enums
public enum MentorshipType
{
    VideoCreation,
    LiveStreaming,
    ChannelGrowth,
    ContentStrategy,
    TechnicalSkills,
    BusinessDevelopment,
    BrandBuilding,
    Monetization,
    General
}

public enum MentorshipStatus
{
    Draft,
    Published,
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum MentorshipVisibility
{
    Public,
    Private,
    InviteOnly
}

public enum ApplicationStatus
{
    Pending,
    UnderReview,
    Accepted,
    Rejected,
    Withdrawn
}

public enum SessionType
{
    VideoCall,
    LiveStream,
    Workshop,
    QandA,
    Review,
    Planning,
    Feedback
}

public enum SessionStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Rescheduled
}

// Social Sharing Enums
public enum SharePlatform
{
    Facebook,
    Twitter,
    Instagram,
    LinkedIn,
    TikTok,
    YouTube,
    Reddit,
    Pinterest,
    WhatsApp,
    Telegram,
    Email,
    SMS,
    Other
}

public enum ViralStatus
{
    Detected,
    Growing,
    Peaking,
    Declining,
    Ended
}

public enum ViralTrigger
{
    Organic,
    Influencer,
    Trending,
    Algorithm,
    CrossPromotion,
    Paid
}

public enum PromotionType
{
    Boost,
    Sponsored,
    Influencer,
    CrossPromotion,
    PaidAds,
    Organic
}

public enum PromotionStatus
{
    Draft,
    Scheduled,
    Active,
    Paused,
    Completed,
    Cancelled
}

public enum StreamingStatus
{
    Created,
    Active,
    Inactive,
    Error,
    Processing,
    Stopped
}