using Microsoft.AspNetCore.Http;

namespace Drashyam.API.Middleware;

public class EmbedSecurityMiddleware
{
    private readonly RequestDelegate _next;

    public EmbedSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add iframe security headers for embed routes
        if (context.Request.Path.StartsWithSegments("/embed"))
        {
            // Allow iframe embedding from any origin
            context.Response.Headers.Append("X-Frame-Options", "ALLOWALL");
            
            // Remove X-Frame-Options to allow embedding
            context.Response.Headers.Remove("X-Frame-Options");
            
            // Add Content Security Policy for iframe embedding
            context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors *");
            
            // Allow all origins for CORS
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
        }

        await _next(context);
    }
}
