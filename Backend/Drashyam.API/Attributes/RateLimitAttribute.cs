using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Drashyam.API.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : ActionFilterAttribute
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private static readonly Dictionary<string, RateLimitInfo> _rateLimitStore = new();
    private static readonly object _lock = new();

    public RateLimitAttribute(int maxRequests, int windowSizeInSeconds = 60)
    {
        _maxRequests = maxRequests;
        _windowSize = TimeSpan.FromSeconds(windowSizeInSeconds);
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var clientId = GetClientIdentifier(context.HttpContext);
        var endpoint = GetEndpointIdentifier(context.HttpContext);
        var key = $"{clientId}:{endpoint}";
        var now = DateTime.UtcNow;

        lock (_lock)
        {
            if (_rateLimitStore.TryGetValue(key, out var rateLimitInfo))
            {
                // Reset window if expired
                if (now - rateLimitInfo.WindowStart > _windowSize)
                {
                    rateLimitInfo.Count = 1;
                    rateLimitInfo.WindowStart = now;
                }
                else
                {
                    rateLimitInfo.Count++;
                }
            }
            else
            {
                _rateLimitStore[key] = new RateLimitInfo { Count = 1, WindowStart = now };
            }

            var currentInfo = _rateLimitStore[key];

            // Check if rate limit exceeded
            if (currentInfo.Count > _maxRequests)
            {
                context.Result = new ObjectResult("Rate limit exceeded. Please try again later.")
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests
                };

                context.HttpContext.Response.Headers.Add("Retry-After", _windowSize.TotalSeconds.ToString());
                context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", _maxRequests.ToString());
                context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", "0");
                context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", (currentInfo.WindowStart.Add(_windowSize) - DateTime.UtcNow).TotalSeconds.ToString());
                return;
            }

            // Add rate limit headers
            context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", _maxRequests.ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", (_maxRequests - currentInfo.Count).ToString());
            context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", (currentInfo.WindowStart.Add(_windowSize) - DateTime.UtcNow).TotalSeconds.ToString());
        }

        base.OnActionExecuting(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private string GetEndpointIdentifier(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();
        return $"{method}:{path}";
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
