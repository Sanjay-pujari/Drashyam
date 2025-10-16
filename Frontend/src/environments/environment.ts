export const environment = {
  production: false,
  apiUrl: 'https://localhost:56379/api', // .NET API URL
  signalRUrl: 'https://localhost:56379/api/hubs',
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