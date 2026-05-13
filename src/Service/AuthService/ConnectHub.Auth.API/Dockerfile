# =========================
# Base Runtime Image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

# Cloud platforms usually provide PORT dynamically
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080


# =========================
# Build Image
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj files
COPY ["ConnectHub.Auth.API/ConnectHub.Auth.API.csproj", "ConnectHub.Auth.API/"]
COPY ["ConnectHub.Auth.Application/ConnectHub.Auth.Application.csproj", "ConnectHub.Auth.Application/"]
COPY ["ConnectHub.Auth.Domain/ConnectHub.Auth.Domain.csproj", "ConnectHub.Auth.Domain/"]
COPY ["ConnectHub.Auth.Infrastructure/ConnectHub.Auth.Infrastructure.csproj", "ConnectHub.Auth.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "ConnectHub.Auth.API/ConnectHub.Auth.API.csproj"

# Copy full source
COPY . .

# Move into API project
WORKDIR "/src/ConnectHub.Auth.API"

# Build
RUN dotnet build "ConnectHub.Auth.API.csproj" -c Release -o /app/build


# =========================
# Publish Image
# =========================
FROM build AS publish

RUN dotnet publish "ConnectHub.Auth.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# =========================
# Final Runtime Image
# =========================
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.Auth.API.dll"]