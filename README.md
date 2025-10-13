# Drashyam - YouTube-like Video Platform

A comprehensive video streaming platform built with Angular frontend and .NET 9 backend.

## Features

### Core Features
- 🎥 Video upload, recording, and live streaming
- 👥 User management with invitation system
- 💬 Comments, likes, and dislikes
- 🔗 Video sharing with custom links
- 📺 Personal channels with subscription-based limits
- 💳 Free and paid subscription tiers
- 📊 Analytics and monetization tools

### Revenue Features
- Ad integration for free users
- Premium subscription tiers
- Channel monetization
- Sponsored content
- Analytics dashboard
- Content recommendations

## Technology Stack

### Frontend
- Angular 18+ with TypeScript
- Angular Material for UI components
- RxJS for reactive programming
- PWA capabilities

### Backend
- .NET 9 Web API
- Entity Framework Core
- SignalR for real-time features
- JWT Authentication
- Azure Blob Storage for video files
- Redis for caching

### Additional Services
- FFmpeg for video processing
- WebRTC for live streaming
- Stripe for payments
- SendGrid for email notifications

## Project Structure

```
Drashyam/
├── Frontend/                 # Angular application
├── Backend/                  # .NET 9 Web API
├── Shared/                   # Shared models and DTOs
├── Database/                 # Database migrations and scripts
└── Infrastructure/           # Docker, CI/CD configurations
```

## Getting Started

### Prerequisites
- Node.js 18+
- .NET 9 SDK
- SQL Server or PostgreSQL
- Redis
- FFmpeg

### Installation

1. Clone the repository
2. Set up the backend:
   ```bash
   cd Backend
   dotnet restore
   dotnet ef database update
   dotnet run
   ```

3. Set up the frontend:
   ```bash
   cd Frontend
   npm install
   ng serve
   ```

## API Documentation

The API documentation will be available at `/swagger` when running the backend.

## Contributing

Please read our contributing guidelines before submitting pull requests.

## License

This project is licensed under the MIT License.