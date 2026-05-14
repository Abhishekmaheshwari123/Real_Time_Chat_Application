FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
<<<<<<< HEAD
COPY ["ConnectHub.Chat.API/ConnectHub.Chat.API.csproj", "ConnectHub.Chat.API/"]
COPY ["ConnectHub.Chat.Application/ConnectHub.Chat.Application.csproj", "ConnectHub.Chat.Application/"]
COPY ["ConnectHub.Chat.Domain/ConnectHub.Chat.Domain.csproj", "ConnectHub.Chat.Domain/"]
COPY ["ConnectHub.Chat.Infrastructure/ConnectHub.Chat.Infrastructure.csproj", "ConnectHub.Chat.Infrastructure/"]
RUN dotnet restore "ConnectHub.Chat.API/ConnectHub.Chat.API.csproj"
COPY . .
WORKDIR "/src/ConnectHub.Chat.API"
RUN dotnet build "ConnectHub.Chat.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectHub.Chat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
=======

# Copy project files
COPY ["src/Service/ChatService/ConnectHub.Chat.API/ConnectHub.Chat.API.csproj", "src/Service/ChatService/ConnectHub.Chat.API/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Application/ConnectHub.Chat.Application.csproj", "src/Service/ChatService/ConnectHub.Chat.Application/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Domain/ConnectHub.Chat.Domain.csproj", "src/Service/ChatService/ConnectHub.Chat.Domain/"]

COPY ["src/Service/ChatService/ConnectHub.Chat.Infrastructure/ConnectHub.Chat.Infrastructure.csproj", "src/Service/ChatService/ConnectHub.Chat.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Service/ChatService/ConnectHub.Chat.API/ConnectHub.Chat.API.csproj"

# Copy all source code
COPY . .

# Move into API project directory
WORKDIR "/src/src/Service/ChatService/ConnectHub.Chat.API"

# Build
RUN dotnet build "ConnectHub.Chat.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ConnectHub.Chat.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

>>>>>>> 63943581c9f79be279c2a0841674ffa1d34db81f
ENTRYPOINT ["dotnet", "ConnectHub.Chat.API.dll"]
