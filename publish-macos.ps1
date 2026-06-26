# macOS Publish Script - Run on Windows to generate macOS executable
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Publishing macOS x64 Version" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Check dotnet SDK
Write-Host "[1/3] Checking .NET SDK..." -ForegroundColor Yellow
dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET SDK not found!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "OK: .NET SDK is available" -ForegroundColor Green
Write-Host ""

# Clean old publish
Write-Host "[2/3] Cleaning old publish..." -ForegroundColor Yellow
Remove-Item -Path .\publish\osx-x64 -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "OK: Cleaned up" -ForegroundColor Green
Write-Host ""

# Publish macOS x64 version
Write-Host "[3/3] Publishing macOS x64 version..." -ForegroundColor Yellow
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishTrimmed=false -o .\publish\osx-x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publish failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}
Write-Host "OK: Publish successful!" -ForegroundColor Green
Write-Host ""

Write-Host "=========================================" -ForegroundColor Green
Write-Host "  macOS x64 Publish Complete!" -ForegroundColor Green
Write-Host "  Location: .\publish\osx-x64" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Note:" -ForegroundColor Yellow
Write-Host "  - Copy to Mac computer" -ForegroundColor Gray
Write-Host "  - Create .app package on Mac" -ForegroundColor Gray
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"
