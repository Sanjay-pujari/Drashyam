using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Drashyam.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _templateService;
    private readonly ISendGridClient? _sendGridClient;

    public EmailService(
        ILogger<EmailService> logger, 
        IOptions<EmailSettings> emailSettings, 
        IEmailTemplateService templateService,
        ISendGridClient? sendGridClient = null)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
        _templateService = templateService;
        _sendGridClient = sendGridClient;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            if (_emailSettings.UseSendGrid && _sendGridClient != null)
            {
                return await SendEmailViaSendGridAsync(to, subject, body, isHtml);
            }
            else
            {
                return await SendEmailViaSmtpAsync(to, subject, body, isHtml);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    private async Task<bool> SendEmailViaSmtpAsync(string to, string subject, string body, bool isHtml = true)
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
            _logger.LogInformation("Email sent successfully via SMTP to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP to {Email}", to);
            return false;
        }
    }

    private async Task<bool> SendEmailViaSendGridAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var from = new EmailAddress(_emailSettings.FromEmail, _emailSettings.FromName);
            var toEmail = new EmailAddress(to);
            
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, 
                isHtml ? string.Empty : body, 
                isHtml ? body : string.Empty);

            var response = await _sendGridClient!.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully via SendGrid to {Email}", to);
                return true;
            }
            else
            {
                _logger.LogError("SendGrid email failed with status {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SendGrid to {Email}", to);
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
        // Properly encode the token to prevent corruption
        var encodedToken = Uri.EscapeDataString(resetToken);
        var resetLink = $"{_emailSettings.BaseUrl}/reset-password?token={encodedToken}";
        var subject = "Reset Your Drashyam Password";
        var body = _templateService.GetPasswordResetEmailTemplate(resetLink);
        return await SendEmailAsync(to, subject, body);
    }

    public async Task<bool> SendEmailVerificationAsync(string to, string verificationToken)
    {
        // Properly encode the token to prevent corruption
        var encodedToken = Uri.EscapeDataString(verificationToken);
        var verificationLink = $"{_emailSettings.BaseUrl}/verify-email?token={encodedToken}";
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
