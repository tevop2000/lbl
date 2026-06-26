# 跨平台打包指南

## Windows 环境下做 macOS 包

### 第一步：在 Windows 上发布 macOS 二进制

运行 `publish-macos.ps1` 或手动执行：

```powershell
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishTrimmed=false -o .\publish\osx-x64
```

这会生成 macOS 可执行文件到 `publish\osx-x64` 文件夹。

---

### 第二步：在 Mac 上完成打包

把整个 `publish\osx-x64` 文件夹复制到 Mac 上，然后在 Mac 上执行：

```bash
# 1. 创建 .app 结构
mkdir -p 数智营.app/Contents/MacOS
mkdir -p 数智营.app/Contents/Resources

# 2. 复制可执行文件
cp publish/osx-x64/AgentManagement.Avalonia 数智营.app/Contents/MacOS/
chmod +x 数智营.app/Contents/MacOS/AgentManagement.Avalonia

# 3. 创建 Info.plist
cat > 数智营.app/Contents/Info.plist << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>AgentManagement.Avalonia</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourcompany.agentmanagement</string>
    <key>CFBundleName</key>
    <string>数智营</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# 4. 可选：复制图标
# cp your-icon.icns 数智营.app/Contents/Resources/
```

---

## 推荐方案

| 平台 | 打包环境 | 推荐方式 |
|-----|---------|---------|
| **Windows** | Windows | 用 Inno Setup 做 .exe 安装包 |
| **macOS** | Mac | 用 Xcode 或第三方工具做 .dmg |

---

## 最简单的替代方案

如果没有 Mac 设备：

1. ✅ 在 Windows 上做 Windows 安装包（已配置好）
2. ✅ 在 Windows 上发布 macOS 文件，给技术用户直接使用
3. ❌ 不做 macOS 安装包，只提供可执行文件

---

## 总结

- **可以**：Windows 上生成 macOS 可执行文件
- **不可以**：Windows 上做 macOS 安装包和签名
- **最佳实践**：各自在各自平台上打包
