#!/bin/bash
# macOS 完成脚本 - 在 Mac 上设置执行权限
# 使用方法：chmod +x finalize-macos-app.sh && ./finalize-macos-app.sh

echo "========================================"
echo "  完成 macOS .app 应用配置"
echo "========================================"
echo ""

# 检查是否有 .app 文件
if [ ! -d "publish/AgentManagement-osx-x64.app" ] && [ ! -d "publish/AgentManagement-osx-arm64.app" ]; then
    echo "错误：找不到 .app 文件！请先在 Windows 上运行 create-macos-app.ps1"
    exit 1
fi

# 函数：设置 .app 权限
function fix-app-permissions {
    local arch=$1
    local appDir="publish/AgentManagement-$arch.app"
    local executable="$appDir/Contents/MacOS/AgentManagement.Avalonia"
    
    if [ -d "$appDir" ]; then
        echo "正在设置 $arch .app 权限..."
        
        # 设置可执行文件权限
        if [ -f "$executable" ]; then
            chmod +x "$executable"
            echo "✅ 已设置可执行权限：$executable"
        else
            echo "⚠️ 警告：找不到可执行文件 $executable"
        fi
        
        # 设置所有 dylib 可执行
        find "$appDir" -name "*.dylib" -type f -exec chmod +x {} \; 2>/dev/null
        
        # 设置所有文件可读
        chmod -R a+r "$appDir"
        
        echo "✅ $arch .app 权限设置完成！"
        echo ""
    fi
}

# 处理两个版本
fix-app-permissions "osx-x64"
fix-app-permissions "osx-arm64"

echo "========================================"
echo "  完成！"
echo ""
echo "现在您可以："
echo "1. 直接双击 .app 文件运行"
echo "2. 或者把 .app 复制到 /Applications 文件夹"
echo ""
echo ".app 文件位置："
if [ -d "publish/AgentManagement-osx-x64.app" ]; then
    echo "   - publish/AgentManagement-osx-x64.app (Intel)"
fi
if [ -d "publish/AgentManagement-osx-arm64.app" ]; then
    echo "   - publish/AgentManagement-osx-arm64.app (Apple Silicon)"
fi
echo "========================================"
