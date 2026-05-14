FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Service/Gateway/ConnectHub.ApiGateway/ConnectHub.ApiGateway.csproj", "src/Service/Gateway/ConnectHub.ApiGateway/"]

RUN dotnet restore "src/Service/Gateway/ConnectHub.ApiGateway/ConnectHub.ApiGateway.csproj"

COPY . .

WORKDIR "/src/src/Service/Gateway/ConnectHub.ApiGateway"

RUN dotnet build "ConnectHub.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConnectHub.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConnectHub.ApiGateway.dll"]