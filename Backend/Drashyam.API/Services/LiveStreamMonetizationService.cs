using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class LiveStreamMonetizationService : ILiveStreamMonetizationService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<LiveStreamMonetizationService> _logger;

    public LiveStreamMonetizationService(DrashyamDbContext context, IMapper mapper, ILogger<LiveStreamMonetizationService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDonationDto> SendDonationAsync(SendDonationDto dto, string userId)
    {
        var donation = new LiveStreamDonation
        {
            LiveStreamId = dto.LiveStreamId,
            DonorId = userId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Message = dto.Message,
            IsAnonymous = dto.IsAnonymous,
            DonatedAt = DateTime.UtcNow
        };

        _context.LiveStreamDonations.Add(donation);
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamDonations
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(d => d.Id == donation.Id);

        return _mapper.Map<LiveStreamDonationDto>(result);
    }

    public async Task<List<LiveStreamDonationDto>> GetDonationsAsync(int liveStreamId, int page = 1, int pageSize = 50)
    {
        var donations = await _context.LiveStreamDonations
            .Include(d => d.Donor)
            .Where(d => d.LiveStreamId == liveStreamId)
            .OrderByDescending(d => d.DonatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamDonationDto>>(donations);
    }

    public async Task<LiveStreamSuperChatDto> SendSuperChatAsync(SendSuperChatDto dto, string userId)
    {
        var superChat = new LiveStreamSuperChat
        {
            LiveStreamId = dto.LiveStreamId,
            UserId = userId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Message = dto.Message,
            Tier = dto.Tier,
            DurationSeconds = GetSuperChatDuration(dto.Tier),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddSeconds(GetSuperChatDuration(dto.Tier))
        };

        _context.LiveStreamSuperChats.Add(superChat);
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamSuperChats
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == superChat.Id);

        return _mapper.Map<LiveStreamSuperChatDto>(result);
    }

    public async Task<List<LiveStreamSuperChatDto>> GetSuperChatsAsync(int liveStreamId, int page = 1, int pageSize = 50)
    {
        var superChats = await _context.LiveStreamSuperChats
            .Include(s => s.User)
            .Where(s => s.LiveStreamId == liveStreamId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamSuperChatDto>>(superChats);
    }

    public async Task<LiveStreamSubscriptionDto> SubscribeToLiveStreamAsync(SubscribeToLiveStreamDto dto, string userId)
    {
        // Check if user is already subscribed
        var existingSubscription = await _context.LiveStreamSubscriptions
            .FirstOrDefaultAsync(s => s.LiveStreamId == dto.LiveStreamId && s.SubscriberId == userId && s.IsActive);

        if (existingSubscription != null)
            throw new InvalidOperationException("You are already subscribed to this live stream");

        var subscription = new LiveStreamSubscription
        {
            LiveStreamId = dto.LiveStreamId,
            SubscriberId = userId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Tier = dto.Tier,
            SubscribedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30), // Default 30-day subscription
            IsActive = true
        };

        _context.LiveStreamSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var result = await _context.LiveStreamSubscriptions
            .Include(s => s.Subscriber)
            .FirstOrDefaultAsync(s => s.Id == subscription.Id);

        return _mapper.Map<LiveStreamSubscriptionDto>(result);
    }

    public async Task<List<LiveStreamSubscriptionDto>> GetSubscriptionsAsync(int liveStreamId, int page = 1, int pageSize = 50)
    {
        var subscriptions = await _context.LiveStreamSubscriptions
            .Include(s => s.Subscriber)
            .Where(s => s.LiveStreamId == liveStreamId && s.IsActive)
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamSubscriptionDto>>(subscriptions);
    }

    public async Task<bool> UnsubscribeFromLiveStreamAsync(int liveStreamId, string userId)
    {
        var subscription = await _context.LiveStreamSubscriptions
            .FirstOrDefaultAsync(s => s.LiveStreamId == liveStreamId && s.SubscriberId == userId && s.IsActive);

        if (subscription == null)
            return false;

        subscription.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<LiveStreamRevenueDto> GetRevenueAsync(int liveStreamId)
    {
        var revenue = await _context.LiveStreamRevenues
            .FirstOrDefaultAsync(r => r.LiveStreamId == liveStreamId);

        if (revenue == null)
        {
            // Calculate revenue from donations, super chats, and subscriptions
            var donations = await _context.LiveStreamDonations
                .Where(d => d.LiveStreamId == liveStreamId)
                .SumAsync(d => d.Amount);

            var superChats = await _context.LiveStreamSuperChats
                .Where(s => s.LiveStreamId == liveStreamId)
                .SumAsync(s => s.Amount);

            var subscriptions = await _context.LiveStreamSubscriptions
                .Where(s => s.LiveStreamId == liveStreamId && s.IsActive)
                .SumAsync(s => s.Amount);

            var totalRevenue = donations + superChats + subscriptions;
            var platformFee = totalRevenue * 0.1m; // 10% platform fee
            var creatorEarnings = totalRevenue - platformFee;

            revenue = new LiveStreamRevenue
            {
                LiveStreamId = liveStreamId,
                TotalRevenue = totalRevenue,
                DonationRevenue = donations,
                SuperChatRevenue = superChats,
                SubscriptionRevenue = subscriptions,
                AdRevenue = 0, // TODO: Implement ad revenue
                PlatformFee = platformFee,
                CreatorEarnings = creatorEarnings,
                CalculatedAt = DateTime.UtcNow
            };

            _context.LiveStreamRevenues.Add(revenue);
            await _context.SaveChangesAsync();
        }

        return _mapper.Map<LiveStreamRevenueDto>(revenue);
    }

    public async Task<LiveStreamMonetizationStatsDto> GetMonetizationStatsAsync(int liveStreamId)
    {
        var donations = await _context.LiveStreamDonations
            .Where(d => d.LiveStreamId == liveStreamId)
            .ToListAsync();

        var superChats = await _context.LiveStreamSuperChats
            .Where(s => s.LiveStreamId == liveStreamId)
            .ToListAsync();

        var subscriptions = await _context.LiveStreamSubscriptions
            .Where(s => s.LiveStreamId == liveStreamId && s.IsActive)
            .ToListAsync();

        var stats = new LiveStreamMonetizationStatsDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = donations.Sum(d => d.Amount) + superChats.Sum(s => s.Amount) + subscriptions.Sum(s => s.Amount),
            DonationRevenue = donations.Sum(d => d.Amount),
            SuperChatRevenue = superChats.Sum(s => s.Amount),
            SubscriptionRevenue = subscriptions.Sum(s => s.Amount),
            AdRevenue = 0, // TODO: Implement ad revenue
            TotalDonations = donations.Count,
            TotalSuperChats = superChats.Count,
            TotalSubscriptions = subscriptions.Count,
            AverageDonation = donations.Any() ? donations.Average(d => d.Amount) : 0,
            AverageSuperChat = superChats.Any() ? superChats.Average(s => s.Amount) : 0,
            AverageSubscription = subscriptions.Any() ? subscriptions.Average(s => s.Amount) : 0,
            LastUpdated = DateTime.UtcNow
        };

        return stats;
    }

    public async Task<List<LiveStreamDonationDto>> GetUserDonationsAsync(string userId, int page = 1, int pageSize = 50)
    {
        var donations = await _context.LiveStreamDonations
            .Include(d => d.Donor)
            .Where(d => d.DonorId == userId)
            .OrderByDescending(d => d.DonatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamDonationDto>>(donations);
    }

    public async Task<List<LiveStreamSuperChatDto>> GetUserSuperChatsAsync(string userId, int page = 1, int pageSize = 50)
    {
        var superChats = await _context.LiveStreamSuperChats
            .Include(s => s.User)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamSuperChatDto>>(superChats);
    }

    public async Task<List<LiveStreamSubscriptionDto>> GetUserSubscriptionsAsync(string userId, int page = 1, int pageSize = 50)
    {
        var subscriptions = await _context.LiveStreamSubscriptions
            .Include(s => s.Subscriber)
            .Where(s => s.SubscriberId == userId)
            .OrderByDescending(s => s.SubscribedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<LiveStreamSubscriptionDto>>(subscriptions);
    }

    public async Task<decimal> GetTotalRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.LiveStreamRevenues
            .Where(r => r.LiveStream.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(r => r.CalculatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.CalculatedAt <= endDate.Value);

        return await query.SumAsync(r => r.CreatorEarnings);
    }

    public async Task<decimal> GetLiveStreamRevenueAsync(int liveStreamId)
    {
        var revenue = await _context.LiveStreamRevenues
            .FirstOrDefaultAsync(r => r.LiveStreamId == liveStreamId);

        return revenue?.CreatorEarnings ?? 0;
    }

    public async Task<bool> ProcessPaymentAsync(string userId, decimal amount, string currency, string paymentMethod)
    {
        // TODO: Implement actual payment processing
        // This is a placeholder for payment processing logic
        _logger.LogInformation($"Processing payment for user {userId}: {amount} {currency} via {paymentMethod}");
        return true;
    }

    public async Task<bool> RefundPaymentAsync(int transactionId, string reason)
    {
        // TODO: Implement actual refund processing
        // This is a placeholder for refund processing logic
        _logger.LogInformation($"Processing refund for transaction {transactionId}: {reason}");
        return true;
    }

    private int GetSuperChatDuration(SuperChatTier tier)
    {
        return tier switch
        {
            SuperChatTier.Bronze => 30,
            SuperChatTier.Silver => 60,
            SuperChatTier.Gold => 120,
            SuperChatTier.Diamond => 300,
            SuperChatTier.Platinum => 600,
            _ => 30
        };
    }
}
