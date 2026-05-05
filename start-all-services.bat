@echo off
setlocal enabledelayedexpansion

echo.
echo ========================================
echo    Starting ConnectHub Backend Services
echo ========================================
echo.

REM Start all services in separate windows
echo Starting Auth API...
start "Auth API" cmd /k "cd src\Service\AuthService\ConnectHub.Auth.API && dotnet run"

timeout /t 1 /nobreak

echo Starting Chat API...
start "Chat API" cmd /k "cd src\Service\ChatService\ConnectHub.Chat.API && dotnet run"

timeout /t 1 /nobreak

echo Starting Notification API...
start "Notification API" cmd /k "cd src\Service\NotificationService\ConnectHub.Notification.API && dotnet run"

timeout /t 1 /nobreak

echo Starting Gateway...
start "Gateway" cmd /k "cd src\Service\ConnectHub.Gateway && dotnet run"

echo.
echo ✓ All services started!
echo.
echo Services running at:
echo   • Auth API:         https://localhost:7001
echo   • Chat API:         https://localhost:7002
echo   • Notification API: https://localhost:7003
echo   • Gateway:          https://localhost:7000
echo.
pause
