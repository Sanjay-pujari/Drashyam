using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class SuperChatService : ISuperChatService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<SuperChatService> _logger;

    public SuperChatService(
        DrashyamDbContext context,
        IMapper mapper,
        IPaymentService paymentService,
        ILogger<SuperChatService> logger)
    {
        _context = context;
        _mapper = mapper;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<SuperChatDto> ProcessSuperChatAsync(SuperChatRequestDto request, string userId)
    {
        try
        {
            // Verify live stream exists and is active
            var liveStream = await _context.LiveStreams
                .FirstOrDefaultAsync(ls => ls.Id == request.LiveStreamId && ls.Status == DTOs.LiveStreamStatus.Live);

            if (liveStream == null)
            {
                throw new ArgumentException("Live stream not found or not active");
            }

            // Process payment
            var paymentDto = new PaymentDto
            {
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethodId = request.PaymentMethodId,
                Description = $"Super Chat for {liveStream.Title}"
            };
            var paymentResult = await _paymentService.ProcessPaymentAsync(paymentDto);

            if (!paymentResult.Success)
            {
                throw new InvalidOperationException($"Payment failed: {paymentResult.ErrorMessage}");
            }

            // Create super chat record
            var superChat = new SuperChat
            {
                LiveStreamId = request.LiveStreamId,
                DonorName = request.IsAnonymous ? "Anonymous" : request.DonorName,
                DonorMessage = request.DonorMessage,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = "Completed",
                ProcessedAt = DateTime.UtcNow,
                PaymentIntentId = paymentResult.PaymentIntentId,
                DisplayDuration = GetDisplayDuration(request.Amount),
                IsAnonymous = request.IsAnonymous
            };

            _context.SuperChats.Add(superChat);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Super chat processed successfully: {SuperChatId} for {Amount} {Currency}", 
                superChat.Id, superChat.Amount, superChat.Currency);

            return _mapper.Map<SuperChatDto>(superChat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing super chat for live stream {LiveStreamId}", request.LiveStreamId);
            throw;
        }
    }

    public async Task<List<SuperChatDisplayDto>> GetLiveStreamSuperChatsAsync(int liveStreamId)
    {
        var superChats = await _context.SuperChats
            .Where(sc => sc.LiveStreamId == liveStreamId && sc.Status == "Completed")
            .OrderByDescending(sc => sc.CreatedAt)
            .Take(50) // Limit to recent 50 super chats
            .Select(sc => new SuperChatDisplayDto
            {
                Id = sc.Id,
                DonorName = sc.DonorName,
                DonorMessage = sc.DonorMessage,
                Amount = sc.Amount,
                Currency = sc.Currency,
                CreatedAt = sc.CreatedAt,
                DonorAvatar = sc.DonorAvatar,
                IsAnonymous = sc.IsAnonymous,
                DisplayDuration = sc.DisplayDuration
            })
            .ToListAsync();

        return superChats;
    }

    public async Task<SuperChatDto> GetSuperChatByIdAsync(int superChatId)
    {
        var superChat = await _context.SuperChats
            .FirstOrDefaultAsync(sc => sc.Id == superChatId);

        if (superChat == null)
        {
            throw new ArgumentException("Super chat not found");
        }

        return _mapper.Map<SuperChatDto>(superChat);
    }

    public async Task<bool> UpdateSuperChatStatusAsync(int superChatId, string status)
    {
        var superChat = await _context.SuperChats
            .FirstOrDefaultAsync(sc => sc.Id == superChatId);

        if (superChat == null)
        {
            return false;
        }

        superChat.Status = status;
        if (status == "Completed")
        {
            superChat.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SuperChatDto>> GetUserSuperChatsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var superChats = await _context.SuperChats
            .Include(sc => sc.LiveStream)
            .Where(sc => sc.LiveStream.UserId == userId)
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<SuperChatDto>>(superChats);
    }

    public async Task<List<SuperChatDto>> GetCreatorSuperChatsAsync(string creatorId, int page = 1, int pageSize = 20)
    {
        var superChats = await _context.SuperChats
            .Include(sc => sc.LiveStream)
            .Where(sc => sc.LiveStream.UserId == creatorId)
            .OrderByDescending(sc => sc.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<List<SuperChatDto>>(superChats);
    }

    public async Task<decimal> GetTotalSuperChatRevenueAsync(string creatorId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.SuperChats
            .Include(sc => sc.LiveStream)
            .Where(sc => sc.LiveStream.UserId == creatorId && sc.Status == "Completed");

        if (startDate.HasValue)
        {
            query = query.Where(sc => sc.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(sc => sc.CreatedAt <= endDate.Value);
        }

        return await query.SumAsync(sc => sc.Amount);
    }

    private int GetDisplayDuration(decimal amount)
    {
        // Display duration based on amount
        if (amount >= 100) return 10; // 10 seconds for $100+
        if (amount >= 50) return 8;  // 8 seconds for $50+
        if (amount >= 25) return 6;  // 6 seconds for $25+
        if (amount >= 10) return 4;  // 4 seconds for $10+
        return 2; // 2 seconds for under $10
    }
}
