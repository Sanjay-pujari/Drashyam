export const environment = {
  production: true,
  apiUrl: 'https://drashyam-api.azurewebsites.net', // Production API URL
  signalRUrl: 'https://drashyam-signalr.service.signalr.net', // Azure SignalR Service URL
  azureSignalR: {
    enabled: true, // Use Azure SignalR Service for production
    connectionString: 'Endpoint=https://drashyam-signalr.service.signalr.net;AccessKey=your-access-key;Version=1.0;',
    hubPrefix: 'drashyam'
  },
  stripePublishableKey: 'pk_live_your_stripe_publishable_key_here',
  googleClientId: 'your_google_client_id_here',
  facebookAppId: 'your_facebook_app_id_here',
  azureStorageAccount: 'drashyamstorage',
  azureStorageKey: 'your_azure_storage_key_here',
  azureStorageContainer: 'videos',
  maxVideoSize: 2 * 1024 * 1024 * 1024, // 2GB
  supportedVideoFormats: ['mp4', 'webm', 'ogg', 'avi', 'mov'],
  supportedImageFormats: ['jpg', 'jpeg', 'png', 'gif', 'webp']
};