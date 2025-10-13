namespace Drashyam.API.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task<bool> SendWelcomeEmailAsync(string to, string firstName);
    Task<bool> SendPasswordResetEmailAsync(string to, string resetToken);
    Task<bool> SendEmailVerificationAsync(string to, string verificationToken);
    Task<bool> SendSubscriptionConfirmationAsync(string to, string planName, decimal amount);
    Task<bool> SendLiveStreamNotificationAsync(string to, string streamTitle, string channelName);
    Task<bool> SendNewSubscriberNotificationAsync(string to, string subscriberName);
    Task<bool> SendCommentNotificationAsync(string to, string commenterName, string videoTitle);
}
