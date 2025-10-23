using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class CollaborationDto
{
    public int Id { get; set; }
    public string InitiatorId { get; set; } = string.Empty;
    public string CollaboratorId { get; set; } = string.Empty;
    public string InitiatorName { get; set; } = string.Empty;
    public string CollaboratorName { get; set; } = string.Empty;
    public string InitiatorAvatar { get; set; } = string.Empty;
    public string CollaboratorAvatar { get; set; } = string.Empty;
    public int? VideoId { get; set; }
    public int? ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CollaborationType Type { get; set; }
    public CollaborationStatus Status { get; set; }
    public CollaborationRole InitiatorRole { get; set; }
    public CollaborationRole CollaboratorRole { get; set; }
    public decimal? RevenueSharePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public int UnreadMessageCount { get; set; }
    public List<CollaborationMessageDto> RecentMessages { get; set; } = new();
    public List<CollaborationAssetDto> Assets { get; set; } = new();
}

public class CollaborationCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string CollaboratorId { get; set; } = string.Empty;
    
    [Required]
    public CollaborationType Type { get; set; }
    
    public CollaborationRole InitiatorRole { get; set; }
    public CollaborationRole CollaboratorRole { get; set; }
    public decimal? RevenueSharePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Deadline { get; set; }
    public int? VideoId { get; set; }
    public int? ChannelId { get; set; }
}

public class CollaborationUpdateDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public CollaborationRole? InitiatorRole { get; set; }
    public CollaborationRole? CollaboratorRole { get; set; }
    public decimal? RevenueSharePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Deadline { get; set; }
}

public class CollaborationResponseDto
{
    [Required]
    public int CollaborationId { get; set; }
    
    [Required]
    public CollaborationStatus Status { get; set; }
    
    [MaxLength(500)]
    public string? Message { get; set; }
}

public class CollaborationMessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAvatar { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
}

public class CollaborationMessageCreateDto
{
    [Required]
    public int CollaborationId { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public MessageType Type { get; set; } = MessageType.Text;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
}

public class CollaborationAssetDto
{
    public int Id { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public AssetType Type { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CollaborationAssetCreateDto
{
    [Required]
    public int CollaborationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string FileUrl { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    public AssetType Type { get; set; }
    public bool IsPublic { get; set; } = false;
}

public class CollaborationFilterDto
{
    public CollaborationStatus? Status { get; set; }
    public CollaborationType? Type { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
