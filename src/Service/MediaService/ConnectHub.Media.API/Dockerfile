FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConnectHub.Media.API/ConnectHub.Media.API.csproj", "ConnectHub.Media.API/"]
COPY ["ConnectHub.Media.Application/ConnectHub.Media.Application.csproj", "ConnectHub.Media.Application/"]
COPY ["ConnectHub.Media.Domain/ConnectHub.Media.Domain.csproj", "ConnectHub.Media.Domain/"]
COPY ["ConnectHub.Media.Infrastructure/ConnectHub.Media.Infrastructure.csproj", "ConnectHub.Media.Infrastructure/"]
RUN dotnet restore "ConnectHub.Media.API/ConnectHub.Media.API.csproj"
COPY . .
WORKDIR "/src/ConnectHub.Media.API"
RUN dotnet build "ConnectHub.Media.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectHub.Media.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConnectHub.Media.API.dll"]
