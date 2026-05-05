param(
    [Switch]$UseHttps = $false
)

# Define the services and their paths
$services = @(
    @{ Name = "Auth API"; Path = "src\Service\AuthService\ConnectHub.Auth.API" },
    @{ Name = "Chat API"; Path = "src\Service\ChatService\ConnectHub.Chat.API" },
    @{ Name = "Notification API"; Path = "src\Service\NotificationService\ConnectHub.Notification.API" },
    @{ Name = "Gateway"; Path = "src\Service\ConnectHub.Gateway" }
)

# Get the root directory
$rootDir = Get-Location

# Function to start a service
function Start-Service {
    param(
        [string]$ServiceName,
        [string]$ServicePath
    )
    
    $fullPath = Join-Path $rootDir $ServicePath
    
    Write-Host "Starting $ServiceName..." -ForegroundColor Cyan
    
    # Start the service in a separate process
    Start-Process -WorkingDirectory $fullPath -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru | Out-Null
}

# Display startup message
Write-Host "╔════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║   Starting ConnectHub Backend Services ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Start all services
foreach ($service in $services) {
    Start-Service -ServiceName $service.Name -ServicePath $service.Path
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "✓ All services started!" -ForegroundColor Green
Write-Host ""
Write-Host "Services running:" -ForegroundColor Yellow
Write-Host "  • Auth API:         https://localhost:7001" -ForegroundColor White
Write-Host "  • Chat API:         https://localhost:7002" -ForegroundColor White
Write-Host "  • Notification API: https://localhost:7003" -ForegroundColor White
Write-Host "  • Gateway:          https://localhost:7000" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop services" -ForegroundColor Yellow
