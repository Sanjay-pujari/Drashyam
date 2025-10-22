using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(DrashyamDbContext context, IMapper mapper, ILogger<SubscriptionService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(SubscriptionCreateDto createDto, string userId)
    {
        var subscription = _mapper.Map<Subscription>(createDto);
        subscription.UserId = userId;
        subscription.StartDate = DateTime.UtcNow;
        subscription.Status = SubscriptionStatus.Active;

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        return _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<SubscriptionDto> GetSubscriptionByIdAsync(int subscriptionId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId);

        if (subscription == null)
            throw new ArgumentException("Subscription not found");

        return _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<SubscriptionDto> GetUserSubscriptionAsync(string userId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (subscription == null)
            throw new ArgumentException("No active subscription found");

        return _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<SubscriptionDto> UpdateSubscriptionAsync(int subscriptionId, SubscriptionUpdateDto updateDto, string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            throw new ArgumentException("Subscription not found or access denied");

        _mapper.Map(updateDto, subscription);
        await _context.SaveChangesAsync();

        return _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<bool> CancelSubscriptionAsync(int subscriptionId, string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RenewSubscriptionAsync(int subscriptionId, string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(int page = 1, int pageSize = 20)
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .CountAsync();

        return new PagedResult<SubscriptionPlanDto>
        {
            Items = _mapper.Map<List<SubscriptionPlanDto>>(plans),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SubscriptionPlanDto> GetSubscriptionPlanByIdAsync(int planId)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null)
            throw new ArgumentException("Subscription plan not found");

        return _mapper.Map<SubscriptionPlanDto>(plan);
    }

    public async Task<SubscriptionDto> UpgradeSubscriptionAsync(string userId, int newPlanId)
    {
        var currentSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (currentSubscription == null)
            throw new ArgumentException("No active subscription found");

        currentSubscription.SubscriptionPlanId = newPlanId;
        await _context.SaveChangesAsync();

        return _mapper.Map<SubscriptionDto>(currentSubscription);
    }

    public async Task<SubscriptionDto> DowngradeSubscriptionAsync(string userId, int newPlanId)
    {
        var currentSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (currentSubscription == null)
            throw new ArgumentException("No active subscription found");

        currentSubscription.SubscriptionPlanId = newPlanId;
        await _context.SaveChangesAsync();

        return _mapper.Map<SubscriptionDto>(currentSubscription);
    }

    public async Task<bool> CheckSubscriptionStatusAsync(string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        return subscription != null;
    }

    public async Task<SubscriptionDto> ProcessPaymentAsync(string userId, PaymentDto paymentDto)
    {
        // Process payment logic here
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (subscription == null)
            throw new ArgumentException("No active subscription found");

        return _mapper.Map<SubscriptionDto>(subscription);
    }

    public async Task<PagedResult<SubscriptionDto>> GetExpiringSubscriptionsAsync(int daysAhead = 7, int page = 1, int pageSize = 20)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysAhead);
        var subscriptions = await _context.Subscriptions
            .Where(s => s.EndDate <= expiryDate && s.Status == SubscriptionStatus.Active)
            .Include(s => s.Plan)
            .Include(s => s.User)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Subscriptions
            .Where(s => s.EndDate <= expiryDate && s.Status == SubscriptionStatus.Active)
            .CountAsync();

        return new PagedResult<SubscriptionDto>
        {
            Items = _mapper.Map<List<SubscriptionDto>>(subscriptions),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<SubscriptionHistoryDto>> GetUserSubscriptionHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        var subscriptions = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .Include(s => s.Plan)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .CountAsync();

        var historyItems = subscriptions.Select(s => new SubscriptionHistoryDto
        {
            Id = s.Id,
            UserId = s.UserId,
            SubscriptionPlanId = s.SubscriptionPlanId,
            PlanName = s.Plan.Name,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            Amount = s.Amount,
            CreatedAt = s.CreatedAt,
            CancelledAt = s.CancelledAt
        }).ToList();

        return new PagedResult<SubscriptionHistoryDto>
        {
            Items = historyItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SubscriptionAnalyticsDto> GetSubscriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Subscriptions.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.CreatedAt <= endDate.Value);

        var subscriptions = await query
            .Include(s => s.Plan)
            .ToListAsync();

        var totalSubscriptions = subscriptions.Count;
        var activeSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
        var expiredSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Expired);
        var cancelledSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Cancelled);

        var totalRevenue = subscriptions.Sum(s => s.Amount);
        var monthlyRecurringRevenue = subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Sum(s => s.Amount);

        var averageRevenuePerUser = activeSubscriptions > 0 ? monthlyRecurringRevenue / activeSubscriptions : 0;

        var planAnalytics = subscriptions
            .GroupBy(s => new { s.Plan.Id, s.Plan.Name })
            .Select(g => new SubscriptionPlanAnalyticsDto
            {
                PlanId = g.Key.Id,
                PlanName = g.Key.Name,
                SubscriberCount = g.Count(),
                Revenue = g.Sum(s => s.Amount),
                AverageRevenuePerUser = g.Count() > 0 ? g.Sum(s => s.Amount) / g.Count() : 0
            })
            .ToList();

        return new SubscriptionAnalyticsDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions,
            ExpiredSubscriptions = expiredSubscriptions,
            CancelledSubscriptions = cancelledSubscriptions,
            TotalRevenue = totalRevenue,
            MonthlyRecurringRevenue = monthlyRecurringRevenue,
            AverageRevenuePerUser = averageRevenuePerUser,
            PlanAnalytics = planAnalytics
        };
    }

    public async Task<bool> SuspendSubscriptionAsync(int subscriptionId, string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Suspended;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactivateSubscriptionAsync(int subscriptionId, string userId)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId);

        if (subscription == null)
            return false;

        subscription.Status = SubscriptionStatus.Active;
        subscription.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
