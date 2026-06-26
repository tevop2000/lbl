# macOS 打包完整指南

## 🎉 最简单的方式（推荐！）

我们已经帮您完成了 90% 的工作！现在您只需要做很少的操作。

---

## 📋 完整步骤

### 在 Windows 上（已完成！）

我们已经为您完成了以下工作：
1. ✅ 发布了 macOS x64 和 arm64 版本
2. ✅ 创建了 .app 应用程序结构

### 在 Mac 上（您需要做的）

1. **把整个项目文件夹复制到 Mac**
   - 可以用 U 盘、网盘、局域网共享等方式

2. **在 Mac 上打开终端（Terminal）**

3. **进入项目目录**
   ```bash
   cd /path/to/your/project/folder
   ```

4. **运行完成脚本**
   ```bash
   chmod +x finalize-macos-app.sh
   ./finalize-macos-app.sh
   ```

5. **完成！** 现在您可以：
   - 直接双击 `publish/AgentManagement-osx-x64.app`（Intel Mac）
   - 直接双击 `publish/AgentManagement-osx-arm64.app`（Apple Silicon Mac）
   - 或者把 .app 复制到 `/Applications` 文件夹

---

## 🚀 如果需要重新打包

### 第一步：在 Windows 上重新发布

```powershell
# 1. 发布 macOS 版本
.\publish-macos.ps1

# 2. 创建 .app 结构
.\create-macos-app.ps1
```

### 第二步：在 Mac 上完成

```bash
cd /path/to/project
chmod +x finalize-macos-app.sh
./finalize-macos-app.sh
```

---

## 📁 文件说明

| 文件 | 用途 |
|------|------|
| `publish-macos.ps1` | Windows 上发布 macOS 可执行文件 |
| `create-macos-app.ps1` | Windows 上创建 .app 结构 |
| `finalize-macos-app.sh` | Mac 上设置执行权限 |
| `build-macos.sh` | 完整的 Mac 打包脚本（如果您想在 Mac 上全程操作） |

---

## 💡 其他方案

### 方案一：GitHub Actions（免费自动化）

如果您想完全自动化，可以用 GitHub Actions。需要的话告诉我，我帮您配置！

### 方案二：云 Mac 服务

- MacinCloud - https://www.macincloud.com/
- MacStadium - https://www.macstadium.com/

### 方案三：找朋友帮忙

把项目文件夹发给有 Mac 的朋友，让他们运行 `finalize-macos-app.sh`

---

## ❓ 常见问题

**Q: .app 文件在 Windows 上看起来像文件夹？**
A: 正常的！macOS 的 .app 本质上就是一个特殊的文件夹，只有在 Mac 上才会显示为应用程序图标。

**Q: 双击 .app 文件说"文件已损坏"？**
A: 在 Mac 终端运行：
```bash
xattr -cr /path/to/AgentManagement.app
```

**Q: 想创建 .dmg 安装包？**
A: 在 Mac 上：
```bash
# 安装 create-dmg
brew install create-dmg

# 创建 dmg
create-dmg --volname "AgentManagement" --window-pos 200 120 --window-size 600 300 --icon-size 100 --icon "AgentManagement-osx-arm64.app" 175 120 --hide-extension "AgentManagement-osx-arm64.app" --app-drop-link 425 120 "AgentManagement-macOS-arm64.dmg" "publish/"
```

---

## 🎊 总结

**最简单的方式**：
1. 把项目文件夹复制到 Mac
2. 在 Mac 上运行 `./finalize-macos-app.sh`
3. 双击 .app 运行！

就是这么简单！
