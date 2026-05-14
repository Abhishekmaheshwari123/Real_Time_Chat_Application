<<<<<<< HEAD
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
=======
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/Service/AuthService/ConnectHub.Auth.API/ConnectHub.Auth.API.csproj", "src/Service/AuthService/ConnectHub.Auth.API/"]
COPY ["src/Service/AuthService/ConnectHub.Auth.Application/ConnectHub.Auth.Application.csproj", "src/Service/AuthService/ConnectHub.Auth.Application/"]
COPY ["src/Service/AuthService/ConnectHub.Auth.Domain/ConnectHub.Auth.Domain.csproj", "src/Service/AuthService/ConnectHub.Auth.Domain/"]
COPY ["src/Service/AuthService/ConnectHub.Auth.Infrastructure/ConnectHub.Auth.Infrastructure.csproj", "src/Service/AuthService/ConnectHub.Auth.Infrastructure/"]

# Restore
RUN dotnet restore "src/Service/AuthService/ConnectHub.Auth.API/ConnectHub.Auth.API.csproj"

# Copy everything
COPY . .

# Move into API project
WORKDIR "/src/src/Service/AuthService/ConnectHub.Auth.API"
>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f

# Build
RUN dotnet build "ConnectHub.Auth.API.csproj" -c Release -o /app/build

<<<<<<< HEAD

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
=======
FROM build AS publish

RUN dotnet publish "ConnectHub.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

<<<<<<< HEAD
ENTRYPOINT ["dotnet", "ConnectHub.Auth.API.dll"]
=======
ENTRYPOINT ["dotnet", "ConnectHub.Auth.API.dll"]
>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
