# AgentManagement.Avalonia 多平台打包指南

本文档介绍如何为 AgentManagement.Avalonia 项目创建 Windows 和 macOS 安装包。

## 📋 前置条件

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- 对于 Windows: [Inno Setup](https://jrsoftware.org/isdl.php)
- 对于 macOS: 一台 Mac 电脑

---

## 🪟 Windows 平台打包

### 步骤 1: 发布应用

在 Windows 上打开 PowerShell，运行以下命令：

```powershell
cd d:\work\1-xk\code\lbl
.\build-publish.ps1
```

这会在 `publish\win-x64` 文件夹中生成 Windows 自包含版本。

### 步骤 2: 创建安装包

1. 下载并安装 [Inno Setup](https://jrsoftware.org/isdl.php)
2. 右键点击 `setup-windows.iss` 文件，选择 "Compile"
3. 或者在 Inno Setup 中打开该文件并按 F9 编译
4. 编译完成后，会在项目根目录生成 `AgentManagement-Setup.exe`

### Windows 直接运行（不打包）

如果只需要一个可执行文件，可以使用以下命令：

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o .\publish\win-x64-single
```

---

## 🍎 macOS 平台打包

### 重要提示
**macOS 打包必须在 Mac 电脑上进行！**

### 步骤 1: 复制项目到 Mac

将整个项目文件夹复制到 Mac 电脑上。

### 步骤 2: 运行打包脚本

在 macOS 终端中运行：

```bash
cd /path/to/lbl
chmod +x build-macos.sh
./build-macos.sh
```

这会生成：
- `publish/osx-x64/`: Intel 芯片 (x64) 版本
- `publish/osx-arm64/`: Apple Silicon 芯片 (ARM64) 版本
- `publish/AgentManagement-osx-x64.app`: Intel .app 包
- `publish/AgentManagement-osx-arm64.app`: Apple Silicon .app 包

### 步骤 3: 创建 .dmg 安装包（可选）

方法一: 使用 create-dmg 工具

```bash
# 安装 create-dmg
brew install create-dmg

# 创建 dmg
create-dmg \
  --volname "AgentManagement" \
  --window-pos 200 120 \
  --window-size 600 300 \
  --icon-size 100 \
  --icon "AgentManagement-osx-arm64.app" 175 120 \
  --hide-extension "AgentManagement-osx-arm64.app" \
  --app-drop-link 425 120 \
  "AgentManagement-macOS-arm64.dmg" \
  "publish/"
```

方法二: 使用 macOS 磁盘工具
1. 打开 "磁盘工具"
2. File -> New Image -> Image from Folder
3. 选择 `publish` 文件夹
4. 设置为 "压缩" 格式

### macOS 公证和签名（生产环境）

如果需要分发应用，建议进行代码签名和公证：

```bash
# 签名（需要 Apple Developer 账号）
codesign --force --deep --sign "Developer ID Application: Your Name" AgentManagement.app

# 创建 .pkg 安装包
pkgbuild --root AgentManagement.app --install-location /Applications AgentManagement.pkg
```

---

## 📁 项目文件说明

| 文件 | 说明 |
|------|------|
| `build-publish.ps1` | Windows 下的发布脚本 |
| `build-macos.sh` | macOS 下的打包脚本 |
| `setup-windows.iss` | Inno Setup 安装脚本 |
| `PACKAGING.md` | 本文档 |

---

## 🔧 自定义配置

### 更改版本号
1. 修改 `setup-windows.iss` 中的 `AppVersion`
2. 修改 `build-macos.sh` 中的 `VERSION`
3. （可选）在 `AgentManagement.Avalonia.csproj` 中设置版本

### 更改应用名称
1. 修改 `setup-windows.iss` 中的 `AppName`
2. 修改 `build-macos.sh` 中的 `APP_NAME`
3. 修改 Info.plist 模板中的相关字段

### 添加图标
1. Windows: 替换 `Assets/avalonia-logo.ico`
2. macOS: 将图标转换为 .icns 格式，放置在 `Assets` 文件夹，然后在脚本中配置

---

## 📦 发布文件结构

```
publish/
├── win-x64/                  # Windows 可执行文件
│   └── AgentManagement.Avalonia.exe
├── osx-x64/                  # macOS Intel 可执行文件
├── osx-arm64/                # macOS Apple Silicon 可执行文件
├── AgentManagement-osx-x64.app    # macOS Intel .app
└── AgentManagement-osx-arm64.app  # macOS Apple Silicon .app
```

---

## 🚀 快速开始

### Windows 用户
1. 运行 `.\build-publish.ps1`
2. 使用 Inno Setup 编译 `setup-windows.iss`
3. 分发 `AgentManagement-Setup.exe`

### macOS 用户
1. 将项目复制到 Mac
2. 运行 `./build-macos.sh`
3. 分发 .app 或 .dmg 文件

---

## 📞 技术支持

如有问题，请查看：
- [Avalonia 文档](https://docs.avaloniaui.net/)
- [.NET 发布文档](https://docs.microsoft.com/en-us/dotnet/core/deploying/)
