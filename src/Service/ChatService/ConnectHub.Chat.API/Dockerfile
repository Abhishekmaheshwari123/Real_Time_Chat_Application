FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
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
ENTRYPOINT ["dotnet", "ConnectHub.Chat.API.dll"]
