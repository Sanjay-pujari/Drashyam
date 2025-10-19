using System.Collections.Concurrent;
using System.Net;

namespace Drashyam.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();
    private readonly TimeSpan _windowSize = TimeSpan.FromMinutes(1);
    private readonly int _maxRequestsPerWindow;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxRequestsPerWindow = configuration.GetValue<int>("RateLimiting:MaxRequestsPerWindow", 100);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointIdentifier(context);

        // Skip rate limiting for Swagger and health endpoints
        if (endpoint == "swagger:skip")
        {
            await _next(context);
            return;
        }

        var key = $"{clientId}:{endpoint}";
        var now = DateTime.UtcNow;

        var rateLimitInfo = _rateLimitStore.AddOrUpdate(key, 
            new RateLimitInfo { Count = 1, WindowStart = now },
            (k, existing) =>
            {
                if (now - existing.WindowStart > _windowSize)
                {
                    return new RateLimitInfo { Count = 1, WindowStart = now };
                }
                return new RateLimitInfo { Count = existing.Count + 1, WindowStart = existing.WindowStart };
            });

        // Check if rate limit exceeded
        if (rateLimitInfo.Count > _maxRequestsPerWindow)
        {

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _windowSize.TotalSeconds.ToString());
            context.Response.Headers.Add("X-RateLimit-Limit", _maxRequestsPerWindow.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");
            context.Response.Headers.Add("X-RateLimit-Reset", (rateLimitInfo.WindowStart.Add(_windowSize) - DateTime.UtcNow).TotalSeconds.ToString());

            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", _maxRequestsPerWindow.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", (_maxRequestsPerWindow - rateLimitInfo.Count).ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", (rateLimitInfo.WindowStart.Add(_windowSize) - DateTime.UtcNow).TotalSeconds.ToString());

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from JWT token first
        var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        // Skip rate limiting for Swagger endpoints
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
            return "swagger:skip";

        // Group similar endpoints for rate limiting
        if (path.StartsWith("/api/invite") && method == "POST")
            return "invite:create";
        if (path.StartsWith("/api/invite") && method == "GET")
            return "invite:read";
        if (path.StartsWith("/api/referral") && method == "POST")
            return "referral:create";
        if (path.StartsWith("/api/referral") && method == "GET")
            return "referral:read";
        if (path.StartsWith("/api/auth") && method == "POST")
            return "auth:login";

        return $"{method}:{path}";
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
