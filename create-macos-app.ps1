# Windows PowerShell 脚本 - 在 Windows 上预先创建 .app 结构
# 使用方法：.\create-macos-app.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Create macOS .app Structure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if publish files exist
if (-not (Test-Path "publish\osx-x64") -or -not (Test-Path "publish\osx-arm64")) {
    Write-Host "Error: Publish files not found! Please run .\publish-macos.ps1 first" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

$appName = "AgentManagement"
$version = "1.0.0"

# Function: Create .app bundle
function Create-App-Bundle {
    param(
        [string]$arch
    )
    
    $appDir = "publish\$appName-$arch.app"
    $contentsDir = "$appDir\Contents"
    $macosDir = "$contentsDir\MacOS"
    $resourcesDir = "$contentsDir\Resources"
    
    Write-Host "Creating $arch .app structure..." -ForegroundColor Yellow
    
    # Delete old if exists
    if (Test-Path $appDir) {
        Remove-Item $appDir -Recurse -Force
    }
    
    # Create directories
    New-Item -ItemType Directory -Path $macosDir -Force | Out-Null
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
    
    # Create Info.plist content
    $plistLines = @(
        '<?xml version="1.0" encoding="UTF-8"?>',
        '<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">',
        '<plist version="1.0">',
        '<dict>',
        '    <key>CFBundleExecutable</key>',
        '    <string>AgentManagement.Avalonia</string>',
        '    <key>CFBundleIdentifier</key>',
        '    <string>com.yourcompany.agentmanagement</string>',
        '    <key>CFBundleName</key>',
        '    <string>' + $appName + '</string>',
        '    <key>CFBundlePackageType</key>',
        '    <string>APPL</string>',
        '    <key>CFBundleShortVersionString</key>',
        '    <string>' + $version + '</string>',
        '    <key>CFBundleVersion</key>',
        '    <string>' + $version + '</string>',
        '    <key>NSHighResolutionCapable</key>',
        '    <true/>',
        '    <key>LSMinimumSystemVersion</key>',
        '    <string>11.0</string>',
        '</dict>',
        '</plist>'
    )
    
    # Write Info.plist
    $plistPath = "$contentsDir\Info.plist"
    $plistLines | Set-Content -Path $plistPath -Encoding UTF8
    
    # Copy all files to MacOS directory
    Write-Host "Copying files..." -ForegroundColor Yellow
    Copy-Item -Path "publish\$arch\*" -Destination $macosDir -Recurse -Force
    
    Write-Host "OK: $arch .app structure created: $appDir" -ForegroundColor Green
}

# Create both versions
Create-App-Bundle "osx-x64"
Create-App-Bundle "osx-arm64"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Done!" -ForegroundColor Green
Write-Host ""
Write-Host "Now you just need to:" -ForegroundColor Cyan
Write-Host "1. Copy entire project folder to Mac" -ForegroundColor White
Write-Host "2. On Mac, open Terminal and run:" -ForegroundColor White
Write-Host "   cd <project-directory>" -ForegroundColor Gray
Write-Host "   chmod +x finalize-macos-app.sh" -ForegroundColor Gray
Write-Host "   ./finalize-macos-app.sh" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Then double-click the .app file to run!" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"
