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

# Build
RUN dotnet build "ConnectHub.Auth.API.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "ConnectHub.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.Auth.API.dll"]
