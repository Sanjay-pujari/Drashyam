# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Drashyam.API/Drashyam.API.csproj Drashyam.API/
RUN dotnet restore "Drashyam.API/Drashyam.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Drashyam.API"
RUN dotnet build "Drashyam.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Drashyam.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install FFmpeg for video processing
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=publish /app/publish .

# Create directories for file uploads
RUN mkdir -p /app/uploads/videos /app/uploads/thumbnails /app/logs

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "Drashyam.API.dll"]
