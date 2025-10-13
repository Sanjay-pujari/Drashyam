export const environment = {
  production: true,
  apiUrl: 'https://api.drashyam.com/api',
  signalRUrl: 'https://api.drashyam.com',
  stripePublishableKey: 'pk_live_...', // Replace with your live Stripe publishable key
  googleClientId: '...', // Replace with your Google OAuth client ID
  facebookAppId: '...', // Replace with your Facebook App ID
  enableAnalytics: true,
  enablePWA: true,
  maxFileSize: 2 * 1024 * 1024 * 1024, // 2GB
  allowedVideoFormats: ['mp4', 'webm', 'ogg', 'mov', 'avi'],
  allowedImageFormats: ['jpg', 'jpeg', 'png', 'gif', 'webp']
};
