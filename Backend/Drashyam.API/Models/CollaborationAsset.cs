using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class CollaborationAsset
{
    public int Id { get; set; }
    
    [Required]
    public int CollaborationId { get; set; }
    
    [Required]
    public string UploadedById { get; set; } = string.Empty;
    
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
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public CreatorCollaboration Collaboration { get; set; } = null!;
    public ApplicationUser UploadedBy { get; set; } = null!;
}

