using System;
using Avalonia.Controls;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.Models;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace AgentManagement.Avalonia.Controls
{
    /// <summary>
    /// 部门-业务经理-代理商级联选择器控件
    /// </summary>
    public partial class DeptManagerAgentSelector : UserControl
    {
        private readonly DeptManagerAgentSelectorViewModel _viewModel;

        public DeptManagerAgentSelector()
        {
            InitializeComponent();
            _viewModel = new DeptManagerAgentSelectorViewModel();
            DataContext = _viewModel;
        }

        /// <summary>
        /// 当部门选择完成时触发
        /// </summary>
        private async void OnDeptSelectionCompleted(object? sender, EventArgs e)
        {
            await LoadManagersAfterDeptSelectionAsync();
        }

        /// <summary>
        /// 初始化选择器（必须在页面加载后调用）
        /// </summary>
        /// <param name="currentUser">当前登录用户信息</param>
        public async Task InitializeAsync(UserInfoResult currentUser)
        {
            await _viewModel.InitializeAsync(currentUser);
        }

        /// <summary>
        /// 当用户完成部门选择后调用此方法加载业务经理
        /// </summary>
        public async Task LoadManagersAfterDeptSelectionAsync()
        {
            await _viewModel.LoadManagersAfterDeptSelectionAsync();
        }

        /// <summary>
        /// 获取选中的部门（大区）
        /// </summary>
        public DeptInfo? SelectedDepartment => _viewModel.SelectedRegionManager;

        /// <summary>
        /// 获取选中的战区
        /// </summary>
        public DeptInfo? SelectedWarZone => _viewModel.SelectedWarZone;

        /// <summary>
        /// 获取选中的渠道部
        /// </summary>
        public DeptInfo? SelectedChannelDept => _viewModel.SelectedChannelDept;

        /// <summary>
        /// 获取选中的大区
        /// </summary>
        public DeptInfo? SelectedRegionManager => _viewModel.SelectedRegionManager;

        /// <summary>
        /// 获取选中的业务经理
        /// </summary>
        public AgentUser? SelectedManager => _viewModel.SelectedManager;

        /// <summary>
        /// 获取选中的代理商
        /// </summary>
        public AgentItem? SelectedAgent => _viewModel.SelectedAgent;

        /// <summary>
        /// 设置选中的部门
        /// </summary>
        public void SetSelectedDepartment(DeptInfo? department)
        {
            _viewModel.SelectedDepartment = department;
        }

        /// <summary>
        /// 设置选中的业务经理
        /// </summary>
        public void SetSelectedManager(AgentUser? manager)
        {
            _viewModel.SelectedManager = manager;
        }

        /// <summary>
        /// 设置选中的代理商
        /// </summary>
        public void SetSelectedAgent(AgentItem? agent)
        {
            _viewModel.SelectedAgent = agent;
        }

        /// <summary>
        /// 获取或设置代理商选择变化的回调
        /// </summary>
        public Action<AgentItem?, AgentUser?, DeptInfo?, DeptInfo?, DeptInfo?>? OnAgentSelectedCallback
        {
            get => _viewModel.OnAgentSelectedCallback;
            set => _viewModel.OnAgentSelectedCallback = value;
        }

        /// <summary>
        /// 获取 ViewModel 引用
        /// </summary>
        public DeptManagerAgentSelectorViewModel ViewModel => _viewModel;
    }
}
