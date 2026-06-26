using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = "超威数智营";

        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private LoginViewModel? _loginViewModel;

        [ObservableProperty]
        private Utils.UserInfo _currentUser = Utils.UserInfo.Instance;

        [ObservableProperty]
        private int _unreadMessageCount = 0;

        /// <summary>
        /// 是否为业务经理角色
        /// </summary>
        private bool _isAgentRole = false;

        // 消息刷新定时器
        private Timer? _messageRefreshTimer;
        // 刷新间隔（毫秒），默认15秒
        private const int RefreshInterval = 15000;

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new();

        public ICommand NavigateToMenuCommand { get; }

        public MainWindowViewModel()
        {
            NavigateToMenuCommand = new RelayCommand<MenuItemViewModel>(NavigateToMenu);
                      
            // 创建 LoginViewModel 实例
            LoginViewModel = new LoginViewModel();
            
            // 检查是否已经登录
            CheckLoginStatus();
            
            if (IsLoggedIn)
            {
                // 已登录，加载菜单和主界面
                InitializeStaticMenu();
                //_ = LoadDynamicMenusAsync();
                
                // 默认显示CRM门户
                NavigateToMenu(new MenuItemViewModel 
                { 
                    Title = "🔗 CRM门户", 
                    ViewType = typeof(Views.CrmPortalView) 
                });

                // 启动消息刷新定时器
                StartMessageRefreshTimer();
            }
            else
            {
                // 未登录，不加载菜单
                Logger.Info("用户未登录，显示登录界面");
            }
        }
        
        /// <summary>
        /// 检查登录状态
        /// </summary>
        private void CheckLoginStatus()
        {
            // 检查是否已登录
            IsLoggedIn = Utils.UserInfo.Instance.IsLoggedIn;
            
            if (IsLoggedIn)
            {
                Logger.Info("检测到用户已登录，自动进入主界面");
            }
        }
        
        /// <summary>
        /// 登录成功回调（由 LoginViewModel 调用）
        /// </summary>
        public void OnLoginSuccess()
        {
            Logger.Info("登录成功，切换到主界面...");
            IsLoggedIn = true;
            
            // 加载菜单
            InitializeStaticMenu();
            // 先获取用户信息
            _ = LoadUserInfoAsync();
            
            //_ = LoadDynamicMenusAsync();
            
            // 触发全局登录事件，通知所有页面重新初始化
            Services.AuthService.OnUserLoggedIn();
            
            // 默认显示CRM门户
            NavigateToMenu(new MenuItemViewModel 
            { 
                Title = "🔗 CRM门户", 
                ViewType = typeof(Views.CrmPortalView) 
            });

            // 启动消息刷新定时器，并立即刷新一次
            StartMessageRefreshTimer();
            _ = RefreshUnreadMessageCountAsync();
        }

        /// <summary>
        /// 加载用户信息
        /// </summary>
        private async Task<UserInfoResult?> LoadUserInfoAsync()
        {
            try
            {
                Logger.Info("开始加载用户信息...");
                var userInfoResult = await AuthService.GetUserInfoAsync();
                
                if (userInfoResult.Success)
                {
                    Logger.Success($"用户名称: {userInfoResult.NickName}");
                    Logger.Info($"是否为业务经理: {userInfoResult.IsAgentRole}");
                    // 保存是否是业务经理的状态
                    _isAgentRole = userInfoResult.IsAgentRole;                  
                    // 加载菜单
                    LoadLblMenus();
                    return userInfoResult;
                }
                else
                {
                    Logger.Warning($"用户信息加载失败: {userInfoResult.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载用户信息异常: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 初始化静态菜单（仅保留基础框架菜单）
        /// </summary>
        private void InitializeStaticMenu()
        {
            // 硬编码的基础菜单
            MenuItems.Add(new MenuItemViewModel { Title = "🔗 CRM门户", ViewType = typeof(Views.CrmPortalView) });            
        }

        /// <summary>
        /// 加载量本利菜单（根据用户角色）
        /// </summary>
        private void LoadLblMenus()
        {
            Logger.Info($"开始加载量本利菜单，用户角色: {(_isAgentRole ? "业务经理" : "非业务经理")}");

            // 创建量本利一级菜单
            var lblMenu = new MenuItemViewModel
            {
                Title = "量本利",
                IsExpanded = false
            };

            // 添加子菜单
            lblMenu.Children.Add(new MenuItemViewModel 
            { 
                Title = "预测模块", 
                ViewType = typeof(Views.rate.yuce.index) 
            });

            // 非业务经理显示督办中心
            if (!_isAgentRole)
            {
                lblMenu.Children.Add(new MenuItemViewModel 
                { 
                    Title = "督办中心", 
                    ViewType = typeof(Views.rate.supervise.index) 
                });
            }

            lblMenu.Children.Add(new MenuItemViewModel 
            { 
                Title = "年度预测", 
                ViewType = typeof(Views.rate.detailyear.index) 
            });

            lblMenu.Children.Add(new MenuItemViewModel 
            { 
                Title = "分析模块", 
                ViewType = typeof(Views.rate.enddetail.index) 
            });

            lblMenu.Children.Add(new MenuItemViewModel 
            { 
                Title = "市占率", 
                ViewType = typeof(Views.rate.marketfact.index) 
            });

            // 添加到菜单列表
            MenuItems.Add(lblMenu);

            Logger.Success($"量本利菜单加载完成，共 {lblMenu.Children.Count} 个子菜单");
        }
        
        /// <summary>
        /// 将 MenuRoute 转换为 MenuItemViewModel（递归处理子菜单）
        /// </summary>
        private MenuItemViewModel? ConvertRouteToMenuItem(MenuRoute route)
        {
            // 跳过隐藏的菜单
            if (route.Hidden)
                return null;
            
            string title = route.Meta?.Title ?? route.Name;
            
            // 创建菜单项（不使用图标）
            var menuItem = new MenuItemViewModel
            {
                Title = title,
                RoutePath = route.Path,
                Component = route.Component,
                IsExpanded = false // 默认不展开
            };
            
            // 递归处理子菜单
            if (route.Children != null && route.Children.Count > 0)
            {
                foreach (var child in route.Children)
                {
                    var childItem = ConvertRouteToMenuItem(child);
                    if (childItem != null)
                    {
                        menuItem.Children.Add(childItem);
                    }
                }
            }
            else
            {
                // 没有子菜单，设置 ViewType（根据 Component 或 Path 映射到具体的 View）
                menuItem.ViewType = GetComponentFromRoute(route);
            }
            
            return menuItem;
        }
        
        /// <summary>
        /// 根据路由信息获取对应的 View 类型
        /// </summary>
        private Type? GetComponentFromRoute(MenuRoute route)
        {
            // 根据 Component 路径自动查找 View
            string component = route.Component?.ToLower() ?? "";
            
            Logger.Debug($"检查路由映射 - Component: {component}");
            
            // 通用规则：根据 Component 路径自动查找 View
            // 例如: "rate/yuce/index" -> "AgentManagement.Avalonia.Views.rate.yuce.index"
            //       "rate/marketfact/index" -> "AgentManagement.Avalonia.Views.rate.marketfact.index"
            if (!string.IsNullOrEmpty(component) && !component.Equals("layout", StringComparison.OrdinalIgnoreCase))
            {
                var viewTypeName = $"AgentManagement.Avalonia.Views.{component.Replace("/", ".")}";
                var viewType = Type.GetType(viewTypeName);
                
                if (viewType != null)
                {
                    Logger.Info($"✓ 自动匹配到 View: {viewType.FullName}");
                    return viewType;
                }
                else
                {
                    Logger.Warning($"未找到 View 类型: {viewTypeName}");
                }
            }
            
            // 其他页面返回 null
            return null;
        }

        public void NavigateToMenu(MenuItemViewModel menuItem)
        {
            try
            {
                // 如果有子菜单，不执行导航（TreeView会自动展开/折叠）
                if (menuItem.HasChildren)
                {
                    Logger.Info($"菜单 '{menuItem.Title}' 有子菜单，点击展开/折叠");
                    return;
                }
                
                // 没有子菜单，导航到对应页面
                var viewType = menuItem.ViewType;
                
                if (viewType == null)
                {
                    Logger.Warning($"菜单 '{menuItem.Title}' 没有关联的视图类型");
                    return;
                }
                
                Logger.Info($"导航到: {menuItem.Title}");
                
                // 根据 ViewType 创建对应的 View
                var view = Activator.CreateInstance(viewType) as global::Avalonia.Controls.Control;
                
                // 为 View 设置对应的 ViewModel（通过命名约定自动匹配）
                // 例如: AgentManagement.Avalonia.Views.CVPAnalysisView 
                //      -> AgentManagement.Avalonia.ViewModels.CVPAnalysisViewModel
                // 例如: AgentManagement.Avalonia.Views.rate.yuce.index
                //      -> AgentManagement.Avalonia.ViewModels.rate.yuce.indexViewModel
                var viewModelTypeName = viewType.FullName;
                if (viewModelTypeName != null)
                {
                    // 先替换命名空间：Views -> ViewModels
                    viewModelTypeName = viewModelTypeName.Replace(".Views.", ".ViewModels.");
                    
                    // 再处理类名：
                    // - 如果以 "View" 结尾：XXXView -> XXXViewModel
                    // - 否则：直接添加 "ViewModel" 后缀
                    if (viewModelTypeName.EndsWith("View"))
                    {
                        viewModelTypeName = viewModelTypeName.Substring(0, viewModelTypeName.Length - 4) + "ViewModel";
                    }
                    else
                    {
                        viewModelTypeName = viewModelTypeName + "ViewModel";
                    }
                }
                    
                Logger.Debug($"View 类型: {viewType.FullName}");
                Logger.Debug($"尝试查找 ViewModel: {viewModelTypeName}");
                
                if (viewModelTypeName != null)
                {
                    var viewModelType = Type.GetType(viewModelTypeName);
                    if (viewModelType != null)
                    {
                        var viewModel = Activator.CreateInstance(viewModelType);
                        view!.DataContext = viewModel;
                        Logger.Success($"已为 {viewType.Name} 设置 DataContext: {viewModelType.Name}");
                    }
                    else
                    {
                        Logger.Warning($"未找到 ViewModel 类型: {viewModelTypeName}");
                    }
                }
                
                CurrentView = view;
                
                Logger.Success($"已切换到: {menuItem.Title}");
            }
            catch (Exception ex)
            {
                Logger.Error($"导航失败: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            try
            {
                Logger.Info("用户退出登录");
                
                // 停止消息刷新定时器
                StopMessageRefreshTimer();
                
                // 清除用户信息
                Utils.UserInfo.Instance.Clear();
                
                // 切换回登录界面
                IsLoggedIn = false;
                CurrentView = null;
                MenuItems.Clear();
                
                // 重置未读消息数
                UnreadMessageCount = 0;
                
                Logger.Info("已退出登录，返回登录界面");
            }
            catch (Exception ex)
            {
                Logger.Error($"退出登录异常: {ex.Message}", ex);
            }
            
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ShowUnreadMessagesAsync()
        {
            try
            {
                var mainWindow = App.MainWindow;
                if (mainWindow == null)
                {
                    Logger.Warning("无法获取主窗口，无法显示未读消息对话框");
                    return;
                }

                var dialog = new Views.UnreadMessagesDialog();
                var dialogViewModel = new UnreadMessagesDialogViewModel();
                dialog.DataContext = dialogViewModel;

                // 监听消息已读事件，刷新未读数量
                dialogViewModel.OnMessageRead += async () =>
                {
                    await RefreshUnreadMessageCountAsync();
                };

                // 先加载消息
                await dialogViewModel.LoadMessagesAsync();

                // 显示对话框
                await dialog.ShowDialog(mainWindow);
            }
            catch (Exception ex)
            {
                Logger.Error($"显示未读消息对话框异常: {ex.Message}", ex);
            }
        }

        #region 消息刷新相关

        /// <summary>
        /// 启动消息刷新定时器
        /// </summary>
        private void StartMessageRefreshTimer()
        {
            // 如果定时器已经存在，先停止
            StopMessageRefreshTimer();

            // 创建新的定时器
            _messageRefreshTimer = new Timer(
                callback: async (state) => await RefreshUnreadMessageCountAsync(),
                state: null,
                dueTime: RefreshInterval,
                period: RefreshInterval);

            Logger.Debug("消息刷新定时器已启动");
        }

        /// <summary>
        /// 停止消息刷新定时器
        /// </summary>
        private void StopMessageRefreshTimer()
        {
            if (_messageRefreshTimer != null)
            {
                _messageRefreshTimer.Dispose();
                _messageRefreshTimer = null;
                Logger.Debug("消息刷新定时器已停止");
            }
        }

        /// <summary>
        /// 刷新未读消息数
        /// </summary>
        private async Task RefreshUnreadMessageCountAsync()
        {
            try
            {
                var result = await MessageService.GetMessageStatsAsync();
                if (result.Success && result.Data != null)
                {
                    UnreadMessageCount = result.Data.UnreadCount;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"刷新未读消息异常: {ex.Message}");
            }
        }

        #endregion
    }

    public partial class MenuItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = string.Empty;
        
        public Type? ViewType { get; set; }
        public string? RoutePath { get; set; }
        public string? Component { get; set; }
        
        // 子菜单
        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<MenuItemViewModel> _children = new();
        
        // 是否展开（用于TreeView/Expander）
        [ObservableProperty]
        private bool _isExpanded;
        
        // 是否有子菜单
        public bool HasChildren => Children != null && Children.Count > 0;
    }
}
