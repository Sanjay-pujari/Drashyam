using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class MentorshipProgram
{
    public int Id { get; set; }
    
    [Required]
    public string MentorId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Requirements { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Benefits { get; set; } = string.Empty;
    
    public MentorshipType Type { get; set; }
    public MentorshipStatus Status { get; set; }
    public MentorshipVisibility Visibility { get; set; }
    
    public int MaxMentees { get; set; } = 5;
    public int CurrentMentees { get; set; } = 0;
    public int DurationWeeks { get; set; } = 12;
    
    public decimal? Fee { get; set; }
    public string? Currency { get; set; }
    public bool IsPaid { get; set; } = false;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    
    public int ViewCount { get; set; } = 0;
    public int ApplicationCount { get; set; } = 0;
    public int Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser Mentor { get; set; } = null!;
    public ICollection<MentorshipApplication> Applications { get; set; } = new List<MentorshipApplication>();
    public ICollection<MentorshipSession> Sessions { get; set; } = new List<MentorshipSession>();
    public ICollection<MentorshipReview> Reviews { get; set; } = new List<MentorshipReview>();
}

public class MentorshipApplication
{
    public int Id { get; set; }
    
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    public string MenteeId { get; set; } = string.Empty;
    
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
    
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    
    [MaxLength(500)]
    public string? RejectionReason { get; set; }
    
    [MaxLength(500)]
    public string? MentorNotes { get; set; }
    
    // Navigation properties
    public MentorshipProgram Program { get; set; } = null!;
    public ApplicationUser Mentee { get; set; } = null!;
}

public class MentorshipSession
{
    public int Id { get; set; }
    
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    public string MenteeId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public SessionType Type { get; set; }
    public SessionStatus Status { get; set; }
    
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int DurationMinutes { get; set; } = 0;
    
    [MaxLength(1000)]
    public string? MeetingUrl { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    [MaxLength(2000)]
    public string? Feedback { get; set; }
    
    public int Rating { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public MentorshipProgram Program { get; set; } = null!;
    public ApplicationUser Mentee { get; set; } = null!;
}

public class MentorshipReview
{
    public int Id { get; set; }
    
    [Required]
    public int ProgramId { get; set; }
    
    [Required]
    public string MenteeId { get; set; } = string.Empty;
    
    [Required]
    public int Rating { get; set; }
    
    [MaxLength(1000)]
    public string? Review { get; set; }
    
    public int MentorRating { get; set; } = 0;
    public int ProgramRating { get; set; } = 0;
    public int ValueRating { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public MentorshipProgram Program { get; set; } = null!;
    public ApplicationUser Mentee { get; set; } = null!;
}

