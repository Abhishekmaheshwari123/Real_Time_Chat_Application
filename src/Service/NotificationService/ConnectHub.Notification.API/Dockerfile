FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
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
ENTRYPOINT ["dotnet", "ConnectHub.Notification.API.dll"]
