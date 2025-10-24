export const environment = {
  production: false,
  apiUrl: 'https://localhost:56379', // .NET API URL (using HTTPS for development)
  signalRUrl: 'https://localhost:56379/hubs', // Development: Local SignalR
  azureSignalR: {
    enabled: false, // Use local SignalR for development
    connectionString: '', // Not needed for development
    hubPrefix: 'drashyam'
  },
  stripePublishableKey: 'pk_test_your_stripe_publishable_key_here',
  googleClientId: 'your_google_client_id_here',
  facebookAppId: 'your_facebook_app_id_here',
  azureStorageAccount: 'your_azure_storage_account_here',
  azureStorageKey: 'your_azure_storage_key_here',
  azureStorageContainer: 'videos',
  maxVideoSize: 2 * 1024 * 1024 * 1024, // 2GB
  supportedVideoFormats: ['mp4', 'webm', 'ogg', 'avi', 'mov'],
  supportedImageFormats: ['jpg', 'jpeg', 'png', 'gif', 'webp']
};