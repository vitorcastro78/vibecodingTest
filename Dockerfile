# Use the official .NET 9.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/SupermarketAPI.API/SupermarketAPI.API.csproj", "src/SupermarketAPI.API/"]
COPY ["src/SupermarketAPI.Application/SupermarketAPI.Application.csproj", "src/SupermarketAPI.Application/"]
COPY ["src/SupermarketAPI.Domain/SupermarketAPI.Domain.csproj", "src/SupermarketAPI.Domain/"]
COPY ["src/SupermarketAPI.Infrastructure/SupermarketAPI.Infrastructure.csproj", "src/SupermarketAPI.Infrastructure/"]
COPY ["src/SupermarketAPI.Scrapers/SupermarketAPI.Scrapers.csproj", "src/SupermarketAPI.Scrapers/"]
COPY ["src/SupermarketAPI.Notifications/SupermarketAPI.Notifications.csproj", "src/SupermarketAPI.Notifications/"]

RUN dotnet restore "src/SupermarketAPI.API/SupermarketAPI.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/SupermarketAPI.API"

# Build the application
RUN dotnet build "SupermarketAPI.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SupermarketAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Create directories for data and logs
RUN mkdir -p /app/data /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Install Chrome for Selenium (if needed for scraping)
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

# Set Chrome path for Selenium
ENV CHROME_BIN=/usr/bin/google-chrome

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost/health || exit 1

# Install curl for health check
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "SupermarketAPI.API.dll"]
