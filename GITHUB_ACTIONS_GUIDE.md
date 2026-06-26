# GitHub Actions 免费打包 macOS 指南

## 🎉 这是最推荐的方式！100% 免费

---

## 📋 完整步骤

### 第一步：创建 GitHub 仓库

1. 访问 https://github.com/new
2. 创建一个新仓库（Public 或 Private 都可以）
3. 不需要初始化 README、.gitignore 等

### 第二步：初始化 Git 并推送代码

在项目目录打开 PowerShell 或 CMD，运行：

```powershell
# 1. 初始化 Git（如果还没有的话）
git init

# 2. 添加所有文件
git add .

# 3. 提交
git commit -m "Initial commit"

# 4. 关联到您的 GitHub 仓库（替换为您的仓库地址）
git remote add origin https://github.com/您的用户名/您的仓库名.git

# 5. 推送到 GitHub
git branch -M main
git push -u origin main
```

### 第三步：手动触发第一次打包

1. 访问您的 GitHub 仓库
2. 点击顶部的 **Actions** 标签
3. 在左侧选择 **Build macOS Package**
4. 点击右侧的 **Run workflow** 按钮
5. 点击绿色的 **Run workflow** 确认

### 第四步：下载打包文件

1. 等待几分钟，直到 workflow 显示绿色的 ✅
2. 点击该 workflow 进入详情页
3. 滚动到底部，找到 **Artifacts** 区域
4. 点击 **macos-packages** 下载 ZIP 文件
5. 解压后就能找到 `.app` 文件了！

---

## 🚀 自动打包（可选）

以后每次您推送代码到 `main` 或 `master` 分支，GitHub Actions 会自动打包！

---

## 📦 下载后怎么用

1. 解压下载的 ZIP 文件
2. 您会看到：
   - `AgentManagement-osx-x64.app` - Intel Mac 用
   - `AgentManagement-osx-arm64.app` - Apple Silicon (M1/M2/M3) Mac 用
3. 双击对应的 .app 文件就能运行！

---

## 💡 GitHub Actions 免费额度

- **公共仓库**：无限使用，完全免费
- **私有仓库**：每月 2000 分钟免费额度（macOS 每分钟消耗 10 倍，即每月 200 分钟 macOS 时间）
- 对于这个项目，每次打包大约 3-5 分钟，完全够用！

---

## 🔍 文件说明

| 文件 | 用途 |
|------|------|
| `.github/workflows/build-macos.yml` | GitHub Actions 配置文件 |
| `GITHUB_ACTIONS_GUIDE.md` | 本文档 |

---

## ❓ 常见问题

**Q: 打包失败怎么办？**
A: 在 Actions 页面点击失败的 workflow，查看日志找出问题。

**Q: 想修改应用名称或版本号？**
A: 编辑 `.github/workflows/build-macos.yml` 文件中的 `APP_NAME` 和 `VERSION` 变量。

**Q: 能同时打包 Windows 吗？**
A: 可以！需要的话告诉我，我帮您添加 Windows 打包步骤。

---

## 🎊 总结

就是这么简单！
1. 推代码到 GitHub
2. Actions 自动打包
3. 下载 .app 文件
