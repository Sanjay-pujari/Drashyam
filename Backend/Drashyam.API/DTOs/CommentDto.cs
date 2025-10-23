using System.Text.Json.Serialization;

namespace Drashyam.API.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int VideoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long LikeCount { get; set; }
    public long ReplyCount { get; set; }
    public bool IsEdited { get; set; }
    public UserDto? User { get; set; }
    [JsonIgnore]
    public CommentDto? ParentComment { get; set; }
    [JsonIgnore]
    public List<CommentDto>? Replies { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public long DislikeCount { get; set; }
}

public class CommentCreateDto
{
    public string Content { get; set; } = string.Empty;
    public int VideoId { get; set; }
    public int? ParentCommentId { get; set; }
}

public class CommentUpdateDto
{
    public string Content { get; set; } = string.Empty;
}
