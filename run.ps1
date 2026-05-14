# run.ps1 - Startup script for ConnectHub

Write-Host "Starting Backend Services..." -ForegroundColor Cyan

# Start Auth Service
Start-Process "dotnet" -ArgumentList "run --project src/Service/AuthService/ConnectHub.Auth.API/ConnectHub.Auth.API.csproj" -NoNewWindow -PassThru

# Start Chat Service
Start-Process "dotnet" -ArgumentList "run --project src/Service/ChatService/ConnectHub.Chat.API/ConnectHub.Chat.API.csproj" -NoNewWindow -PassThru

# Start Notification Service
Start-Process "dotnet" -ArgumentList "run --project src/Service/NotificationService/ConnectHub.Notification.API/ConnectHub.Notification.API.csproj" -NoNewWindow -PassThru

Write-Host "Starting Frontend App..." -ForegroundColor Cyan

# Start React Frontend
Set-Location .\ConnectHub-Frontend
Start-Process "npm" -ArgumentList "run dev" -NoNewWindow -PassThru

Write-Host "All services started! You can view the app at http://localhost:5173 (or http://localhost:5174)" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop all services." -ForegroundColor Yellow
