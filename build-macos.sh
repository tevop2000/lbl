#!/bin/bash
# macOS 打包脚本 - 需要在 macOS 上运行
# 使用方法: chmod +x build-macos.sh && ./build-macos.sh

echo "====================================="
echo "AgentManagement.Avalonia macOS 打包"
echo "====================================="

# 设置项目根目录
PROJECT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PUBLISH_DIR="$PROJECT_DIR/publish"
APP_NAME="AgentManagement"
VERSION="1.0.0"

# 清理旧文件
if [ -d "$PUBLISH_DIR" ]; then
    echo "清理旧的发布文件..."
    rm -rf "$PUBLISH_DIR"
fi

# 发布版本
echo "发布 macOS x64 版本..."
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishTrimmed=false -o "$PUBLISH_DIR/osx-x64"
if [ $? -ne 0 ]; then
    echo "macOS x64 发布失败!"
    exit 1
fi

echo "发布 macOS ARM64 版本..."
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishTrimmed=false -o "$PUBLISH_DIR/osx-arm64"
if [ $? -ne 0 ]; then
    echo "macOS ARM64 发布失败!"
    exit 1
fi

# 创建 .app 包
create_app_bundle() {
    local ARCH=$1
    local SOURCE_DIR="$PUBLISH_DIR/$ARCH"
    local APP_DIR="$PUBLISH_DIR/$APP_NAME-$ARCH.app"
    local CONTENTS_DIR="$APP_DIR/Contents"
    local MACOS_DIR="$CONTENTS_DIR/MacOS"
    local RESOURCES_DIR="$CONTENTS_DIR/Resources"

    echo "创建 $ARCH .app 包..."

    # 创建目录结构
    mkdir -p "$MACOS_DIR"
    mkdir -p "$RESOURCES_DIR"

    # 创建 Info.plist
    cat > "$CONTENTS_DIR/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>AgentManagement.Avalonia</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourcompany.agentmanagement</string>
    <key>CFBundleName</key>
    <string>AgentManagement</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
</dict>
</plist>
EOF

    # 复制可执行文件和资源
    cp -r "$SOURCE_DIR"/* "$MACOS_DIR/"

    # 复制图标（如果有）
    if [ -f "$PROJECT_DIR/Assets/avalonia-logo.ico" ]; then
        echo "注意: 需要转换 .ico 为 .icns 格式"
        # sips -s format png "$PROJECT_DIR/Assets/avalonia-logo.ico" --out "$RESOURCES_DIR/icon.png"
        # iconutil -c icns "$RESOURCES_DIR/icon.iconset" 2>/dev/null || true
    fi

    # 设置执行权限
    chmod +x "$MACOS_DIR/AgentManagement.Avalonia"

    echo "创建 $ARCH .app 包完成: $APP_DIR"
}

create_app_bundle "osx-x64"
create_app_bundle "osx-arm64"

# 创建 .dmg 安装包（可选）
echo ""
echo "====================================="
echo "发布完成！"
echo "如需创建 .dmg 安装包，需要使用额外工具，如:"
echo "https://github.com/create-dmg/create-dmg"
echo "或使用 macOS 磁盘工具手动创建"
echo "====================================="
