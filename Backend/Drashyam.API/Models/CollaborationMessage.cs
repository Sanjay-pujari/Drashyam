using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class CollaborationMessage
{
    public int Id { get; set; }
    
    [Required]
    public int CollaborationId { get; set; }
    
    [Required]
    public string SenderId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public MessageType Type { get; set; } = MessageType.Text;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentType { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    
    // Navigation properties
    public CreatorCollaboration Collaboration { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
}

