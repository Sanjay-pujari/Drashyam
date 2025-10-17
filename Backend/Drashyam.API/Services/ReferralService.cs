using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class ReferralService : IReferralService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ReferralService> _logger;

    public ReferralService(
        DrashyamDbContext context,
        IMapper mapper,
        ILogger<ReferralService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReferralDto> CreateReferralAsync(string referrerId, CreateReferralDto createDto)
    {
        // Check if referral already exists
        var existingReferral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferrerId == referrerId && r.ReferredUserId == createDto.ReferredUserId);

        if (existingReferral != null)
            throw new InvalidOperationException("Referral already exists for this user");

        var referral = new Referral
        {
            ReferrerId = referrerId,
            ReferredUserId = createDto.ReferredUserId,
            ReferralCode = createDto.ReferralCode ?? GenerateReferralCode(),
            Status = ReferralStatus.Pending
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        return await GetReferralDtoAsync(referral.Id);
    }

    public async Task<ReferralDto> GetReferralByIdAsync(int referralId)
    {
        var referral = await _context.Referrals
            .Include(r => r.Referrer)
            .Include(r => r.ReferredUser)
            .FirstOrDefaultAsync(r => r.Id == referralId);

        if (referral == null)
            throw new ArgumentException("Referral not found");

        return _mapper.Map<ReferralDto>(referral);
    }

    public async Task<PagedResult<ReferralDto>> GetUserReferralsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var referrals = await _context.Referrals
            .Include(r => r.ReferredUser)
            .Where(r => r.ReferrerId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Referrals
            .Where(r => r.ReferrerId == userId)
            .CountAsync();

        return new PagedResult<ReferralDto>
        {
            Items = _mapper.Map<List<ReferralDto>>(referrals),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ReferralDto>> GetReferralsByUserAsync(string userId, int page = 1, int pageSize = 20)
    {
        var referrals = await _context.Referrals
            .Include(r => r.Referrer)
            .Where(r => r.ReferredUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Referrals
            .Where(r => r.ReferredUserId == userId)
            .CountAsync();

        return new PagedResult<ReferralDto>
        {
            Items = _mapper.Map<List<ReferralDto>>(referrals),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReferralStatsDto> GetReferralStatsAsync(string userId)
    {
        var referrals = await _context.Referrals
            .Where(r => r.ReferrerId == userId)
            .ToListAsync();

        var totalReferrals = referrals.Count;
        var completedReferrals = referrals.Count(r => r.Status == ReferralStatus.Completed);
        var pendingReferrals = referrals.Count(r => r.Status == ReferralStatus.Pending);

        var rewards = await _context.ReferralRewards
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var totalRewards = rewards.Sum(r => r.Amount);
        var pendingRewards = rewards.Where(r => r.Status == RewardStatus.Pending).Sum(r => r.Amount);

        var conversionRate = totalReferrals > 0 ? (decimal)completedReferrals / totalReferrals * 100 : 0;

        return new ReferralStatsDto
        {
            TotalReferrals = totalReferrals,
            CompletedReferrals = completedReferrals,
            PendingReferrals = pendingReferrals,
            TotalRewards = totalRewards,
            PendingRewards = pendingRewards,
            ConversionRate = conversionRate
        };
    }

    public async Task<ReferralCodeDto> CreateReferralCodeAsync(string userId, CreateReferralCodeDto createDto)
    {
        var code = createDto.Code ?? GenerateReferralCode();
        
        // Check if code already exists
        var existingCode = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == code);

        if (existingCode != null)
            throw new InvalidOperationException("Referral code already exists");

        var referral = new Referral
        {
            ReferrerId = userId,
            ReferredUserId = userId, // Self-referral for code generation
            ReferralCode = code,
            Status = ReferralStatus.Pending,
            RewardAmount = createDto.RewardAmount,
            RewardType = createDto.RewardType
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();

        return new ReferralCodeDto
        {
            Code = code,
            CreatedAt = referral.CreatedAt,
            ExpiresAt = createDto.ExpiresAt,
            UsageCount = 0,
            MaxUsage = createDto.MaxUsage ?? int.MaxValue,
            IsActive = true
        };
    }

    public async Task<ReferralCodeDto> GetReferralCodeAsync(string userId)
    {
        var referral = await _context.Referrals
            .Where(r => r.ReferrerId == userId && r.ReferralCode != null)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (referral == null)
            return new ReferralCodeDto { Code = string.Empty };

        var usageCount = await _context.Referrals
            .CountAsync(r => r.ReferralCode == referral.ReferralCode && r.Status == ReferralStatus.Completed);

        return new ReferralCodeDto
        {
            Code = referral.ReferralCode!,
            CreatedAt = referral.CreatedAt,
            ExpiresAt = null,
            UsageCount = usageCount,
            MaxUsage = int.MaxValue,
            IsActive = true
        };
    }

    public async Task<bool> ValidateReferralCodeAsync(string code)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == code);

        return referral != null && referral.Status == ReferralStatus.Pending;
    }

    public async Task<ReferralRewardDto> ClaimRewardAsync(string userId, ClaimRewardDto claimDto)
    {
        var reward = await _context.ReferralRewards
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == claimDto.RewardId && r.UserId == userId);

        if (reward == null)
            throw new ArgumentException("Reward not found");

        if (reward.Status != RewardStatus.Pending)
            throw new InvalidOperationException("Reward has already been claimed or is not available");

        if (reward.ExpiresAt.HasValue && reward.ExpiresAt.Value < DateTime.UtcNow)
            throw new InvalidOperationException("Reward has expired");

        reward.Status = RewardStatus.Claimed;
        reward.ClaimedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return _mapper.Map<ReferralRewardDto>(reward);
    }

    public async Task<PagedResult<ReferralRewardDto>> GetUserRewardsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var rewards = await _context.ReferralRewards
            .Include(r => r.Referral)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.ReferralRewards
            .Where(r => r.UserId == userId)
            .CountAsync();

        return new PagedResult<ReferralRewardDto>
        {
            Items = _mapper.Map<List<ReferralRewardDto>>(rewards),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ProcessReferralRewardAsync(int referralId)
    {
        var referral = await _context.Referrals
            .Include(r => r.Referrer)
            .FirstOrDefaultAsync(r => r.Id == referralId);

        if (referral == null)
            return false;

        if (referral.Status != ReferralStatus.Completed)
            return false;

        // Create reward
        var reward = new ReferralReward
        {
            UserId = referral.ReferrerId,
            ReferralId = referralId,
            Amount = referral.RewardAmount ?? 10.00m,
            RewardType = referral.RewardType ?? "Points",
            ExpiresAt = DateTime.UtcNow.AddDays(90)
        };

        _context.ReferralRewards.Add(reward);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateReferralStatusAsync(int referralId, ReferralStatus status)
    {
        var referral = await _context.Referrals.FindAsync(referralId);
        if (referral == null)
            return false;

        referral.Status = status;
        if (status == ReferralStatus.Completed)
        {
            referral.RewardedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Process reward if completed
        if (status == ReferralStatus.Completed)
        {
            await ProcessReferralRewardAsync(referralId);
        }

        return true;
    }

    private async Task<ReferralDto> GetReferralDtoAsync(int referralId)
    {
        var referral = await _context.Referrals
            .Include(r => r.Referrer)
            .Include(r => r.ReferredUser)
            .FirstOrDefaultAsync(r => r.Id == referralId);

        return _mapper.Map<ReferralDto>(referral!);
    }

    private string GenerateReferralCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
