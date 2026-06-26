# 发布脚本 - 用于发布 Windows 和 macOS 版本
# 使用方法: .\build-publish.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "AgentManagement.Avalonia 多平台发布脚本" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# 设置输出目录
$publishRoot = ".\publish"

# 清理旧的发布文件
if (Test-Path $publishRoot) {
    Write-Host "清理旧的发布文件夹..." -ForegroundColor Yellow
    Remove-Item $publishRoot -Recurse -Force
}

Write-Host "开始发布 Windows x64 版本..." -ForegroundColor Green
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o "$publishRoot\win-x64"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Windows 发布失败！" -ForegroundColor Red
    exit 1
}

Write-Host "开始发布 macOS x64 版本..." -ForegroundColor Green
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishTrimmed=false -o "$publishRoot\osx-x64"
if ($LASTEXITCODE -ne 0) {
    Write-Host "macOS x64 发布失败！" -ForegroundColor Red
    exit 1
}

Write-Host "开始发布 macOS ARM64 版本..." -ForegroundColor Green
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishTrimmed=false -o "$publishRoot\osx-arm64"
if ($LASTEXITCODE -ne 0) {
    Write-Host "macOS ARM64 发布失败！" -ForegroundColor Red
    exit 1
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "发布完成！" -ForegroundColor Green
Write-Host "发布位置: $publishRoot" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
