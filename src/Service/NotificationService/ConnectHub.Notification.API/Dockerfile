FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
<<<<<<< HEAD
COPY ["ConnectHub.Notification.API/ConnectHub.Notification.API.csproj", "ConnectHub.Notification.API/"]
COPY ["ConnectHub.Notification.Application/ConnectHub.Notification.Application.csproj", "ConnectHub.Notification.Application/"]
COPY ["ConnectHub.Notification.Domain/ConnectHub.Notification.Domain.csproj", "ConnectHub.Notification.Domain/"]
COPY ["ConnectHub.Notification.Infrastructure/ConnectHub.Notification.Infrastructure.csproj", "ConnectHub.Notification.Infrastructure/"]
RUN dotnet restore "ConnectHub.Notification.API/ConnectHub.Notification.API.csproj"
COPY . .
WORKDIR "/src/ConnectHub.Notification.API"
RUN dotnet build "ConnectHub.Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectHub.Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
=======

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

>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
ENTRYPOINT ["dotnet", "ConnectHub.Notification.API.dll"]
