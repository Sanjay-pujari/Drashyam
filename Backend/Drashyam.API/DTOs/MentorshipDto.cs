using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class MentorshipProgramDto
{
    public int Id { get; set; }
    public string MentorId { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public string MentorAvatar { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public string Benefits { get; set; } = string.Empty;
    public MentorshipType Type { get; set; }
    public MentorshipStatus Status { get; set; }
    public MentorshipVisibility Visibility { get; set; }
    public int MaxMentees { get; set; }
    public int CurrentMentees { get; set; }
    public int DurationWeeks { get; set; }
    public decimal? Fee { get; set; }
    public string? Currency { get; set; }
    public bool IsPaid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public int ViewCount { get; set; }
    public int ApplicationCount { get; set; }
    public int Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool HasApplied { get; set; }
    public List<MentorshipApplicationDto> RecentApplications { get; set; } = new();
    public List<MentorshipReviewDto> RecentReviews { get; set; } = new();
}

public class MentorshipProgramCreateDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Requirements { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Benefits { get; set; } = string.Empty;
    
    [Required]
    public MentorshipType Type { get; set; }
    
    public MentorshipVisibility Visibility { get; set; } = MentorshipVisibility.Public;
    
    [Range(1, 50)]
    public int MaxMentees { get; set; } = 5;
    
    [Range(1, 52)]
    public int DurationWeeks { get; set; } = 12;
    
    public decimal? Fee { get; set; }
    public string? Currency { get; set; }
    public bool IsPaid { get; set; } = false;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public DateTime ApplicationDeadline { get; set; }
}

public class MentorshipApplicationDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string MenteeId { get; set; } = string.Empty;
    public string MenteeName { get; set; } = string.Empty;
    public string MenteeAvatar { get; set; } = string.Empty;
    public string Motivation { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Goals { get; set; } = string.Empty;
    public string? PortfolioUrl { get; set; }
    public string? ResumeUrl { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? MentorNotes { get; set; }
}

public class MentorshipApplicationCreateDto
{
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Motivation { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Experience { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Goals { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? PortfolioUrl { get; set; }
    
    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}

public class MentorshipSessionDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string MenteeId { get; set; } = string.Empty;
    public string MenteeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SessionType Type { get; set; }
    public SessionStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Notes { get; set; }
    public string? Feedback { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MentorshipSessionCreateDto
{
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    public string MenteeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public SessionType Type { get; set; }
    
    [Required]
    public DateTime ScheduledAt { get; set; }
    
    [MaxLength(1000)]
    public string? MeetingUrl { get; set; }
}

public class MentorshipReviewDto
{
    public int Id { get; set; }
    public int ProgramId { get; set; }
    public string MenteeId { get; set; } = string.Empty;
    public string MenteeName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Review { get; set; }
    public int MentorRating { get; set; }
    public int ProgramRating { get; set; }
    public int ValueRating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MentorshipReviewCreateDto
{
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    public string MenteeId { get; set; } = string.Empty;
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Review { get; set; }
    
    [Range(1, 5)]
    public int MentorRating { get; set; }
    
    [Range(1, 5)]
    public int ProgramRating { get; set; }
    
    [Range(1, 5)]
    public int ValueRating { get; set; }
}

public class MentorshipFilterDto
{
    public MentorshipType? Type { get; set; }
    public MentorshipStatus? Status { get; set; }
    public MentorshipVisibility? Visibility { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsPaid { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? MaxFee { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class MentorshipAnalyticsDto
{
    public int TotalPrograms { get; set; }
    public int ActivePrograms { get; set; }
    public int CompletedPrograms { get; set; }
    public int TotalApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    public List<MentorshipTypeStats> TypeStats { get; set; } = new();
    public List<MonthlyMentorshipStats> MonthlyStats { get; set; } = new();
}

public class MentorshipTypeStats
{
    public MentorshipType Type { get; set; }
    public int Count { get; set; }
    public int Applications { get; set; }
    public int Sessions { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyMentorshipStats
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Programs { get; set; }
    public int Applications { get; set; }
    public int Sessions { get; set; }
    public decimal Revenue { get; set; }
}
