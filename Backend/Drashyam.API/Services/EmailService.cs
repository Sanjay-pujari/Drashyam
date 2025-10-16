namespace Drashyam.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);
        return true;
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string firstName)
    {
        _logger.LogInformation("Sending welcome email to {To}", to);
        return true;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken)
    {
        _logger.LogInformation("Sending password reset email to {To}", to);
        return true;
    }

    public async Task<bool> SendEmailVerificationAsync(string to, string verificationToken)
    {
        _logger.LogInformation("Sending email verification to {To}", to);
        return true;
    }

    public async Task<bool> SendSubscriptionConfirmationAsync(string to, string planName, decimal amount)
    {
        _logger.LogInformation("Sending subscription confirmation to {To}", to);
        return true;
    }

    public async Task<bool> SendLiveStreamNotificationAsync(string to, string streamTitle, string channelName)
    {
        _logger.LogInformation("Sending live stream notification to {To}", to);
        return true;
    }

    public async Task<bool> SendNewSubscriberNotificationAsync(string to, string subscriberName)
    {
        _logger.LogInformation("Sending new subscriber notification to {To}", to);
        return true;
    }

    public async Task<bool> SendCommentNotificationAsync(string to, string commenterName, string videoTitle)
    {
        _logger.LogInformation("Sending comment notification to {To}", to);
        return true;
    }
}
