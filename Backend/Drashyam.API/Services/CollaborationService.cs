using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public class CollaborationService : ICollaborationService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CollaborationService> _logger;
    private readonly INotificationService _notificationService;

    public CollaborationService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<CollaborationService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<CollaborationDto> CreateCollaborationAsync(CollaborationCreateDto createDto, string userId)
    {
        try
        {
            // Check if collaborator exists
            var collaborator = await _context.Users.FindAsync(createDto.CollaboratorId);
            if (collaborator == null)
            {
                throw new ArgumentException("Collaborator not found");
            }

            // Check if user is trying to collaborate with themselves
            if (userId == createDto.CollaboratorId)
            {
                throw new ArgumentException("Cannot collaborate with yourself");
            }

            // Check for existing pending collaboration
            var existingCollaboration = await _context.CreatorCollaborations
                .FirstOrDefaultAsync(c => c.InitiatorId == userId && 
                                        c.CollaboratorId == createDto.CollaboratorId && 
                                        c.Status == CollaborationStatus.Pending);

            if (existingCollaboration != null)
            {
                throw new InvalidOperationException("A pending collaboration already exists with this user");
            }

            var collaboration = new CreatorCollaboration
            {
                InitiatorId = userId,
                CollaboratorId = createDto.CollaboratorId,
                Title = createDto.Title,
                Description = createDto.Description,
                Type = createDto.Type,
                InitiatorRole = createDto.InitiatorRole,
                CollaboratorRole = createDto.CollaboratorRole,
                RevenueSharePercentage = createDto.RevenueSharePercentage,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                Deadline = createDto.Deadline,
                VideoId = createDto.VideoId,
                ChannelId = createDto.ChannelId,
                Status = CollaborationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.CreatorCollaborations.Add(collaboration);
            await _context.SaveChangesAsync();

            // Send notification to collaborator
            // TODO: Implement notification service
            // await _notificationService.SendNotificationAsync(
            //     createDto.CollaboratorId,
            //     "New Collaboration Request",
            //     $"You have received a collaboration request from {collaboration.Initiator.UserName}",
            //     NotificationType.CollaborationRequest
            // );

            _logger.LogInformation("Collaboration created: {CollaborationId} by {UserId}", collaboration.Id, userId);

            return await GetCollaborationByIdAsync(collaboration.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating collaboration for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CollaborationDto> GetCollaborationByIdAsync(int id, string userId)
    {
        var collaboration = await _context.CreatorCollaborations
            .Include(c => c.Initiator)
            .Include(c => c.Collaborator)
            .Include(c => c.Video)
            .Include(c => c.Channel)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(5))
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id && (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found");
        }

        var dto = _mapper.Map<CollaborationDto>(collaboration);
        
        // Calculate unread message count
        dto.UnreadMessageCount = await _context.CollaborationMessages
            .CountAsync(m => m.CollaborationId == id && 
                           m.SenderId != userId && 
                           !m.IsRead);

        return dto;
    }

    public async Task<PagedResult<CollaborationDto>> GetUserCollaborationsAsync(string userId, CollaborationFilterDto filter)
    {
        var query = _context.CreatorCollaborations
            .Include(c => c.Initiator)
            .Include(c => c.Collaborator)
            .Include(c => c.Video)
            .Include(c => c.Channel)
            .Where(c => c.InitiatorId == userId || c.CollaboratorId == userId);

        // Apply filters
        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(c => c.Type == filter.Type.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => c.Title.Contains(filter.SearchTerm) || 
                                   c.Description.Contains(filter.SearchTerm));
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filter.EndDate.Value);
        }

        var totalCount = await query.CountAsync();

        var collaborations = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<CollaborationDto>>(collaborations);

        return new PagedResult<CollaborationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }

    public async Task<PagedResult<CollaborationDto>> GetPendingCollaborationsAsync(string userId, CollaborationFilterDto filter)
    {
        var query = _context.CreatorCollaborations
            .Include(c => c.Initiator)
            .Include(c => c.Collaborator)
            .Where(c => c.CollaboratorId == userId && c.Status == CollaborationStatus.Pending);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => c.Title.Contains(filter.SearchTerm) || 
                                   c.Description.Contains(filter.SearchTerm));
        }

        var totalCount = await query.CountAsync();

        var collaborations = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<CollaborationDto>>(collaborations);

        return new PagedResult<CollaborationDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }

    public async Task<CollaborationDto> UpdateCollaborationAsync(int id, CollaborationUpdateDto updateDto, string userId)
    {
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == id && c.InitiatorId == userId);

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found or you don't have permission to update it");
        }

        if (collaboration.Status != CollaborationStatus.Pending)
        {
            throw new InvalidOperationException("Cannot update collaboration that has been responded to");
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateDto.Title))
            collaboration.Title = updateDto.Title;

        if (!string.IsNullOrEmpty(updateDto.Description))
            collaboration.Description = updateDto.Description;

        if (updateDto.InitiatorRole.HasValue)
            collaboration.InitiatorRole = updateDto.InitiatorRole.Value;

        if (updateDto.CollaboratorRole.HasValue)
            collaboration.CollaboratorRole = updateDto.CollaboratorRole.Value;

        if (updateDto.RevenueSharePercentage.HasValue)
            collaboration.RevenueSharePercentage = updateDto.RevenueSharePercentage.Value;

        if (updateDto.StartDate.HasValue)
            collaboration.StartDate = updateDto.StartDate.Value;

        if (updateDto.EndDate.HasValue)
            collaboration.EndDate = updateDto.EndDate.Value;

        if (updateDto.Deadline.HasValue)
            collaboration.Deadline = updateDto.Deadline.Value;

        collaboration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetCollaborationByIdAsync(id, userId);
    }

    public async Task<bool> RespondToCollaborationAsync(CollaborationResponseDto responseDto, string userId)
    {
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == responseDto.CollaborationId && c.CollaboratorId == userId);

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found");
        }

        if (collaboration.Status != CollaborationStatus.Pending)
        {
            throw new InvalidOperationException("Collaboration has already been responded to");
        }

        collaboration.Status = responseDto.Status;
        collaboration.RespondedAt = DateTime.UtcNow;
        collaboration.UpdatedAt = DateTime.UtcNow;

        // Add response message if provided
        if (!string.IsNullOrEmpty(responseDto.Message))
        {
            var message = new CollaborationMessage
            {
                CollaborationId = collaboration.Id,
                SenderId = userId,
                Content = responseDto.Message,
                Type = MessageType.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.CollaborationMessages.Add(message);
        }

        await _context.SaveChangesAsync();

        // Send notification to initiator
        var notificationMessage = responseDto.Status == CollaborationStatus.Accepted 
            ? "Your collaboration request has been accepted!" 
            : "Your collaboration request has been declined.";

        // TODO: Implement notification service
        // await _notificationService.SendNotificationAsync(
        //     collaboration.InitiatorId,
        //     "Collaboration Response",
        //     notificationMessage,
        //     NotificationType.CollaborationResponse
        // );

        _logger.LogInformation("Collaboration {CollaborationId} responded to with status {Status}", 
            collaboration.Id, responseDto.Status);

        return true;
    }

    public async Task<bool> CancelCollaborationAsync(int id, string userId)
    {
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == id && (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found");
        }

        if (collaboration.Status == CollaborationStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed collaboration");
        }

        collaboration.Status = CollaborationStatus.Cancelled;
        collaboration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send notification to the other party
        var otherUserId = collaboration.InitiatorId == userId ? collaboration.CollaboratorId : collaboration.InitiatorId;
        // TODO: Implement notification service
        // await _notificationService.SendNotificationAsync(
        //     otherUserId,
        //     "Collaboration Cancelled",
        //     "A collaboration has been cancelled",
        //     NotificationType.CollaborationCancelled
        // );

        return true;
    }

    public async Task<bool> CompleteCollaborationAsync(int id, string userId)
    {
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == id && (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found");
        }

        if (collaboration.Status != CollaborationStatus.InProgress)
        {
            throw new InvalidOperationException("Only in-progress collaborations can be completed");
        }

        collaboration.Status = CollaborationStatus.Completed;
        collaboration.EndDate = DateTime.UtcNow;
        collaboration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send notification to the other party
        var otherUserId = collaboration.InitiatorId == userId ? collaboration.CollaboratorId : collaboration.InitiatorId;
        // TODO: Implement notification service
        // await _notificationService.SendNotificationAsync(
        //     otherUserId,
        //     "Collaboration Completed",
        //     "A collaboration has been marked as completed",
        //     NotificationType.CollaborationCompleted
        // );

        return true;
    }

    public async Task<CollaborationMessageDto> SendMessageAsync(CollaborationMessageCreateDto createDto, string userId)
    {
        // Verify user has access to this collaboration
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == createDto.CollaborationId && 
                                   (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found or access denied");
        }

        var message = new CollaborationMessage
        {
            CollaborationId = createDto.CollaborationId,
            SenderId = userId,
            Content = createDto.Content,
            Type = createDto.Type,
            AttachmentUrl = createDto.AttachmentUrl,
            AttachmentType = createDto.AttachmentType,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollaborationMessages.Add(message);
        await _context.SaveChangesAsync();

        // Send notification to the other party
        var otherUserId = collaboration.InitiatorId == userId ? collaboration.CollaboratorId : collaboration.InitiatorId;
        // TODO: Implement notification service
        // await _notificationService.SendNotificationAsync(
        //     otherUserId,
        //     "New Collaboration Message",
        //     "You have received a new message in a collaboration",
        //     NotificationType.CollaborationMessage
        // );

        return _mapper.Map<CollaborationMessageDto>(message);
    }

    public async Task<PagedResult<CollaborationMessageDto>> GetCollaborationMessagesAsync(int collaborationId, string userId, int page = 1, int pageSize = 50)
    {
        // Verify user has access to this collaboration
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == collaborationId && 
                                   (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found or access denied");
        }

        var query = _context.CollaborationMessages
            .Include(m => m.Sender)
            .Where(m => m.CollaborationId == collaborationId);

        var totalCount = await query.CountAsync();

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<CollaborationMessageDto>>(messages);

        return new PagedResult<CollaborationMessageDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<bool> MarkMessagesAsReadAsync(int collaborationId, string userId)
    {
        var messages = await _context.CollaborationMessages
            .Where(m => m.CollaborationId == collaborationId && 
                       m.SenderId != userId && 
                       !m.IsRead)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadMessageCountAsync(string userId)
    {
        return await _context.CollaborationMessages
            .Include(m => m.Collaboration)
            .CountAsync(m => (m.Collaboration.InitiatorId == userId || m.Collaboration.CollaboratorId == userId) &&
                           m.SenderId != userId && 
                           !m.IsRead);
    }

    public async Task<CollaborationAssetDto> UploadAssetAsync(CollaborationAssetCreateDto createDto, string userId)
    {
        // Verify user has access to this collaboration
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == createDto.CollaborationId && 
                                   (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found or access denied");
        }

        var asset = new CollaborationAsset
        {
            CollaborationId = createDto.CollaborationId,
            UploadedById = userId,
            Name = createDto.Name,
            Description = createDto.Description,
            FileUrl = createDto.FileUrl,
            FileType = createDto.FileType,
            FileSize = createDto.FileSize,
            Type = createDto.Type,
            IsPublic = createDto.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        _context.CollaborationAssets.Add(asset);
        await _context.SaveChangesAsync();

        return _mapper.Map<CollaborationAssetDto>(asset);
    }

    public async Task<List<CollaborationAssetDto>> GetCollaborationAssetsAsync(int collaborationId, string userId)
    {
        // Verify user has access to this collaboration
        var collaboration = await _context.CreatorCollaborations
            .FirstOrDefaultAsync(c => c.Id == collaborationId && 
                                   (c.InitiatorId == userId || c.CollaboratorId == userId));

        if (collaboration == null)
        {
            throw new ArgumentException("Collaboration not found or access denied");
        }

        var assets = await _context.CollaborationAssets
            .Include(a => a.UploadedBy)
            .Where(a => a.CollaborationId == collaborationId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<CollaborationAssetDto>>(assets);
    }

    public async Task<bool> DeleteAssetAsync(int assetId, string userId)
    {
        var asset = await _context.CollaborationAssets
            .Include(a => a.Collaboration)
            .FirstOrDefaultAsync(a => a.Id == assetId && 
                                    (a.Collaboration.InitiatorId == userId || a.Collaboration.CollaboratorId == userId));

        if (asset == null)
        {
            throw new ArgumentException("Asset not found or access denied");
        }

        _context.CollaborationAssets.Remove(asset);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CollaborationDto>> GetSuggestedCollaboratorsAsync(string userId, int count = 10)
    {
        // Get users who have similar content or are in the same category
        var userVideos = await _context.Videos
            .Where(v => v.UserId == userId)
            .Select(v => v.Category)
            .Distinct()
            .ToListAsync();

        var suggestedUsers = await _context.Users
            .Where(u => u.Id != userId)
            .Where(u => _context.Videos.Any(v => v.UserId == u.Id && userVideos.Contains(v.Category)))
            .Take(count)
            .ToListAsync();

        // This is a simplified suggestion algorithm
        // In a real implementation, you'd use more sophisticated matching
        return new List<CollaborationDto>();
    }

    public async Task<List<CollaborationDto>> GetPublicCollaborationsAsync(CollaborationFilterDto filter)
    {
        var query = _context.CreatorCollaborations
            .Include(c => c.Initiator)
            .Include(c => c.Collaborator)
            .Where(c => c.Status == CollaborationStatus.Accepted);

        if (filter.Type.HasValue)
        {
            query = query.Where(c => c.Type == filter.Type.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => c.Title.Contains(filter.SearchTerm) || 
                                   c.Description.Contains(filter.SearchTerm));
        }

        var collaborations = await query
            .OrderByDescending(c => c.CreatedAt)
            .Take(filter.PageSize)
            .ToListAsync();

        return _mapper.Map<List<CollaborationDto>>(collaborations);
    }

    public async Task<bool> IsUserAvailableForCollaborationAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var conflictingCollaborations = await _context.CreatorCollaborations
            .Where(c => (c.InitiatorId == userId || c.CollaboratorId == userId) &&
                       c.Status == CollaborationStatus.Accepted &&
                       ((c.StartDate <= endDate && c.EndDate >= startDate) ||
                        (c.StartDate == null && c.EndDate == null)))
            .AnyAsync();

        return !conflictingCollaborations;
    }

    public async Task<CollaborationAnalyticsDto> GetCollaborationAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.CreatorCollaborations
            .Where(c => c.InitiatorId == userId || c.CollaboratorId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        var collaborations = await query.ToListAsync();

        var analytics = new CollaborationAnalyticsDto
        {
            TotalCollaborations = collaborations.Count,
            ActiveCollaborations = collaborations.Count(c => c.Status == CollaborationStatus.InProgress),
            CompletedCollaborations = collaborations.Count(c => c.Status == CollaborationStatus.Completed),
            PendingInvitations = collaborations.Count(c => c.CollaboratorId == userId && c.Status == CollaborationStatus.Pending),
            TotalRevenueShared = collaborations.Where(c => c.RevenueSharePercentage.HasValue).Sum(c => c.RevenueSharePercentage ?? 0)
        };

        // Type statistics
        analytics.TypeStats = collaborations
            .GroupBy(c => c.Type)
            .Select(g => new CollaborationTypeStats
            {
                Type = g.Key,
                Count = g.Count(),
                Revenue = g.Where(c => c.RevenueSharePercentage.HasValue).Sum(c => c.RevenueSharePercentage ?? 0)
            })
            .ToList();

        // Monthly statistics
        analytics.MonthlyStats = collaborations
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new MonthlyCollaborationStats
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Collaborations = g.Count(),
                Revenue = g.Where(c => c.RevenueSharePercentage.HasValue).Sum(c => c.RevenueSharePercentage ?? 0)
            })
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToList();

        return analytics;
    }
}
