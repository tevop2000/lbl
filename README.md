# AgentManagement.Avalonia - 跨平台版本

## 📖 项目说明

这是原WinFormsApp1项目的Avalonia跨平台版本，支持Windows、macOS和Linux。

## ✨ 已完成的功能

### 1. CVP分析页面
- ✅ 代理商列表加载（从API）
- ✅ 月份选择
- ✅ 模板下载（带认证Token）
- ✅ Excel导入（文件选择 + 选项对话框）
- ✅ 产品配置面板
- ✅ 产品编辑对话框
  - 添加/删除产品
  - 占比总和实时校验
  - 数据表格编辑

### 2. 核心架构
- ✅ MVVM模式完整实现
- ✅ 跨平台文件对话框服务
- ✅ 共享代码复用（Models、Services、Utils、Data）
- ✅ 导航系统

## 🚀 运行应用

```bash
cd e:\project\AgentManagement.Avalonia
dotnet run
```

## 📦 编译项目

```bash
cd e:\project\AgentManagement.Avalonia
dotnet build
```

## 🔧 发布多平台版本

### Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

### macOS (Intel)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true
```

### macOS (Apple Silicon)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true
```

### Linux
```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

## 📁 项目结构

```
AgentManagement.Avalonia/
├── Views/                  # 视图层
│   ├── MainWindow.axaml           # 主窗口
│   ├── CVPAnalysisView.axaml      # CVP分析页面
│   ├── AgentProductEditDialog.axaml  # 产品编辑对话框
│   └── ImportOptionsDialog.axaml     # 导入选项对话框
├── ViewModels/             # 视图模型层
│   ├── ViewModelBase.cs
│   ├── MainWindowViewModel.cs
│   ├── CVPAnalysisViewModel.cs
│   ├── CVPConfigViewModel.cs
│   ├── AgentProductEditViewModel.cs
│   └── ImportOptionsViewModel.cs
├── Controls/               # 自定义控件
│   └── CVPConfigPanel.axaml
├── Services/               # 服务层
│   └── FileService.cs      # 跨平台文件服务
├── Models/                 # 数据模型（链接自原项目）
├── Services/               # 业务服务（链接自原项目）
├── Utils/                  # 工具类（链接自原项目）
└── Data/                   # 数据访问（链接自原项目）
```

## 🎯 功能使用指南

### 模板下载
1. 点击顶部筛选区域的"📄 模板"按钮
2. 选择保存位置
3. 模板将自动下载并保存

### Excel导入
1. 点击"📥 导入"按钮
2. 选择Excel文件
3. 在弹出的对话框中选择：
   - 年份（近5年）
   - 是否覆盖已有数据
4. 点击"确定"开始导入

### 产品配置
1. 在产品配置区域点击"✏️ 编辑"按钮
2. 在弹出的对话框中：
   - 点击"➕ 添加产品"添加新产品
   - 点击"✕"删除产品
   - 编辑产品的各项参数
3. 注意占比总和必须为100%
4. 点击"💾 保存"保存配置

## 🔍 技术栈

- **UI框架**: Avalonia 11.2.1
- **MVVM框架**: CommunityToolkit.Mvvm 8.4.1
- **JSON处理**: Newtonsoft.Json 13.0.3
- **Excel处理**: NPOI 2.6.0
- **数据库**: SQLite (Entity Framework Core 8.0.0)
- **目标框架**: .NET 8.0

## ⚠️ 注意事项

1. **首次运行**: 需要确保网络连接正常，以便调用API
2. **认证**: 系统会自动使用已登录用户的Token进行API认证
3. **文件权限**: 确保有读写文件的权限
4. **跨平台**: 文件路径在不同平台上格式不同，系统会自动处理

## 🐛 已知问题

- 图表功能尚未集成（计划使用ScottPlot.Avalonia）
- 登录页面尚未迁移
- 部分高级功能页面待迁移

## 📝 开发计划

- [ ] 集成图表功能（ScottPlot.Avalonia）
- [ ] 迁移登录页面
- [ ] 迁移其他业务页面
- [ ] 完善错误处理和用户提示
- [ ] 添加单元测试
- [ ] 性能优化

## 🤝 贡献

欢迎提交Issue和Pull Request！

## 📄 许可证

与原项目保持一致
