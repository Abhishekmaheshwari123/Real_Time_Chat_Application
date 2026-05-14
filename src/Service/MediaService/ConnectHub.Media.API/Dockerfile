FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/Service/MediaService/ConnectHub.Media.API/ConnectHub.Media.API.csproj", "src/Service/MediaService/ConnectHub.Media.API/"]

COPY ["src/Service/MediaService/ConnectHub.Media.Application/ConnectHub.Media.Application.csproj", "src/Service/MediaService/ConnectHub.Media.Application/"]

COPY ["src/Service/MediaService/ConnectHub.Media.Domain/ConnectHub.Media.Domain.csproj", "src/Service/MediaService/ConnectHub.Media.Domain/"]

COPY ["src/Service/MediaService/ConnectHub.Media.Infrastructure/ConnectHub.Media.Infrastructure.csproj", "src/Service/MediaService/ConnectHub.Media.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Service/MediaService/ConnectHub.Media.API/ConnectHub.Media.API.csproj"

# Copy all source code
COPY . .

# Move into API project
WORKDIR "/src/src/Service/MediaService/ConnectHub.Media.API"

# Build
RUN dotnet build "ConnectHub.Media.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ConnectHub.Media.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.Media.API.dll"]
