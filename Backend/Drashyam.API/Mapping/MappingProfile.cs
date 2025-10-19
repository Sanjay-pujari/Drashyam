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
            .ForMember(dest => dest.ChannelName, opt => opt.MapFrom(src => src.Channel != null ? src.Channel.Name : null));
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
    }
}
