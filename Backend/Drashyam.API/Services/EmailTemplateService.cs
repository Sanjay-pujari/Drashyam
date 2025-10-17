namespace Drashyam.API.Services;

public interface IEmailTemplateService
{
    string GetInviteEmailTemplate(string inviterName, string personalMessage, string inviteLink, DateTime expiresAt);
    string GetWelcomeEmailTemplate(string firstName);
    string GetPasswordResetEmailTemplate(string resetLink);
    string GetEmailVerificationTemplate(string verificationLink);
    string GetSubscriptionConfirmationTemplate(string planName, decimal amount);
    string GetLiveStreamNotificationTemplate(string streamTitle, string channelName);
    string GetNewSubscriberNotificationTemplate(string subscriberName);
    string GetCommentNotificationTemplate(string commenterName, string videoTitle);
}

public class EmailTemplateService : IEmailTemplateService
{
    public string GetInviteEmailTemplate(string inviterName, string personalMessage, string inviteLink, DateTime expiresAt)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>You're Invited to Drashyam!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 You're Invited to Drashyam!</h1>
        </div>
        <div class='content'>
            <h2>Hello!</h2>
            <p><strong>{inviterName}</strong> has invited you to join Drashyam, the ultimate video platform for creators and viewers.</p>
            
            {(string.IsNullOrEmpty(personalMessage) ? "" : $"<div style='background: #e3f2fd; padding: 15px; border-left: 4px solid #2196f3; margin: 20px 0;'><strong>Personal Message:</strong><br>\"{personalMessage}\"</div>")}
            
            <p>With Drashyam, you can:</p>
            <ul>
                <li>📹 Upload and share your videos</li>
                <li>🎥 Create live streams</li>
                <li>👥 Build your audience</li>
                <li>💰 Monetize your content</li>
                <li>📊 Track your analytics</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{inviteLink}' class='button'>Accept Invitation & Join Now</a>
            </div>
            
            <p><strong>This invitation expires on {expiresAt:MMMM dd, yyyy 'at' h:mm tt}.</strong></p>
            
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #2563eb;'>{inviteLink}</p>
        </div>
        <div class='footer'>
            <p>This invitation was sent by {inviterName}. If you didn't expect this invitation, you can safely ignore this email.</p>
            <p>© 2024 Drashyam. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetWelcomeEmailTemplate(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to Drashyam!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Drashyam, {firstName}!</h1>
        </div>
        <div class='content'>
            <p>We're thrilled to have you join our community of creators and viewers!</p>
            
            <h3>Get Started:</h3>
            <ul>
                <li>📹 Upload your first video</li>
                <li>🎥 Start a live stream</li>
                <li>👥 Connect with other creators</li>
                <li>📊 Explore analytics</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='button'>Start Creating</a>
            </div>
            
            <p>Need help? Check out our <a href='#'>Getting Started Guide</a> or contact our support team.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetPasswordResetEmailTemplate(string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc2626; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>We received a request to reset your password for your Drashyam account.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>
            
            <p><strong>This link will expire in 1 hour for security reasons.</strong></p>
            
            <p>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetEmailVerificationTemplate(string verificationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your Email</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #059669; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #059669; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Verify Your Email Address</h1>
        </div>
        <div class='content'>
            <p>Please verify your email address to complete your Drashyam account setup.</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{verificationLink}' class='button'>Verify Email</a>
            </div>
            
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #059669;'>{verificationLink}</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetSubscriptionConfirmationTemplate(string planName, decimal amount)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Subscription Confirmed</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #7c3aed; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Subscription Confirmed!</h1>
        </div>
        <div class='content'>
            <p>Thank you for upgrading to <strong>{planName}</strong>!</p>
            <p>Amount: <strong>${amount:F2}</strong></p>
            <p>You now have access to all premium features. Start creating amazing content!</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetLiveStreamNotificationTemplate(string streamTitle, string channelName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Live Stream Started</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc2626; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #dc2626; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔴 LIVE NOW</h1>
        </div>
        <div class='content'>
            <p><strong>{channelName}</strong> is now live streaming:</p>
            <h3>{streamTitle}</h3>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' class='button'>Watch Live Stream</a>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    public string GetNewSubscriberNotificationTemplate(string subscriberName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Subscriber!</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #059669; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 New Subscriber!</h1>
        </div>
        <div class='content'>
            <p><strong>{subscriberName}</strong> just subscribed to your channel!</p>
            <p>Keep creating amazing content to grow your audience even more.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GetCommentNotificationTemplate(string commenterName, string videoTitle)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>New Comment</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #2563eb; color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>💬 New Comment</h1>
        </div>
        <div class='content'>
            <p><strong>{commenterName}</strong> commented on your video:</p>
            <h3>{videoTitle}</h3>
            <p>Engage with your audience by replying to their comments!</p>
        </div>
    </div>
</body>
</html>";
    }
}
