FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/Service/NotificationService/ConnectHub.Notification.API/ConnectHub.Notification.API.csproj", "src/Service/NotificationService/ConnectHub.Notification.API/"]

COPY ["src/Service/NotificationService/ConnectHub.Notification.Application/ConnectHub.Notification.Application.csproj", "src/Service/NotificationService/ConnectHub.Notification.Application/"]

COPY ["src/Service/NotificationService/ConnectHub.Notification.Domain/ConnectHub.Notification.Domain.csproj", "src/Service/NotificationService/ConnectHub.Notification.Domain/"]

COPY ["src/Service/NotificationService/ConnectHub.Notification.Infrastructure/ConnectHub.Notification.Infrastructure.csproj", "src/Service/NotificationService/ConnectHub.Notification.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Service/NotificationService/ConnectHub.Notification.API/ConnectHub.Notification.API.csproj"

# Copy all source code
COPY . .

# Move into API project
WORKDIR "/src/src/Service/NotificationService/ConnectHub.Notification.API"

# Build
RUN dotnet build "ConnectHub.Notification.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ConnectHub.Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.Notification.API.dll"]
