using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Drashyam.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _templateService;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings, IEmailTemplateService templateService)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _templateService = templateService;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                EnableSsl = _emailSettings.EnableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string to, string firstName)
    {
        var subject = "Welcome to Drashyam!";
        var body = _templateService.GetWelcomeEmailTemplate(firstName);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetToken)
    {
        var resetLink = $"{_emailSettings.BaseUrl}/reset-password?token={resetToken}";
        var subject = "Reset Your Drashyam Password";
        var body = _templateService.GetPasswordResetEmailTemplate(resetLink);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendEmailVerificationAsync(string to, string verificationToken)
    {
        var verificationLink = $"{_emailSettings.BaseUrl}/verify-email?token={verificationToken}";
        var subject = "Verify Your Email Address";
        var body = _templateService.GetEmailVerificationTemplate(verificationLink);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendSubscriptionConfirmationAsync(string to, string planName, decimal amount)
    {
        var subject = "Subscription Confirmed - Drashyam";
        var body = _templateService.GetSubscriptionConfirmationTemplate(planName, amount);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendLiveStreamNotificationAsync(string to, string streamTitle, string channelName)
    {
        var subject = $"🔴 LIVE: {streamTitle}";
        var body = _templateService.GetLiveStreamNotificationTemplate(streamTitle, channelName);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendNewSubscriberNotificationAsync(string to, string subscriberName)
    {
        var subject = "🎉 New Subscriber!";
        var body = _templateService.GetNewSubscriberNotificationTemplate(subscriberName);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendCommentNotificationAsync(string to, string commenterName, string videoTitle)
    {
        var subject = $"💬 New Comment on {videoTitle}";
        var body = _templateService.GetCommentNotificationTemplate(commenterName, videoTitle);
        return await SendEmailAsync(to, subject, body);
    }
}
