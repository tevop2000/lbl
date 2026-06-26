@echo off
cd /d "%~dp0"
echo Publishing...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o publish\win-x64
echo.
echo Done! Press any key to open folder...
pause >nul
explorer publish\win-x64
