using AgentManagement.Avalonia.ViewModels.rate.supervise;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Services;

namespace AgentManagement.Avalonia.Views.rate.supervise
{
    public partial class index : UserControl
    {
        private Controls.DeptManagerAgentSelector? _selectorControl;
        private indexViewModel? ViewModel => DataContext as indexViewModel;
        
        public index()
        {
            InitializeComponent();
            
            // 获取选择器控件引用
            _selectorControl = this.FindControl<Controls.DeptManagerAgentSelector>("SelectorControl");
            
            // 页面加载时初始化控件
            Loaded += async (s, e) => await InitializeSelectorAsync();
            
            // 监听用户登录事件，退出登录后重新登录时重新初始化
            Services.AuthService.UserLoggedIn += async () => await InitializeSelectorAsync();
        }

        /// <summary>
        /// 初始化级联选择器控件
        /// </summary>
        private async Task InitializeSelectorAsync()
        {
            try
            {
                // 获取当前用户信息
                var userInfoResult = await AuthService.GetUserInfoAsync();
                
                if (userInfoResult.Success && _selectorControl != null)
                {
                    await _selectorControl.InitializeAsync(userInfoResult);
                    
                    // 设置回调
                    _selectorControl.OnAgentSelectedCallback = async (agent, manager, region, channel, warZone) =>
                    {
                        if (ViewModel != null)
                        {
                            await ViewModel.OnAgentChangedAsync(agent, manager, region, channel, warZone);
                        }
                    };
                    
                    // 初始化数据
                    if (ViewModel != null)
                    {
                        await ViewModel.InitializeDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化级联选择器失败: {ex.Message}");
            }
        }

        private void OnPredictionTabClicked(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is indexViewModel vm)
            {
                vm.IsPredictionTabSelected = true;
            }
        }

        private void OnImportTabClicked(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is indexViewModel vm)
            {
                vm.IsPredictionTabSelected = false;
            }
        }

        private void OnExpandClicked(object sender, PointerPressedEventArgs e)
        {
            if (sender is Control control && control.Tag is indexViewModel.SuperviseItem item)
            {
                if (!item.IsExpanded)
                {
                    item.IsExpanded = true;
                    item.LoadDetailMonthsFromApiCommand.Execute(null);
                }
                else
                {
                    item.IsExpanded = false;
                }
            }
        }

        private void OnAllFilterClicked(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is indexViewModel vm)
            {
                vm.SetFilterStatusCommand.Execute("全部");
            }
        }

        private void OnCompletedFilterClicked(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is indexViewModel vm)
            {
                vm.SetFilterStatusCommand.Execute("已完成");
            }
        }
    }
}