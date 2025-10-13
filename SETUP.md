# Drashyam Setup Guide

## Prerequisites

- .NET 9 SDK
- Node.js 18+
- SQL Server (LocalDB or SQL Server)
- Redis
- FFmpeg
- Docker (optional)

## Quick Start with Docker

1. Clone the repository:
```bash
git clone <repository-url>
cd Drashyam
```

2. Update configuration files:
   - Update `Backend/Drashyam.API/appsettings.json` with your database connection strings
   - Update `Frontend/src/environments/environment.ts` with your API URLs
   - Add your Stripe keys, SendGrid API key, and Azure Storage connection string

3. Run with Docker Compose:
```bash
docker-compose up -d
```

4. Access the application:
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:7001
   - API Documentation: http://localhost:7001/swagger

## Manual Setup

### Backend Setup

1. Navigate to the backend directory:
```bash
cd Backend/Drashyam.API
```

2. Restore packages:
```bash
dotnet restore
```

3. Update database:
```bash
dotnet ef database update
```

4. Run the application:
```bash
dotnet run
```

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd Frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
ng serve
```

## Configuration

### Database Configuration

Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DrashyamDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Azure Storage Configuration

For video file storage, configure Azure Blob Storage:
```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorageaccount;AccountKey=yourkey;EndpointSuffix=core.windows.net",
    "ContainerName": "videos"
  }
}
```

### Stripe Configuration

For payment processing:
```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### SendGrid Configuration

For email notifications:
```json
{
  "SendGrid": {
    "ApiKey": "SG....",
    "FromEmail": "noreply@drashyam.com",
    "FromName": "Drashyam"
  }
}
```

## Features Implemented

### Core Features ✅
- [x] User authentication and authorization
- [x] Video upload, processing, and playback
- [x] Live streaming capabilities
- [x] Channel management
- [x] Comments, likes, and dislikes
- [x] Video sharing with custom links
- [x] User subscriptions (Free, Premium, Pro)
- [x] Real-time notifications with SignalR

### Revenue Features ✅
- [x] Ad placement system (pre-roll, mid-roll, post-roll)
- [x] Sponsor management
- [x] Donation system
- [x] Merchandise store
- [x] Analytics dashboard
- [x] Monetization tools
- [x] Revenue reporting

### Additional Revenue Features ✅
- [x] Premium content gating
- [x] Channel subscriptions
- [x] Super Chat/Super Stickers
- [x] Channel memberships
- [x] Affiliate marketing tools
- [x] Brand partnership management
- [x] Advanced analytics and insights

## API Endpoints

### Authentication
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/forgot-password` - Forgot password
- `POST /api/auth/reset-password` - Reset password

### Videos
- `GET /api/videos` - Get videos with filtering
- `POST /api/videos/upload` - Upload video
- `GET /api/videos/{id}` - Get video by ID
- `PUT /api/videos/{id}` - Update video
- `DELETE /api/videos/{id}` - Delete video
- `POST /api/videos/{id}/like` - Like/dislike video
- `POST /api/videos/{id}/view` - Record video view

### Channels
- `GET /api/channels` - Get channels
- `POST /api/channels` - Create channel
- `GET /api/channels/{id}` - Get channel by ID
- `PUT /api/channels/{id}` - Update channel
- `DELETE /api/channels/{id}` - Delete channel
- `POST /api/channels/{id}/subscribe` - Subscribe to channel

### Live Streaming
- `GET /api/livestreams` - Get live streams
- `POST /api/livestreams` - Create live stream
- `GET /api/livestreams/{id}` - Get live stream by ID
- `PUT /api/livestreams/{id}` - Update live stream
- `POST /api/livestreams/{id}/start` - Start live stream
- `POST /api/livestreams/{id}/end` - End live stream

### Analytics
- `GET /api/analytics/video/{id}` - Get video analytics
- `GET /api/analytics/channel/{id}` - Get channel analytics
- `GET /api/analytics/revenue` - Get revenue analytics
- `GET /api/analytics/audience` - Get audience insights

### Monetization
- `GET /api/monetization/status` - Get monetization status
- `POST /api/monetization/enable` - Enable monetization
- `GET /api/monetization/ads` - Get ad placements
- `POST /api/monetization/ads` - Create ad placement
- `GET /api/monetization/sponsors` - Get sponsors
- `POST /api/monetization/sponsors` - Create sponsor
- `GET /api/monetization/donations` - Get donations
- `POST /api/monetization/donations` - Process donation

## Deployment

### Production Deployment

1. Build the frontend:
```bash
cd Frontend
npm run build
```

2. Publish the backend:
```bash
cd Backend/Drashyam.API
dotnet publish -c Release -o ./publish
```

3. Deploy to your preferred cloud provider (Azure, AWS, Google Cloud)

### Environment Variables

Set the following environment variables in production:

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<production-connection-string>
ConnectionStrings__Redis=<production-redis-connection>
JwtSettings__SecretKey=<production-secret-key>
AzureStorage__ConnectionString=<production-storage-connection>
Stripe__SecretKey=<production-stripe-key>
SendGrid__ApiKey=<production-sendgrid-key>
```

## Support

For support and questions, please contact the development team or create an issue in the repository.
