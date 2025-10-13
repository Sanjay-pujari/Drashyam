# Drashyam - YouTube-like Video Portal

Drashyam is a comprehensive video streaming platform built with modern web technologies, designed to provide a YouTube-like experience with advanced features for content creators and viewers.

## 🚀 Features

### Core Features
- **Video Upload & Processing**: Support for multiple video formats with automatic transcoding
- **Video Player**: Advanced video player with playback controls, quality selection, and keyboard shortcuts
- **Live Streaming**: Real-time streaming capabilities with viewer interaction
- **User Channels**: Personal and business channels with customization options
- **Comments & Interactions**: Rich commenting system with likes, replies, and moderation
- **Subscriptions**: Channel subscription system with notifications
- **Search & Discovery**: Advanced search with filters and recommendations
- **Playlists**: Video organization and collection features

### Monetization Features
- **Subscription Tiers**: Free, Premium, and Pro plans with different features
- **Ad Integration**: Server-side and client-side ad serving
- **Premium Content**: Paywall system for exclusive content
- **Super Chat/Donations**: Real-time viewer support during live streams
- **Analytics Dashboard**: Comprehensive creator analytics and insights

### Social Features
- **User Profiles**: Customizable user profiles with social links
- **Notifications**: Real-time notifications for uploads, comments, and subscriptions
- **Sharing**: Social media integration and embeddable video players
- **Community**: User interactions and community features

## 🛠 Technology Stack

### Frontend
- **Angular 18**: Modern TypeScript framework
- **NgRx**: State management with Redux pattern
- **Angular Material**: UI component library
- **RxJS**: Reactive programming
- **Video.js**: Advanced video player
- **SignalR**: Real-time communication

### Backend
- **.NET 9**: Latest .NET framework
- **ASP.NET Core Web API**: RESTful API services
- **Entity Framework Core**: ORM for database operations
- **PostgreSQL**: Primary database
- **Redis**: Caching and session storage
- **SignalR**: Real-time features
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging

### Infrastructure
- **Docker**: Containerization
- **Azure Blob Storage**: Video and image storage
- **Azure Media Services**: Video processing and streaming
- **Stripe**: Payment processing
- **SendGrid**: Email services
- **OAuth 2.0**: Social authentication

## 📁 Project Structure

```
Drashyam/
├── Backend/
│   └── Drashyam.API/
│       ├── Controllers/          # API controllers
│       ├── Data/                # Database context and migrations
│       ├── DTOs/                # Data transfer objects
│       ├── Hubs/                # SignalR hubs
│       ├── Models/              # Entity models
│       ├── Services/            # Business logic services
│       ├── Validators/          # FluentValidation validators
│       ├── Middleware/          # Custom middleware
│       └── Mapping/             # AutoMapper profiles
├── Frontend/
│   └── src/
│       ├── app/
│       │   ├── components/      # Angular components
│       │   ├── services/        # Angular services
│       │   ├── models/          # TypeScript models
│       │   ├── store/           # NgRx store
│       │   └── guards/          # Route guards
│       ├── assets/              # Static assets
│       └── environments/        # Environment configurations
└── docker-compose.yml           # Docker orchestration
```

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- PostgreSQL 14+
- Redis 6+
- Docker (optional)

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/drashyam.git
   cd drashyam
   ```

2. **Navigate to backend directory**
   ```bash
   cd Backend/Drashyam.API
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Configure database connection**
   Update `appsettings.json` with your PostgreSQL connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=drashyam;Username=postgres;Password=your_password"
     }
   }
   ```

5. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

6. **Start the API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd Frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure environment**
   Update `src/environments/environment.ts` with your API URL:
   ```typescript
   export const environment = {
     apiUrl: 'https://localhost:7001'
   };
   ```

4. **Start the development server**
   ```bash
   ng serve
   ```

### Docker Setup (Alternative)

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

## 🔧 Configuration

### Environment Variables

#### Backend (.NET)
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection string
- `ConnectionStrings:Redis`: Redis connection string
- `JwtSettings:SecretKey`: JWT signing key
- `AzureStorage:ConnectionString`: Azure Blob Storage connection
- `Stripe:SecretKey`: Stripe payment processing key
- `SendGrid:ApiKey`: SendGrid email service key

#### Frontend (Angular)
- `apiUrl`: Backend API URL
- `stripePublishableKey`: Stripe public key
- `googleClientId`: Google OAuth client ID
- `facebookAppId`: Facebook OAuth app ID

### Database Schema

The application uses Entity Framework Core with the following main entities:
- **Users**: User accounts and profiles
- **Channels**: User channels and branding
- **Videos**: Video content and metadata
- **Comments**: Video comments and replies
- **Subscriptions**: Channel subscriptions
- **LiveStreams**: Live streaming sessions
- **Analytics**: View and engagement data
- **Playlists**: Video collections
- **Notifications**: User notifications
- **AdCampaigns**: Advertisement management

## 🎯 Key Features Implementation

### Video Upload & Processing
- Multi-format video support (MP4, WebM, AVI, MOV)
- Automatic thumbnail generation
- Video transcoding for multiple qualities
- Progress tracking and status updates
- File size and format validation

### Advanced Video Player
- Custom video player with Video.js
- Multiple quality options
- Playback speed control
- Keyboard shortcuts
- Fullscreen support
- View tracking and analytics

### Live Streaming
- Real-time streaming with WebRTC
- Viewer count tracking
- Live chat integration
- Stream recording
- Stream key management

### Monetization System
- Subscription-based revenue model
- Ad integration with multiple providers
- Premium content paywall
- Creator revenue sharing
- Payment processing with Stripe

### Analytics Dashboard
- View count and engagement metrics
- Revenue tracking
- Audience demographics
- Performance insights
- Export capabilities

## 🔒 Security Features

- JWT-based authentication
- Role-based authorization
- Input validation and sanitization
- CORS configuration
- Rate limiting
- File upload security
- SQL injection prevention
- XSS protection

## 📱 Responsive Design

The application is fully responsive and optimized for:
- Desktop computers
- Tablets
- Mobile phones
- Various screen resolutions

## 🧪 Testing

### Backend Testing
```bash
cd Backend/Drashyam.API
dotnet test
```

### Frontend Testing
```bash
cd Frontend
ng test
```

### E2E Testing
```bash
cd Frontend
ng e2e
```

## 🚀 Deployment

### Production Deployment

1. **Build the frontend**
   ```bash
   cd Frontend
   ng build --prod
   ```

2. **Publish the backend**
   ```bash
   cd Backend/Drashyam.API
   dotnet publish -c Release -o ./publish
   ```

3. **Deploy with Docker**
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```

### Environment Setup
- Configure production environment variables
- Set up SSL certificates
- Configure CDN for static assets
- Set up monitoring and logging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- YouTube for inspiration
- Angular team for the excellent framework
- .NET team for the robust backend framework
- All contributors and open-source libraries used

## 📞 Support

For support and questions:
- Create an issue on GitHub
- Email: support@drashyam.com
- Documentation: [docs.drashyam.com](https://docs.drashyam.com)

---

**Drashyam** - Empowering creators, engaging audiences, revolutionizing video content.