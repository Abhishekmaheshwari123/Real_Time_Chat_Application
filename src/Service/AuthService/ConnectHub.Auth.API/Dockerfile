FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConnectHub.Auth.API/ConnectHub.Auth.API.csproj", "ConnectHub.Auth.API/"]
COPY ["ConnectHub.Auth.Application/ConnectHub.Auth.Application.csproj", "ConnectHub.Auth.Application/"]
COPY ["ConnectHub.Auth.Domain/ConnectHub.Auth.Domain.csproj", "ConnectHub.Auth.Domain/"]
COPY ["ConnectHub.Auth.Infrastructure/ConnectHub.Auth.Infrastructure.csproj", "ConnectHub.Auth.Infrastructure/"]
RUN dotnet restore "ConnectHub.Auth.API/ConnectHub.Auth.API.csproj"
COPY . .
WORKDIR "/src/ConnectHub.Auth.API"
RUN dotnet build "ConnectHub.Auth.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectHub.Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConnectHub.Auth.API.dll"]
