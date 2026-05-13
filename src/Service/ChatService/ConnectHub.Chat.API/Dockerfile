FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/Service/ChatService/ConnectHub.Chat.API/ConnectHub.Chat.API.csproj", "src/Service/ChatService/ConnectHub.Chat.API/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Application/ConnectHub.Chat.Application.csproj", "src/Service/ChatService/ConnectHub.Chat.Application/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Domain/ConnectHub.Chat.Domain.csproj", "src/Service/ChatService/ConnectHub.Chat.Domain/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Infrastructure/ConnectHub.Chat.Infrastructure.csproj", "src/Service/ChatService/ConnectHub.Chat.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Service/ChatService/ConnectHub.Chat.API/ConnectHub.Chat.API.csproj"

# Copy everything
COPY . .

# Move to API project
WORKDIR "/src/src/Service/ChatService/ConnectHub.Chat.API"

# Build
RUN dotnet build "ConnectHub.Chat.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ConnectHub.Chat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.Chat.API.dll"]
