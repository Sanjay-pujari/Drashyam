using Drashyam.API.Data;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Drashyam.API.Hubs;
using Drashyam.API.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using FluentValidation;
using Drashyam.API.Validators;
using Drashyam.API.Mapping;
using Drashyam.API.Middleware;
using Microsoft.AspNetCore.Http.Features;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Controllers & Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // Use camelCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // Allow case insensitive
        // Accept string values for enums (e.g., "Banner") in requests and emit strings in responses
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Drashyam API", 
        Version = "v1" 
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DbContext (PostgreSQL)
builder.Services.AddDbContext<DrashyamDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<DrashyamDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "change-this-dev-secret";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "Drashyam.API",
        ValidAudience = jwtSettings["Audience"] ?? "Drashyam.Client",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Allow SignalR to receive access token via query string for WebSockets/SSE
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].ToString();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && 
                (path.StartsWithSegments("/notificationHub") || 
                 path.StartsWithSegments("/liveStreamHub") || 
                 path.StartsWithSegments("/chatHub") || 
                 path.StartsWithSegments("/videoHub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
    });
    
    // Allow embedding from any origin for iframe content
    options.AddPolicy("AllowEmbedding", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
    
    // Development CORS policy - more permissive but still allows credentials
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://127.0.0.1:4200",
                    "https://127.0.0.1:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
        });
    }
});

// Azure SignalR Service Configuration
builder.Services.Configure<AzureSignalRSettings>(builder.Configuration.GetSection("AzureSignalR"));

// SignalR with Azure SignalR Service
var signalRSettings = builder.Configuration.GetSection("AzureSignalR").Get<AzureSignalRSettings>();
if (signalRSettings?.UseAzureSignalR == true && !string.IsNullOrEmpty(signalRSettings.ConnectionString))
{
    builder.Services.AddSignalR()
        .AddAzureSignalR(options =>
        {
            options.ConnectionString = signalRSettings.ConnectionString;
        });
}
else
{
    // Fallback to in-memory SignalR for development
    builder.Services.AddSignalR();
}

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Memory Cache
builder.Services.AddMemoryCache();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInviteValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReferralValidator>();

// Email Configuration
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Conditionally register SendGrid
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();
if (emailSettings?.UseSendGrid == true)
{
    var sendGridApiKey = builder.Configuration["SendGrid:ApiKey"];
    if (!string.IsNullOrEmpty(sendGridApiKey))
    {
        builder.Services.AddSingleton<ISendGridClient>(provider => new SendGridClient(sendGridApiKey));
    }
}

// Azure Storage Configuration
builder.Services.Configure<AzureStorageSettings>(builder.Configuration.GetSection("AzureStorage"));
builder.Services.AddSingleton(provider =>
{
    var settings = provider.GetRequiredService<IOptions<AzureStorageSettings>>().Value;
    
    // Check if connection string is provided
    if (string.IsNullOrEmpty(settings.ConnectionString))
    {
        // Return null for development when Azure Storage is not configured
        return null;
    }
    
    return new BlobServiceClient(settings.ConnectionString);
});

// HttpClient for Azure Communication Services (replacement for deprecated Azure Media Services)
builder.Services.AddHttpClient<IAzureCommunicationService, AzureCommunicationService>();

// Stripe Configuration
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// File upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; // 2GB
});

// Application services (current in-repo implementations)
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILiveStreamService, LiveStreamService>();
builder.Services.AddScoped<ILiveStreamChatService, LiveStreamChatService>();
builder.Services.AddScoped<ILiveStreamMonetizationService, LiveStreamMonetizationService>();
builder.Services.AddScoped<IStreamingService, StreamingService>();
builder.Services.AddScoped<IStreamAnalyticsService, StreamAnalyticsService>();
builder.Services.AddScoped<IStreamingInfrastructureService, StreamingInfrastructureService>();
builder.Services.AddScoped<IAzureCommunicationService, AzureCommunicationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IFileStorageService>(provider =>
{
    var blobServiceClient = provider.GetService<BlobServiceClient>();
    var storageSettings = provider.GetRequiredService<IOptions<AzureStorageSettings>>();
    var logger = provider.GetRequiredService<ILogger<FileStorageService>>();
    return new FileStorageService(blobServiceClient, storageSettings, logger);
});
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<ICollaborationService, CollaborationService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<IWatchLaterService, WatchLaterService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
builder.Services.AddScoped<IAdService, AdService>();
builder.Services.AddScoped<IChannelBrandingService, ChannelBrandingService>();
builder.Services.AddScoped<IPremiumContentService, PremiumContentService>();
builder.Services.AddScoped<IMerchandiseService, MerchandiseService>();
builder.Services.AddScoped<IMonetizationService, MonetizationService>();
builder.Services.AddScoped<ISuperChatService, SuperChatService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IRecommendationCacheService, RecommendationCacheService>();
builder.Services.AddScoped<IAnalyticsDashboardService, AnalyticsDashboardService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<IVideoProcessingService, VideoProcessingService>();
builder.Services.AddScoped<IPrivacyService, PrivacyService>();

// Background services
builder.Services.AddHostedService<RecommendationBackgroundService>();
builder.Services.AddHostedService<VideoProcessingBackgroundService>();

// Ensure wwwroot exists for static file hosting
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
Directory.CreateDirectory(wwwrootPath);

var app = builder.Build();

// Exception handling should be first
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Embed security middleware
app.UseMiddleware<EmbedSecurityMiddleware>();

// CORS - must be before authentication
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowFrontend");
}

// Static files for uploaded content
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Add CORS headers for static files
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
    }
});

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting (after auth to allow authenticated users)
app.UseMiddleware<RateLimitingMiddleware>();

// Swagger (should be after auth to avoid issues)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoints
app.MapControllers();
app.MapHub<VideoHub>("/videoHub");
app.MapHub<LiveStreamHub>("/liveStreamHub");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");

// Seed database only in development and only if database is empty
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<DrashyamDbContext>();
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.Initialize(ctx, userMgr, roleMgr);
        
        // Update existing videos with categories
        await Drashyam.API.Scripts.UpdateVideoCategories.UpdateExistingVideos(ctx);
    }
}

app.Run();
