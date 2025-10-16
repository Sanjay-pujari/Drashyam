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
        CreateMap<Video, VideoDto>();
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
    }
}
