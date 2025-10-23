using AutoMapper;
using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<UserUpdateDto, ApplicationUser>();

        // Video mappings
        CreateMap<Video, VideoDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.UserProfilePicture, opt => opt.MapFrom(src => src.User.ProfilePictureUrl))
            .ForMember(dest => dest.ChannelName, opt => opt.MapFrom(src => src.Channel != null ? src.Channel.Name : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapVideoProcessingStatusToVideoStatus(src.Status)));
        CreateMap<VideoUploadDto, Video>();
        CreateMap<VideoUpdateDto, Video>();

        // Channel mappings
        CreateMap<Channel, ChannelDto>();
        CreateMap<ChannelCreateDto, Channel>();
        CreateMap<ChannelUpdateDto, Channel>();

        // Comment mappings
        CreateMap<Comment, CommentDto>();
        CreateMap<CommentCreateDto, Comment>();
        CreateMap<CommentUpdateDto, Comment>();

        // LiveStream mappings
        CreateMap<LiveStream, LiveStreamDto>();
        CreateMap<LiveStreamCreateDto, LiveStream>();
        CreateMap<LiveStreamUpdateDto, LiveStream>();

        // Subscription mappings
        CreateMap<Subscription, SubscriptionDto>();
        CreateMap<SubscriptionCreateDto, Subscription>();
        CreateMap<SubscriptionUpdateDto, Subscription>();
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>();

        // Analytics mappings
        CreateMap<Analytics, AnalyticsDto>();
        
        // Analytics Dashboard mappings
        CreateMap<AnalyticsDashboard, AnalyticsDashboardDto>();
        CreateMap<VideoAnalytics, VideoAnalyticsDto>();
        CreateMap<RevenueAnalytics, RevenueAnalyticsDto>();
        CreateMap<AudienceAnalytics, AudienceAnalyticsDto>();
        CreateMap<EngagementAnalytics, EngagementAnalyticsDto>();

        // User Settings mappings
        CreateMap<UserSettings, PrivacySettingsDto>();
        CreateMap<UserSettings, NotificationSettingsDto>();

        // Invite mappings
        CreateMap<UserInvite, UserInviteDto>()
            .ForMember(dest => dest.InviterName, opt => opt.MapFrom(src => $"{src.Inviter.FirstName} {src.Inviter.LastName}"))
            .ForMember(dest => dest.AcceptedUserName, opt => opt.MapFrom(src => src.AcceptedUser != null ? $"{src.AcceptedUser.FirstName} {src.AcceptedUser.LastName}" : null));

        // Referral mappings
        CreateMap<Referral, ReferralDto>()
            .ForMember(dest => dest.ReferrerName, opt => opt.MapFrom(src => $"{src.Referrer.FirstName} {src.Referrer.LastName}"))
            .ForMember(dest => dest.ReferredUserName, opt => opt.MapFrom(src => $"{src.ReferredUser.FirstName} {src.ReferredUser.LastName}"));

        // ReferralReward mappings
        CreateMap<ReferralReward, ReferralRewardDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));

        // WatchLater mappings
        CreateMap<WatchLater, WatchLaterDto>();
        CreateMap<WatchLaterCreateDto, WatchLater>();

        // Playlist mappings
        CreateMap<Playlist, PlaylistDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.ChannelName, opt => opt.MapFrom(src => src.Channel != null ? src.Channel.Name : null));
        CreateMap<PlaylistCreateDto, Playlist>();
        CreateMap<PlaylistUpdateDto, Playlist>();
        CreateMap<PlaylistVideo, PlaylistVideoDto>();
        CreateMap<PlaylistVideoCreateDto, PlaylistVideo>();

        // Ad Campaign mappings
        CreateMap<AdCampaign, AdCampaignDto>();
        CreateMap<AdCampaignCreateDto, AdCampaign>();
        CreateMap<AdCampaignUpdateDto, AdCampaign>();
        CreateMap<AdImpression, AdImpressionDto>();

        // Premium Content mappings
        CreateMap<PremiumVideo, PremiumVideoDto>()
            .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.Video.Title))
            .ForMember(dest => dest.VideoThumbnailUrl, opt => opt.MapFrom(src => src.Video.ThumbnailUrl))
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Video.User.FirstName} {src.Video.User.LastName}"));
        CreateMap<PremiumVideoCreateDto, PremiumVideo>();
        CreateMap<PremiumVideoUpdateDto, PremiumVideo>();
        CreateMap<PremiumPurchase, PremiumPurchaseDto>()
            .ForMember(dest => dest.VideoTitle, opt => opt.MapFrom(src => src.PremiumVideo.Video.Title));
        CreateMap<PremiumPurchaseCreateDto, PremiumPurchase>();

        // Merchandise Store mappings
        CreateMap<MerchandiseStore, MerchandiseStoreDto>();
        CreateMap<MerchandiseStoreCreateDto, MerchandiseStore>();
        CreateMap<MerchandiseStoreUpdateDto, MerchandiseStore>();

        // Recommendation mappings
        CreateMap<Recommendation, RecommendationDto>();
        CreateMap<TrendingVideo, TrendingVideoDto>();
        CreateMap<UserPreference, UserPreferenceDto>();
        CreateMap<UserInteraction, InteractionDto>();

        // SuperChat mappings
        CreateMap<SuperChat, SuperChatDto>();
        CreateMap<SuperChatRequestDto, SuperChat>();
        CreateMap<SuperChat, SuperChatDisplayDto>();
        
        // Collaboration mappings
        CreateMap<CreatorCollaboration, CollaborationDto>()
            .ForMember(dest => dest.InitiatorName, opt => opt.MapFrom(src => $"{src.Initiator.FirstName} {src.Initiator.LastName}"))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src => $"{src.Collaborator.FirstName} {src.Collaborator.LastName}"))
            .ForMember(dest => dest.InitiatorAvatar, opt => opt.MapFrom(src => src.Initiator.ProfilePictureUrl))
            .ForMember(dest => dest.CollaboratorAvatar, opt => opt.MapFrom(src => src.Collaborator.ProfilePictureUrl));
        
        CreateMap<CollaborationMessage, CollaborationMessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => $"{src.Sender.FirstName} {src.Sender.LastName}"))
            .ForMember(dest => dest.SenderAvatar, opt => opt.MapFrom(src => src.Sender.ProfilePictureUrl));
        
        CreateMap<CollaborationAsset, CollaborationAssetDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => $"{src.UploadedBy.FirstName} {src.UploadedBy.LastName}"));
        
        // Challenge mappings
        CreateMap<CommunityChallenge, ChallengeDto>()
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => $"{src.Creator.FirstName} {src.Creator.LastName}"))
            .ForMember(dest => dest.CreatorAvatar, opt => opt.MapFrom(src => src.Creator.ProfilePictureUrl));
        
        CreateMap<ChallengeSubmission, ChallengeSubmissionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl));
        
        CreateMap<ChallengeParticipant, ChallengeParticipantDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl));
        
        CreateMap<ChallengeComment, ChallengeCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl));
        
        // Enhanced Comment mappings
        CreateMap<EnhancedComment, EnhancedCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src => src.User.ProfilePictureUrl));
        
        CreateMap<CommentReaction, CommentReactionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
        
        // Mentorship mappings
        CreateMap<MentorshipProgram, MentorshipProgramDto>()
            .ForMember(dest => dest.MentorName, opt => opt.MapFrom(src => $"{src.Mentor.FirstName} {src.Mentor.LastName}"))
            .ForMember(dest => dest.MentorAvatar, opt => opt.MapFrom(src => src.Mentor.ProfilePictureUrl));
        
        CreateMap<MentorshipApplication, MentorshipApplicationDto>()
            .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => $"{src.Mentee.FirstName} {src.Mentee.LastName}"))
            .ForMember(dest => dest.MenteeAvatar, opt => opt.MapFrom(src => src.Mentee.ProfilePictureUrl));
        
        CreateMap<MentorshipSession, MentorshipSessionDto>()
            .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => $"{src.Mentee.FirstName} {src.Mentee.LastName}"));
        
        CreateMap<MentorshipReview, MentorshipReviewDto>()
            .ForMember(dest => dest.MenteeName, opt => opt.MapFrom(src => $"{src.Mentee.FirstName} {src.Mentee.LastName}"));
        
        // Social Sharing mappings
        CreateMap<SocialShare, SocialShareDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
        
        CreateMap<ViralContent, ViralContentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
        
        CreateMap<ContentPromotion, ContentPromotionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
        
        CreateMap<SocialConnection, SocialConnectionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
    }

    private static VideoStatus MapVideoProcessingStatusToVideoStatus(VideoProcessingStatus status)
    {
        return status switch
        {
            VideoProcessingStatus.Processing => VideoStatus.Processing,
            VideoProcessingStatus.Ready => VideoStatus.Published,
            VideoProcessingStatus.Failed => VideoStatus.Processing, // Treat failed as processing for now
            VideoProcessingStatus.Deleted => VideoStatus.Deleted,
            _ => VideoStatus.Processing
        };
    }
}
