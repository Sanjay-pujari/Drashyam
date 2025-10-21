namespace Drashyam.API.DTOs;

public class RecommendationDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public VideoDto Video { get; set; } = null!;
}

public class TrendingVideoDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string? Category { get; set; }
    public string? Country { get; set; }
    public decimal TrendingScore { get; set; }
    public int Position { get; set; }
    public DateTime CalculatedAt { get; set; }
    public VideoDto Video { get; set; } = null!;
}

public class UserPreferenceDto
{
    public int Id { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public decimal Weight { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecommendationRequestDto
{
    public int? Limit { get; set; } = 20;
    public string? Category { get; set; }
    public string? Type { get; set; }
    public bool IncludeTrending { get; set; } = true;
    public bool IncludePersonalized { get; set; } = true;
}

public class InteractionDto
{
    public int VideoId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public TimeSpan? WatchDuration { get; set; }
}

public class RecommendationFeedbackDto
{
    public int RecommendationId { get; set; }
    public bool IsClicked { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public TimeSpan? WatchDuration { get; set; }
}
