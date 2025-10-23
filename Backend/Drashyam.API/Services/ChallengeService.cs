using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public class ChallengeService : IChallengeService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ChallengeService> _logger;
    private readonly INotificationService _notificationService;

    public ChallengeService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<ChallengeService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<ChallengeDto> CreateChallengeAsync(ChallengeCreateDto createDto, string userId)
    {
        try
        {
            var challenge = new CommunityChallenge
            {
                CreatorId = userId,
                Title = createDto.Title,
                Description = createDto.Description,
                Hashtag = createDto.Hashtag,
                Type = createDto.Type,
                Visibility = createDto.Visibility,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                VotingEndDate = createDto.VotingEndDate,
                MaxParticipants = createDto.MaxParticipants,
                MinVideoLength = createDto.MinVideoLength,
                MaxVideoLength = createDto.MaxVideoLength,
                Rules = createDto.Rules,
                Prizes = createDto.Prizes,
                PrizeAmount = createDto.PrizeAmount,
                PrizeCurrency = createDto.PrizeCurrency,
                ThumbnailUrl = createDto.ThumbnailUrl,
                BannerUrl = createDto.BannerUrl,
                AllowVoting = createDto.AllowVoting,
                AllowComments = createDto.AllowComments,
                RequireApproval = createDto.RequireApproval,
                Status = ChallengeStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommunityChallenges.Add(challenge);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Challenge created: {ChallengeId} by {UserId}", challenge.Id, userId);

            return await GetChallengeByIdAsync(challenge.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating challenge for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ChallengeDto> GetChallengeByIdAsync(int id, string? userId = null)
    {
        var challenge = await _context.CommunityChallenges
            .Include(c => c.Creator)
            .Include(c => c.Participants.Take(5))
            .Include(c => c.Submissions.OrderByDescending(s => s.VoteCount).Take(5))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found");
        }

        var dto = _mapper.Map<ChallengeDto>(challenge);
        
        // Check if user is participant
        if (!string.IsNullOrEmpty(userId))
        {
            dto.IsParticipant = await _context.ChallengeParticipants
                .AnyAsync(p => p.ChallengeId == id && p.UserId == userId);
            
            dto.HasSubmitted = await _context.ChallengeSubmissions
                .AnyAsync(s => s.ChallengeId == id && s.UserId == userId);
        }

        return dto;
    }

    public async Task<PagedResult<ChallengeDto>> GetChallengesAsync(ChallengeFilterDto filter, string? userId = null)
    {
        var query = _context.CommunityChallenges
            .Include(c => c.Creator)
            .Where(c => c.Status != ChallengeStatus.Draft);

        // Apply filters
        if (filter.Type.HasValue)
        {
            query = query.Where(c => c.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(c => c.Status == filter.Status.Value);
        }

        if (filter.Visibility.HasValue)
        {
            query = query.Where(c => c.Visibility == filter.Visibility.Value);
        }

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(c => c.Title.Contains(filter.SearchTerm) || 
                                   c.Description.Contains(filter.SearchTerm));
        }

        if (!string.IsNullOrEmpty(filter.Hashtag))
        {
            query = query.Where(c => c.Hashtag.Contains(filter.Hashtag));
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(c => c.StartDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(c => c.EndDate <= filter.EndDate.Value);
        }

        if (filter.IsParticipant.HasValue && !string.IsNullOrEmpty(userId))
        {
            if (filter.IsParticipant.Value)
            {
                query = query.Where(c => c.Participants.Any(p => p.UserId == userId));
            }
            else
            {
                query = query.Where(c => !c.Participants.Any(p => p.UserId == userId));
            }
        }

        if (filter.HasPrize.HasValue)
        {
            if (filter.HasPrize.Value)
            {
                query = query.Where(c => c.PrizeAmount.HasValue && c.PrizeAmount > 0);
            }
            else
            {
                query = query.Where(c => !c.PrizeAmount.HasValue || c.PrizeAmount == 0);
            }
        }

        var totalCount = await query.CountAsync();

        var challenges = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ChallengeDto>>(challenges);

        // Set participation status for each challenge
        if (!string.IsNullOrEmpty(userId))
        {
            foreach (var dto in dtos)
            {
                dto.IsParticipant = await _context.ChallengeParticipants
                    .AnyAsync(p => p.ChallengeId == dto.Id && p.UserId == userId);
                
                dto.HasSubmitted = await _context.ChallengeSubmissions
                    .AnyAsync(s => s.ChallengeId == dto.Id && s.UserId == userId);
            }
        }

        return new PagedResult<ChallengeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }

    public async Task<PagedResult<ChallengeDto>> GetUserChallengesAsync(string userId, ChallengeFilterDto filter)
    {
        var query = _context.CommunityChallenges
            .Include(c => c.Creator)
            .Where(c => c.CreatorId == userId);

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

        var totalCount = await query.CountAsync();

        var challenges = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ChallengeDto>>(challenges);

        return new PagedResult<ChallengeDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }

    public async Task<ChallengeDto> UpdateChallengeAsync(int id, ChallengeUpdateDto updateDto, string userId)
    {
        var challenge = await _context.CommunityChallenges
            .FirstOrDefaultAsync(c => c.Id == id && c.CreatorId == userId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found or you don't have permission to update it");
        }

        if (challenge.Status == ChallengeStatus.Active || challenge.Status == ChallengeStatus.Completed)
        {
            throw new InvalidOperationException("Cannot update an active or completed challenge");
        }

        // Update fields
        if (!string.IsNullOrEmpty(updateDto.Title))
            challenge.Title = updateDto.Title;

        if (!string.IsNullOrEmpty(updateDto.Description))
            challenge.Description = updateDto.Description;

        if (!string.IsNullOrEmpty(updateDto.Hashtag))
            challenge.Hashtag = updateDto.Hashtag;

        if (updateDto.Visibility.HasValue)
            challenge.Visibility = updateDto.Visibility.Value;

        if (updateDto.StartDate.HasValue)
            challenge.StartDate = updateDto.StartDate.Value;

        if (updateDto.EndDate.HasValue)
            challenge.EndDate = updateDto.EndDate.Value;

        if (updateDto.VotingEndDate.HasValue)
            challenge.VotingEndDate = updateDto.VotingEndDate.Value;

        if (updateDto.MaxParticipants.HasValue)
            challenge.MaxParticipants = updateDto.MaxParticipants.Value;

        if (updateDto.MinVideoLength.HasValue)
            challenge.MinVideoLength = updateDto.MinVideoLength.Value;

        if (updateDto.MaxVideoLength.HasValue)
            challenge.MaxVideoLength = updateDto.MaxVideoLength.Value;

        if (!string.IsNullOrEmpty(updateDto.Rules))
            challenge.Rules = updateDto.Rules;

        if (!string.IsNullOrEmpty(updateDto.Prizes))
            challenge.Prizes = updateDto.Prizes;

        if (updateDto.PrizeAmount.HasValue)
            challenge.PrizeAmount = updateDto.PrizeAmount.Value;

        if (!string.IsNullOrEmpty(updateDto.PrizeCurrency))
            challenge.PrizeCurrency = updateDto.PrizeCurrency;

        if (!string.IsNullOrEmpty(updateDto.ThumbnailUrl))
            challenge.ThumbnailUrl = updateDto.ThumbnailUrl;

        if (!string.IsNullOrEmpty(updateDto.BannerUrl))
            challenge.BannerUrl = updateDto.BannerUrl;

        if (updateDto.AllowVoting.HasValue)
            challenge.AllowVoting = updateDto.AllowVoting.Value;

        if (updateDto.AllowComments.HasValue)
            challenge.AllowComments = updateDto.AllowComments.Value;

        if (updateDto.RequireApproval.HasValue)
            challenge.RequireApproval = updateDto.RequireApproval.Value;

        challenge.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetChallengeByIdAsync(id, userId);
    }

    public async Task<bool> DeleteChallengeAsync(int id, string userId)
    {
        var challenge = await _context.CommunityChallenges
            .FirstOrDefaultAsync(c => c.Id == id && c.CreatorId == userId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found or you don't have permission to delete it");
        }

        if (challenge.Status == ChallengeStatus.Active)
        {
            throw new InvalidOperationException("Cannot delete an active challenge");
        }

        _context.CommunityChallenges.Remove(challenge);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PublishChallengeAsync(int id, string userId)
    {
        var challenge = await _context.CommunityChallenges
            .FirstOrDefaultAsync(c => c.Id == id && c.CreatorId == userId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found or you don't have permission to publish it");
        }

        if (challenge.Status != ChallengeStatus.Draft)
        {
            throw new InvalidOperationException("Only draft challenges can be published");
        }

        challenge.Status = ChallengeStatus.Published;
        challenge.PublishedAt = DateTime.UtcNow;
        challenge.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelChallengeAsync(int id, string userId)
    {
        var challenge = await _context.CommunityChallenges
            .FirstOrDefaultAsync(c => c.Id == id && c.CreatorId == userId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found or you don't have permission to cancel it");
        }

        if (challenge.Status == ChallengeStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed challenge");
        }

        challenge.Status = ChallengeStatus.Cancelled;
        challenge.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> JoinChallengeAsync(int challengeId, string userId)
    {
        var challenge = await _context.CommunityChallenges.FindAsync(challengeId);
        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found");
        }

        if (challenge.Status != ChallengeStatus.Published && challenge.Status != ChallengeStatus.Active)
        {
            throw new InvalidOperationException("Challenge is not available for participation");
        }

        if (challenge.MaxParticipants.HasValue && challenge.ParticipantCount >= challenge.MaxParticipants.Value)
        {
            throw new InvalidOperationException("Challenge has reached maximum participants");
        }

        var existingParticipant = await _context.ChallengeParticipants
            .FirstOrDefaultAsync(p => p.ChallengeId == challengeId && p.UserId == userId);

        if (existingParticipant != null)
        {
            throw new InvalidOperationException("You are already participating in this challenge");
        }

        var participant = new ChallengeParticipant
        {
            ChallengeId = challengeId,
            UserId = userId,
            Status = ParticipationStatus.Joined,
            JoinedAt = DateTime.UtcNow
        };

        _context.ChallengeParticipants.Add(participant);
        
        // Update participant count
        challenge.ParticipantCount++;
        
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> LeaveChallengeAsync(int challengeId, string userId)
    {
        var participant = await _context.ChallengeParticipants
            .FirstOrDefaultAsync(p => p.ChallengeId == challengeId && p.UserId == userId);

        if (participant == null)
        {
            throw new ArgumentException("You are not participating in this challenge");
        }

        participant.Status = ParticipationStatus.Withdrawn;
        participant.LeftAt = DateTime.UtcNow;

        // Update participant count
        var challenge = await _context.CommunityChallenges.FindAsync(challengeId);
        if (challenge != null)
        {
            challenge.ParticipantCount--;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<PagedResult<ChallengeParticipantDto>> GetChallengeParticipantsAsync(int challengeId, int page = 1, int pageSize = 20)
    {
        var query = _context.ChallengeParticipants
            .Include(p => p.User)
            .Where(p => p.ChallengeId == challengeId);

        var totalCount = await query.CountAsync();

        var participants = await query
            .OrderByDescending(p => p.JoinedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ChallengeParticipantDto>>(participants);

        return new PagedResult<ChallengeParticipantDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ChallengeSubmissionDto> SubmitToChallengeAsync(ChallengeSubmissionCreateDto createDto, string userId)
    {
        var challenge = await _context.CommunityChallenges.FindAsync(createDto.ChallengeId);
        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found");
        }

        // Check if user is participating
        var participant = await _context.ChallengeParticipants
            .FirstOrDefaultAsync(p => p.ChallengeId == createDto.ChallengeId && p.UserId == userId);

        if (participant == null)
        {
            throw new InvalidOperationException("You must join the challenge before submitting");
        }

        // Check video length constraints
        if (challenge.MinVideoLength.HasValue && createDto.Duration < challenge.MinVideoLength.Value)
        {
            throw new InvalidOperationException($"Video must be at least {challenge.MinVideoLength} seconds long");
        }

        if (challenge.MaxVideoLength.HasValue && createDto.Duration > challenge.MaxVideoLength.Value)
        {
            throw new InvalidOperationException($"Video must be no longer than {challenge.MaxVideoLength} seconds");
        }

        var submission = new ChallengeSubmission
        {
            ChallengeId = createDto.ChallengeId,
            UserId = userId,
            VideoId = createDto.VideoId,
            Title = createDto.Title,
            Description = createDto.Description,
            VideoUrl = createDto.VideoUrl,
            ThumbnailUrl = createDto.ThumbnailUrl,
            Duration = createDto.Duration,
            FileSize = createDto.FileSize,
            VideoQuality = createDto.VideoQuality,
            Status = challenge.RequireApproval ? SubmissionStatus.Pending : SubmissionStatus.Approved,
            SubmittedAt = DateTime.UtcNow
        };

        if (!challenge.RequireApproval)
        {
            submission.ApprovedAt = DateTime.UtcNow;
        }

        _context.ChallengeSubmissions.Add(submission);
        
        // Update submission count
        participant.SubmissionCount++;
        
        await _context.SaveChangesAsync();

        return _mapper.Map<ChallengeSubmissionDto>(submission);
    }

    public async Task<PagedResult<ChallengeSubmissionDto>> GetChallengeSubmissionsAsync(int challengeId, int page = 1, int pageSize = 20, string? userId = null)
    {
        var query = _context.ChallengeSubmissions
            .Include(s => s.User)
            .Include(s => s.Votes)
            .Where(s => s.ChallengeId == challengeId && s.Status == SubmissionStatus.Approved);

        var totalCount = await query.CountAsync();

        var submissions = await query
            .OrderByDescending(s => s.VoteCount)
            .ThenByDescending(s => s.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ChallengeSubmissionDto>>(submissions);

        // Check if user has voted on each submission
        if (!string.IsNullOrEmpty(userId))
        {
            foreach (var dto in dtos)
            {
                dto.HasVoted = await _context.ChallengeVotes
                    .AnyAsync(v => v.ChallengeId == challengeId && v.SubmissionId == dto.Id && v.UserId == userId);
            }
        }

        return new PagedResult<ChallengeSubmissionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ChallengeSubmissionDto> GetSubmissionByIdAsync(int id, string? userId = null)
    {
        var submission = await _context.ChallengeSubmissions
            .Include(s => s.User)
            .Include(s => s.Votes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (submission == null)
        {
            throw new ArgumentException("Submission not found");
        }

        var dto = _mapper.Map<ChallengeSubmissionDto>(submission);

        if (!string.IsNullOrEmpty(userId))
        {
            dto.HasVoted = await _context.ChallengeVotes
                .AnyAsync(v => v.ChallengeId == submission.ChallengeId && v.SubmissionId == id && v.UserId == userId);
        }

        return dto;
    }

    public async Task<bool> ApproveSubmissionAsync(int submissionId, string userId)
    {
        var submission = await _context.ChallengeSubmissions
            .Include(s => s.Challenge)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            throw new ArgumentException("Submission not found");
        }

        if (submission.Challenge.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the challenge creator can approve submissions");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("Submission is not pending approval");
        }

        submission.Status = SubmissionStatus.Approved;
        submission.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectSubmissionAsync(int submissionId, string userId, string reason)
    {
        var submission = await _context.ChallengeSubmissions
            .Include(s => s.Challenge)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            throw new ArgumentException("Submission not found");
        }

        if (submission.Challenge.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the challenge creator can reject submissions");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("Submission is not pending approval");
        }

        submission.Status = SubmissionStatus.Rejected;
        submission.RejectedAt = DateTime.UtcNow;
        submission.RejectionReason = reason;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSubmissionAsync(int submissionId, string userId)
    {
        var submission = await _context.ChallengeSubmissions
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.UserId == userId);

        if (submission == null)
        {
            throw new ArgumentException("Submission not found or you don't have permission to delete it");
        }

        _context.ChallengeSubmissions.Remove(submission);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VoteOnSubmissionAsync(ChallengeVoteCreateDto createDto, string userId)
    {
        var challenge = await _context.CommunityChallenges.FindAsync(createDto.ChallengeId);
        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found");
        }

        if (!challenge.AllowVoting)
        {
            throw new InvalidOperationException("Voting is not allowed for this challenge");
        }

        var submission = await _context.ChallengeSubmissions.FindAsync(createDto.SubmissionId);
        if (submission == null)
        {
            throw new ArgumentException("Submission not found");
        }

        // Check if user already voted
        var existingVote = await _context.ChallengeVotes
            .FirstOrDefaultAsync(v => v.ChallengeId == createDto.ChallengeId && 
                                    v.SubmissionId == createDto.SubmissionId && 
                                    v.UserId == userId);

        if (existingVote != null)
        {
            // Update existing vote
            existingVote.Type = createDto.Type;
            existingVote.Comment = createDto.Comment;
            existingVote.VotedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new vote
            var vote = new ChallengeVote
            {
                ChallengeId = createDto.ChallengeId,
                SubmissionId = createDto.SubmissionId,
                UserId = userId,
                Type = createDto.Type,
                Comment = createDto.Comment,
                VotedAt = DateTime.UtcNow
            };

            _context.ChallengeVotes.Add(vote);
        }

        // Update vote count
        submission.VoteCount = await _context.ChallengeVotes
            .CountAsync(v => v.SubmissionId == createDto.SubmissionId && v.Type == VoteType.Upvote);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveVoteAsync(int challengeId, int submissionId, string userId)
    {
        var vote = await _context.ChallengeVotes
            .FirstOrDefaultAsync(v => v.ChallengeId == challengeId && 
                                    v.SubmissionId == submissionId && 
                                    v.UserId == userId);

        if (vote == null)
        {
            throw new ArgumentException("Vote not found");
        }

        _context.ChallengeVotes.Remove(vote);

        // Update vote count
        var submission = await _context.ChallengeSubmissions.FindAsync(submissionId);
        if (submission != null)
        {
            submission.VoteCount = await _context.ChallengeVotes
                .CountAsync(v => v.SubmissionId == submissionId && v.Type == VoteType.Upvote);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ChallengeSubmissionDto>> GetTopSubmissionsAsync(int challengeId, int count = 10)
    {
        var submissions = await _context.ChallengeSubmissions
            .Include(s => s.User)
            .Where(s => s.ChallengeId == challengeId && s.Status == SubmissionStatus.Approved)
            .OrderByDescending(s => s.VoteCount)
            .ThenByDescending(s => s.SubmittedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ChallengeSubmissionDto>>(submissions);
    }

    public async Task<ChallengeCommentDto> AddCommentAsync(ChallengeCommentCreateDto createDto, string userId)
    {
        var challenge = await _context.CommunityChallenges.FindAsync(createDto.ChallengeId);
        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found");
        }

        if (!challenge.AllowComments)
        {
            throw new InvalidOperationException("Comments are not allowed for this challenge");
        }

        var comment = new ChallengeComment
        {
            ChallengeId = createDto.ChallengeId,
            SubmissionId = createDto.SubmissionId,
            ParentCommentId = createDto.ParentCommentId,
            UserId = userId,
            Content = createDto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ChallengeComments.Add(comment);
        await _context.SaveChangesAsync();

        return _mapper.Map<ChallengeCommentDto>(comment);
    }

    public async Task<PagedResult<ChallengeCommentDto>> GetCommentsAsync(int challengeId, int? submissionId = null, int page = 1, int pageSize = 20)
    {
        var query = _context.ChallengeComments
            .Include(c => c.User)
            .Include(c => c.Replies)
            .Include(c => c.Likes)
            .Where(c => c.ChallengeId == challengeId && !c.IsDeleted);

        if (submissionId.HasValue)
        {
            query = query.Where(c => c.SubmissionId == submissionId.Value);
        }
        else
        {
            query = query.Where(c => c.SubmissionId == null);
        }

        var totalCount = await query.CountAsync();

        var comments = await query
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = _mapper.Map<List<ChallengeCommentDto>>(comments);

        return new PagedResult<ChallengeCommentDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<bool> LikeCommentAsync(int commentId, string userId)
    {
        var existingLike = await _context.ChallengeCommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            return true; // Already liked
        }

        var like = new ChallengeCommentLike
        {
            CommentId = commentId,
            UserId = userId,
            LikedAt = DateTime.UtcNow
        };

        _context.ChallengeCommentLikes.Add(like);

        // Update like count
        var comment = await _context.ChallengeComments.FindAsync(commentId);
        if (comment != null)
        {
            comment.LikeCount = await _context.ChallengeCommentLikes
                .CountAsync(l => l.CommentId == commentId);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnlikeCommentAsync(int commentId, string userId)
    {
        var like = await _context.ChallengeCommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (like == null)
        {
            return true; // Not liked
        }

        _context.ChallengeCommentLikes.Remove(like);

        // Update like count
        var comment = await _context.ChallengeComments.FindAsync(commentId);
        if (comment != null)
        {
            comment.LikeCount = await _context.ChallengeCommentLikes
                .CountAsync(l => l.CommentId == commentId);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCommentAsync(int commentId, string userId)
    {
        var comment = await _context.ChallengeComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId);

        if (comment == null)
        {
            throw new ArgumentException("Comment not found or you don't have permission to delete it");
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> PinCommentAsync(int commentId, string userId)
    {
        var comment = await _context.ChallengeComments
            .Include(c => c.Challenge)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new ArgumentException("Comment not found");
        }

        if (comment.Challenge.CreatorId != userId)
        {
            throw new UnauthorizedAccessException("Only the challenge creator can pin comments");
        }

        comment.IsPinned = !comment.IsPinned;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ChallengeAnalyticsDto> GetChallengeAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.CommunityChallenges
            .Where(c => c.CreatorId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value);
        }

        var challenges = await query.ToListAsync();

        var analytics = new ChallengeAnalyticsDto
        {
            TotalChallenges = challenges.Count,
            ActiveChallenges = challenges.Count(c => c.Status == ChallengeStatus.Active),
            CompletedChallenges = challenges.Count(c => c.Status == ChallengeStatus.Completed),
            TotalParticipants = challenges.Sum(c => c.ParticipantCount),
            TotalSubmissions = challenges.Sum(c => c.Submissions.Count),
            TotalVotes = challenges.Sum(c => c.Votes.Count),
            TotalPrizeAmount = challenges.Where(c => c.PrizeAmount.HasValue).Sum(c => c.PrizeAmount ?? 0)
        };

        // Type statistics
        analytics.TypeStats = challenges
            .GroupBy(c => c.Type)
            .Select(g => new ChallengeTypeStats
            {
                Type = g.Key,
                Count = g.Count(),
                Participants = g.Sum(c => c.ParticipantCount),
                Submissions = g.Sum(c => c.Submissions.Count)
            })
            .ToList();

        // Monthly statistics
        analytics.MonthlyStats = challenges
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new MonthlyChallengeStats
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Challenges = g.Count(),
                Participants = g.Sum(c => c.ParticipantCount),
                Submissions = g.Sum(c => c.Submissions.Count)
            })
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToList();

        // Top challenges
        analytics.TopChallenges = challenges
            .OrderByDescending(c => c.ParticipantCount)
            .Take(5)
            .Select(c => new TopChallengeDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatorName = c.Creator.UserName,
                ParticipantCount = c.ParticipantCount,
                SubmissionCount = c.Submissions.Count,
                ViewCount = c.ViewCount,
                PrizeAmount = c.PrizeAmount
            })
            .ToList();

        return analytics;
    }

    public async Task<ChallengeAnalyticsDto> GetChallengeAnalyticsByIdAsync(int challengeId, string userId)
    {
        var challenge = await _context.CommunityChallenges
            .Include(c => c.Creator)
            .Include(c => c.Participants)
            .Include(c => c.Submissions)
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == challengeId && c.CreatorId == userId);

        if (challenge == null)
        {
            throw new ArgumentException("Challenge not found or you don't have permission to view analytics");
        }

        var analytics = new ChallengeAnalyticsDto
        {
            TotalChallenges = 1,
            ActiveChallenges = challenge.Status == ChallengeStatus.Active ? 1 : 0,
            CompletedChallenges = challenge.Status == ChallengeStatus.Completed ? 1 : 0,
            TotalParticipants = challenge.ParticipantCount,
            TotalSubmissions = challenge.Submissions.Count,
            TotalVotes = challenge.Votes.Count,
            TotalPrizeAmount = challenge.PrizeAmount ?? 0
        };

        return analytics;
    }

    public async Task<List<ChallengeDto>> GetTrendingChallengesAsync(int count = 10)
    {
        var challenges = await _context.CommunityChallenges
            .Include(c => c.Creator)
            .Where(c => c.Status == ChallengeStatus.Active || c.Status == ChallengeStatus.Published)
            .OrderByDescending(c => c.ParticipantCount)
            .ThenByDescending(c => c.ViewCount)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ChallengeDto>>(challenges);
    }

    public async Task<List<ChallengeDto>> GetRecommendedChallengesAsync(string userId, int count = 10)
    {
        // Simple recommendation based on user's previous participation
        var userParticipations = await _context.ChallengeParticipants
            .Include(p => p.Challenge)
            .Where(p => p.UserId == userId)
            .Select(p => p.Challenge.Type)
            .ToListAsync();

        var recommendedChallenges = await _context.CommunityChallenges
            .Include(c => c.Creator)
            .Where(c => (c.Status == ChallengeStatus.Active || c.Status == ChallengeStatus.Published) &&
                       userParticipations.Contains(c.Type))
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ChallengeDto>>(recommendedChallenges);
    }

    public async Task<List<ChallengeDto>> GetChallengesByHashtagAsync(string hashtag, int page = 1, int pageSize = 20)
    {
        var challenges = await _context.CommunityChallenges
            .Include(c => c.Creator)
            .Where(c => c.Hashtag.Contains(hashtag) && 
                       (c.Status == ChallengeStatus.Active || c.Status == ChallengeStatus.Published))
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<ChallengeDto>>(challenges);
    }
}
