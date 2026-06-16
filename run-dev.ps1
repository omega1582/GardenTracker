# PowerShell script to run the dev environment from Windows

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Configuration
$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Starting GardenTracker Dev Environment" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Start Backend API
Write-Host "-> Starting Backend API (.NET 10)..." -ForegroundColor Yellow
$backendProcess = Start-Process dotnet -ArgumentList "run --project GardenTracker.Api --launch-profile http" -PassThru -NoNewWindow

# Start Frontend Web (Vite)
Write-Host "-> Starting Frontend Web (Vite)..." -ForegroundColor Yellow
Set-Location GardenTracker.Web
$frontendProcess = Start-Process npm -ArgumentList "run dev" -PassThru -NoNewWindow

# Monitor and handle Ctrl+C to clean up processes
try {
    Write-Host "Dev servers running. Press Ctrl+C to stop." -ForegroundColor Green
    while ($true) {
        Start-Sleep -Seconds 1
    }
}
finally {
    Write-Host "`nStopping dev servers..." -ForegroundColor Red
    Stop-Process -Id $backendProcess.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $frontendProcess.Id -Force -ErrorAction SilentlyContinue
    Write-Host "Stopped." -ForegroundColor Red
}
