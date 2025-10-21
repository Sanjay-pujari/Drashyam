using Drashyam.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class RecommendationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecommendationBackgroundService> _logger;
    private readonly TimeSpan _period = TimeSpan.FromHours(1); // Run every hour

    public RecommendationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecommendationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
                var context = scope.ServiceProvider.GetRequiredService<DrashyamDbContext>();

                _logger.LogInformation("Starting recommendation background tasks at {Time}", DateTime.UtcNow);

                // Calculate trending videos
                await recommendationService.CalculateTrendingVideosAsync();

                // Clean up expired recommendations
                await CleanupExpiredRecommendationsAsync(context);

                // Clean up old interactions (keep last 90 days)
                await CleanupOldInteractionsAsync(context);

                _logger.LogInformation("Completed recommendation background tasks at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during recommendation background tasks");
            }

            await Task.Delay(_period, stoppingToken);
        }
    }

    private async Task CleanupExpiredRecommendationsAsync(DrashyamDbContext context)
    {
        try
        {
            var expiredRecommendations = await context.Recommendations
                .Where(r => r.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredRecommendations.Any())
            {
                context.Recommendations.RemoveRange(expiredRecommendations);
                await context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired recommendations", expiredRecommendations.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired recommendations");
        }
    }

    private async Task CleanupOldInteractionsAsync(DrashyamDbContext context)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-90);
            var oldInteractions = await context.UserInteractions
                .Where(i => i.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldInteractions.Any())
            {
                context.UserInteractions.RemoveRange(oldInteractions);
                await context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} old interactions", oldInteractions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old interactions");
        }
    }
}
