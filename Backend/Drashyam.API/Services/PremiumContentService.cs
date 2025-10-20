using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class PremiumContentService : IPremiumContentService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PremiumContentService> _logger;

    public PremiumContentService(DrashyamDbContext context, IMapper mapper, ILogger<PremiumContentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PremiumVideoDto> CreatePremiumVideoAsync(PremiumVideoCreateDto premiumVideoDto)
    {
        // Check if video exists and belongs to the creator
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == premiumVideoDto.VideoId && v.UserId == premiumVideoDto.CreatorId);

        if (video == null)
            throw new ArgumentException("Video not found or access denied");

        // Check if video is already premium
        var existingPremium = await _context.PremiumVideos
            .FirstOrDefaultAsync(pv => pv.VideoId == premiumVideoDto.VideoId);

        if (existingPremium != null)
            throw new ArgumentException("Video is already premium");

        var premiumVideo = _mapper.Map<PremiumVideo>(premiumVideoDto);
        _context.PremiumVideos.Add(premiumVideo);
        await _context.SaveChangesAsync();

        return _mapper.Map<PremiumVideoDto>(premiumVideo);
    }

    public async Task<PremiumVideoDto> UpdatePremiumVideoAsync(int premiumVideoId, PremiumVideoUpdateDto premiumVideoDto)
    {
        var premiumVideo = await _context.PremiumVideos
            .Include(pv => pv.Video)
            .FirstOrDefaultAsync(pv => pv.Id == premiumVideoId);

        if (premiumVideo == null)
            throw new ArgumentException("Premium video not found");

        // Check if user owns the video
        if (premiumVideo.Video.UserId != premiumVideoDto.CreatorId)
            throw new UnauthorizedAccessException("Access denied");

        _mapper.Map(premiumVideoDto, premiumVideo);
        premiumVideo.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<PremiumVideoDto>(premiumVideo);
    }

    public async Task<bool> DeletePremiumVideoAsync(int premiumVideoId)
    {
        var premiumVideo = await _context.PremiumVideos.FindAsync(premiumVideoId);
        if (premiumVideo == null)
            return false;

        _context.PremiumVideos.Remove(premiumVideo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PremiumVideoDto> GetPremiumVideoAsync(int premiumVideoId)
    {
        var premiumVideo = await _context.PremiumVideos
            .Include(pv => pv.Video)
            .ThenInclude(v => v.User)
            .FirstOrDefaultAsync(pv => pv.Id == premiumVideoId);

        if (premiumVideo == null)
            throw new ArgumentException("Premium video not found");

        return _mapper.Map<PremiumVideoDto>(premiumVideo);
    }

    public async Task<PagedResult<PremiumVideoDto>> GetPremiumVideosAsync(int page = 1, int pageSize = 20)
    {
        var premiumVideos = await _context.PremiumVideos
            .Include(pv => pv.Video)
            .ThenInclude(v => v.User)
            .Where(pv => pv.IsActive)
            .OrderByDescending(pv => pv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.PremiumVideos
            .Where(pv => pv.IsActive)
            .CountAsync();

        return new PagedResult<PremiumVideoDto>
        {
            Items = _mapper.Map<List<PremiumVideoDto>>(premiumVideos),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsVideoPremiumAsync(int videoId)
    {
        return await _context.PremiumVideos
            .AnyAsync(pv => pv.VideoId == videoId && pv.IsActive);
    }

    public async Task<bool> HasUserPurchasedAsync(int premiumVideoId, string userId)
    {
        return await _context.PremiumPurchases
            .AnyAsync(pp => pp.PremiumVideoId == premiumVideoId && 
                           pp.UserId == userId && 
                           pp.Status == PremiumPurchaseStatus.Completed);
    }

    public async Task<PremiumPurchaseDto> CreatePurchaseAsync(PremiumPurchaseCreateDto purchaseDto)
    {
        var premiumVideo = await _context.PremiumVideos
            .Include(pv => pv.Video)
            .FirstOrDefaultAsync(pv => pv.Id == purchaseDto.PremiumVideoId);

        if (premiumVideo == null)
            throw new ArgumentException("Premium video not found");

        // Check if user already purchased
        var existingPurchase = await _context.PremiumPurchases
            .FirstOrDefaultAsync(pp => pp.PremiumVideoId == purchaseDto.PremiumVideoId && 
                                     pp.UserId == purchaseDto.UserId && 
                                     pp.Status == PremiumPurchaseStatus.Completed);

        if (existingPurchase != null)
            throw new ArgumentException("User has already purchased this content");

        var purchase = new PremiumPurchase
        {
            PremiumVideoId = purchaseDto.PremiumVideoId,
            UserId = purchaseDto.UserId,
            Amount = premiumVideo.Price,
            Currency = premiumVideo.Currency,
            PaymentIntentId = purchaseDto.PaymentIntentId,
            Status = PremiumPurchaseStatus.Pending
        };

        _context.PremiumPurchases.Add(purchase);
        await _context.SaveChangesAsync();

        return _mapper.Map<PremiumPurchaseDto>(purchase);
    }

    public async Task<bool> CompletePurchaseAsync(string paymentIntentId)
    {
        var purchase = await _context.PremiumPurchases
            .FirstOrDefaultAsync(pp => pp.PaymentIntentId == paymentIntentId);

        if (purchase == null)
            return false;

        purchase.Status = PremiumPurchaseStatus.Completed;
        purchase.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefundPurchaseAsync(int purchaseId)
    {
        var purchase = await _context.PremiumPurchases.FindAsync(purchaseId);
        if (purchase == null)
            return false;

        purchase.Status = PremiumPurchaseStatus.Refunded;
        purchase.RefundedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<PremiumPurchaseDto>> GetUserPurchasesAsync(string userId, int page = 1, int pageSize = 20)
    {
        var purchases = await _context.PremiumPurchases
            .Include(pp => pp.PremiumVideo)
            .ThenInclude(pv => pv.Video)
            .Where(pp => pp.UserId == userId)
            .OrderByDescending(pp => pp.PurchasedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalCount = await _context.PremiumPurchases
            .Where(pp => pp.UserId == userId)
            .CountAsync();

        return new PagedResult<PremiumPurchaseDto>
        {
            Items = _mapper.Map<List<PremiumPurchaseDto>>(purchases),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PremiumContentAnalyticsDto> GetPremiumContentAnalyticsAsync(int premiumVideoId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.PremiumPurchases
            .Where(pp => pp.PremiumVideoId == premiumVideoId && pp.Status == PremiumPurchaseStatus.Completed);

        if (startDate.HasValue)
            query = query.Where(pp => pp.CompletedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(pp => pp.CompletedAt <= endDate.Value);

        var purchases = await query.ToListAsync();

        return new PremiumContentAnalyticsDto
        {
            PremiumVideoId = premiumVideoId,
            TotalPurchases = purchases.Count,
            TotalRevenue = purchases.Sum(p => p.Amount),
            AveragePrice = purchases.Count > 0 ? purchases.Average(p => p.Amount) : 0,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<decimal> CalculatePremiumRevenueAsync(string creatorId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.PremiumPurchases
            .Where(pp => pp.PremiumVideo.Video.UserId == creatorId && pp.Status == PremiumPurchaseStatus.Completed);

        if (startDate.HasValue)
            query = query.Where(pp => pp.CompletedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(pp => pp.CompletedAt <= endDate.Value);

        return await query.SumAsync(pp => pp.Amount);
    }
}
