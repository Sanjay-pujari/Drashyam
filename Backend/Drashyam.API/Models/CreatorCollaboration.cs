using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class CreatorCollaboration
{
    public int Id { get; set; }
    
    [Required]
    public string InitiatorId { get; set; } = string.Empty;
    
    [Required]
    public string CollaboratorId { get; set; } = string.Empty;
    
    public int? VideoId { get; set; }
    public int? ChannelId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public CollaborationType Type { get; set; }
    public CollaborationStatus Status { get; set; }
    public CollaborationRole InitiatorRole { get; set; }
    public CollaborationRole CollaboratorRole { get; set; }
    
    public decimal? RevenueSharePercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? Deadline { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser Initiator { get; set; } = null!;
    public ApplicationUser Collaborator { get; set; } = null!;
    public Video? Video { get; set; }
    public Channel? Channel { get; set; }
    
    public ICollection<CollaborationMessage> Messages { get; set; } = new List<CollaborationMessage>();
    public ICollection<CollaborationAsset> Assets { get; set; } = new List<CollaborationAsset>();
}

