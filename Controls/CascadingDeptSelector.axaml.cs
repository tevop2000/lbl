using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Services;

namespace AgentManagement.Avalonia.Controls
{
    /// <summary>
    /// 级联部门选择器控件
    /// </summary>
    public partial class CascadingDeptSelector : UserControl
    {
        public static readonly StyledProperty<string> PlaceholderTextProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, string>(nameof(PlaceholderText), "请选择部门");

        public static readonly StyledProperty<DeptInfo?> SelectedWarZoneProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, DeptInfo?>(nameof(SelectedWarZone));

        public static readonly StyledProperty<DeptInfo?> SelectedChannelDeptProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, DeptInfo?>(nameof(SelectedChannelDept));

        public static readonly StyledProperty<DeptInfo?> SelectedRegionManagerProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, DeptInfo?>(nameof(SelectedRegionManager));

        public static readonly StyledProperty<ObservableCollection<DeptInfo>> WarZoneListProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, ObservableCollection<DeptInfo>>(nameof(WarZoneList));

        public static readonly StyledProperty<ObservableCollection<DeptInfo>> ChannelDeptListProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, ObservableCollection<DeptInfo>>(nameof(ChannelDeptList));

        public static readonly StyledProperty<ObservableCollection<DeptInfo>> RegionManagerListProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, ObservableCollection<DeptInfo>>(nameof(RegionManagerList));

        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<CascadingDeptSelector, bool>(nameof(IsReadOnly));

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        // 选择完成事件
        public event EventHandler? SelectionCompleted;

        private TextBox? _displayTextBlock;
        private Popup? _popup;
        private ListBox? _warZoneListBox;
        private ListBox? _channelDeptListBox;
        private ListBox? _regionManagerListBox;
        private Border? _triggerBorder;

        public string PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public DeptInfo? SelectedWarZone
        {
            get => GetValue(SelectedWarZoneProperty);
            set => SetValue(SelectedWarZoneProperty, value);
        }

        public DeptInfo? SelectedChannelDept
        {
            get => GetValue(SelectedChannelDeptProperty);
            set => SetValue(SelectedChannelDeptProperty, value);
        }

        public DeptInfo? SelectedRegionManager
        {
            get => GetValue(SelectedRegionManagerProperty);
            set => SetValue(SelectedRegionManagerProperty, value);
        }

        public ObservableCollection<DeptInfo> WarZoneList
        {
            get => GetValue(WarZoneListProperty);
            set => SetValue(WarZoneListProperty, value);
        }

        public ObservableCollection<DeptInfo> ChannelDeptList
        {
            get => GetValue(ChannelDeptListProperty);
            set => SetValue(ChannelDeptListProperty, value);
        }

        public ObservableCollection<DeptInfo> RegionManagerList
        {
            get => GetValue(RegionManagerListProperty);
            set => SetValue(RegionManagerListProperty, value);
        }

        public CascadingDeptSelector()
        {
            InitializeComponent();
            
            // 使用 Loaded 事件来确保控件已经完全加载
            this.Loaded += OnLoaded;
            
            // 监听用户登录事件，退出登录后重新登录时重新加载部门树
            Services.AuthService.UserLoggedIn += async () =>
            {
                System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: 收到登录事件，重新加载部门树...");
                System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: _triggerBorder is null: {_triggerBorder == null}, _popup is null: {_popup == null}");
                await LoadDeptTreeAsync();
            };
            
            // 自动加载部门树数据
            _ = LoadDeptTreeAsync();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: Loaded event fired");
            
            // 在 Loaded 事件中查找控件
            _displayTextBlock = this.FindControl<TextBox>("PART_DisplayText");
            _triggerBorder = this.FindControl<Border>("PART_TriggerBorder");
            _popup = this.FindControl<Popup>("PART_Popup");
            _warZoneListBox = this.FindControl<ListBox>("PART_WarZoneList");
            _channelDeptListBox = this.FindControl<ListBox>("PART_ChannelDeptList");
            _regionManagerListBox = this.FindControl<ListBox>("PART_RegionManagerList");

            System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector Loaded: DisplayText={_displayTextBlock != null}, Popup={_popup != null}");
            System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector Loaded: WarZoneList={_warZoneListBox != null}, ChannelDeptList={_channelDeptListBox != null}, RegionManagerList={_regionManagerListBox != null}");

            if (_triggerBorder != null)
            {
                if (IsReadOnly)
                {
                    _triggerBorder.Cursor = new Cursor(StandardCursorType.Arrow);  // 不显示手型光标
                }
                else
                {
                    _triggerBorder.PointerPressed += (s, e) => OpenPopup();
                }
                System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: TriggerBorder initialized");
            }
            
            if (_displayTextBlock != null)
            {
                // 初始状态显示 placeholder
                UpdateDisplayText();
                System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector Loaded: Initial text set to '{_displayTextBlock.Text}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: PART_DisplayText not found in Loaded event!");
            }

            if (_warZoneListBox != null)
            {
                // SelectedItem 绑定会自动触发 ViewModel 的 OnSelectedWarZoneChanged
            }

            if (_channelDeptListBox != null)
            {
                // SelectedItem 绑定会自动触发 ViewModel 的 OnSelectedChannelDeptChanged
            }

            if (_regionManagerListBox != null)
            {
                // SelectedItem 绑定会自动触发关闭弹窗
                _regionManagerListBox.SelectionChanged += (s, e) =>
                {
                    if (_popup != null)
                    {
                        _popup.IsOpen = false;
                    }
                };
            }
            
            // 监听 Popup 打开事件，注册窗口失焦处理
            if (_popup != null)
            {
                _popup.Opened += (s, e) =>
                {
                    // 当 Popup 打开时，订阅窗口的 Deactivated 事件
                    var window = TopLevel.GetTopLevel(this) as Window;
                    if (window != null)
                    {
                        void OnWindowDeactivated(object? sender2, EventArgs e2)
                        {
                            if (_popup != null && _popup.IsOpen)
                            {
                                _popup.IsOpen = false;
                            }
                            // 取消订阅，避免内存泄漏
                            window.Deactivated -= OnWindowDeactivated;
                        }
                        window.Deactivated += OnWindowDeactivated;
                    }
                };
                
                // 监听 Popup 关闭事件
                _popup.Closed += (s, e) =>
                {
                    // 触发选择完成事件
                    SelectionCompleted?.Invoke(this, EventArgs.Empty);
                };
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: OnApplyTemplate called (skip initialization)");
        }

        private void OpenPopup()
        {
            if (IsReadOnly) return;  // 如果是只读状态，不打开弹窗
            if (_popup != null)
            {
                _popup.IsOpen = true;
            }
        }

        private void UpdateDisplayText()
        {
            if (_displayTextBlock == null) return;

            var parts = new System.Collections.Generic.List<string>();
            
            if (SelectedWarZone != null)
                parts.Add(SelectedWarZone.DisplayName);
            
            if (SelectedChannelDept != null)
                parts.Add(SelectedChannelDept.DisplayName);
            
            if (SelectedRegionManager != null)
                parts.Add(SelectedRegionManager.DisplayName);

            _displayTextBlock.Text = parts.Count > 0 ? string.Join(" > ", parts) : PlaceholderText;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectedWarZoneProperty)
            {
                UpdateDisplayText();
                
                // 当战区改变时，自动加载渠道部列表
                if (change.NewValue is DeptInfo warZone && warZone != null)
                {
                    Console.WriteLine($"\n[级联选择器] 选择战区: {warZone.DisplayName} (ID: {warZone.Id})");
                    
                    var channelDepts = new ObservableCollection<DeptInfo>();
                    foreach (var channel in warZone.Children)
                    {
                        channelDepts.Add(channel);
                    }
                    ChannelDeptList = channelDepts;
                    
                    // 清空下级选择
                    SelectedChannelDept = null;
                    SelectedRegionManager = null;
                    RegionManagerList = new ObservableCollection<DeptInfo>();
                }
                else
                {
                    Console.WriteLine("\n[级联选择器] 清除战区选择");
                }
            }
            else if (change.Property == SelectedChannelDeptProperty)
            {
                UpdateDisplayText();
                
                // 当渠道部改变时，自动加载大区经理列表
                if (change.NewValue is DeptInfo channel && channel != null)
                {
                    Console.WriteLine($"[级联选择器] 选择渠道部: {channel.DisplayName} (ID: {channel.Id})");
                    
                    var regionManagers = new ObservableCollection<DeptInfo>();
                    foreach (var region in channel.Children)
                    {
                        regionManagers.Add(region);
                    }
                    RegionManagerList = regionManagers;
                    
                    // 清空下级选择
                    SelectedRegionManager = null;
                }
                else
                {
                    Console.WriteLine("[级联选择器] 清除渠道部选择");
                }
            }
            else if (change.Property == SelectedRegionManagerProperty)
            {
                UpdateDisplayText();
                
                // 当大区经理选择完成时，打印最终选择的部门ID
                if (change.NewValue is DeptInfo region && region != null)
                {
                    Console.WriteLine($"\n[级联选择器] 最终选择部门ID: {region.Id}\n");
                }
            }
            else if (change.Property == IsReadOnlyProperty)
            {
                if (_triggerBorder != null)
                {
                    if (IsReadOnly)
                    {
                        _triggerBorder.Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                    else
                    {
                        _triggerBorder.Cursor = new Cursor(StandardCursorType.Hand);
                    }
                }
            }
        }
        
        /// <summary>
        /// 加载部门树数据
        /// </summary>
        private async Task LoadDeptTreeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: 开始加载部门树...");
                
                var response = await NewApiClient.GetAsync<ObservableCollection<DeptInfo>>("/system/user/deptTree");
                
                if (response.Code == 200 && response.Data != null)
                {
                    // 解析部门树结构
                    // 第1级：公司（超威科技）- 跳过
                    // 第2级：部门（营销中心）- 跳过
                    // 第3级：战区 - 加载到 WarZoneList
                    
                    var warZones = new ObservableCollection<DeptInfo>();
                    
                    foreach (var company in response.Data)  // 第1级：公司
                    {
                        foreach (var dept in company.Children)  // 第2级：部门
                        {
                            // 从第3级开始处理 - 只加载战区列表
                            foreach (var warZone in dept.Children)  // 第3级：战区
                            {
                                warZones.Add(warZone);
                            }
                        }
                    }
                    
                    // 更新控件的 WarZoneList 属性
                    WarZoneList = warZones;
                    
                    System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: 成功加载 {warZones.Count} 个战区");
                    
                    // 确保 UI 元素已初始化
                    EnsureUIInitialized();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: 加载部门树失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: 加载部门树异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 确保 UI 元素已初始化
        /// </summary>
        private void EnsureUIInitialized()
        {
            // 如果 _triggerBorder 为 null，说明 Loaded 事件可能还没触发或已失效
            if (_triggerBorder == null)
            {
                System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: UI 元素未初始化，尝试重新查找...");
                
                _displayTextBlock = this.FindControl<TextBox>("PART_DisplayText");
                _triggerBorder = this.FindControl<Border>("PART_TriggerBorder");
                _popup = this.FindControl<Popup>("PART_Popup");
                _warZoneListBox = this.FindControl<ListBox>("PART_WarZoneList");
                _channelDeptListBox = this.FindControl<ListBox>("PART_ChannelDeptList");
                _regionManagerListBox = this.FindControl<ListBox>("PART_RegionManagerList");
                
                System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: 重新查找结果 - TriggerBorder={_triggerBorder != null}, Popup={_popup != null}");
                
                if (_triggerBorder != null)
                {
                    if (IsReadOnly)
                    {
                        _triggerBorder.Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                    else
                    {
                        _triggerBorder.PointerPressed += (s, e) => OpenPopup();
                    }
                    System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: TriggerBorder click handler attached");
                }
                
                if (_regionManagerListBox != null && _popup != null)
                {
                    _regionManagerListBox.SelectionChanged += (s, e) =>
                    {
                        if (_popup != null)
                        {
                            _popup.IsOpen = false;
                        }
                    };
                }
                
                // 监听 Popup 关闭事件，触发选择完成事件
                if (_popup != null)
                {
                    _popup.Closed += (s, e) =>
                    {
                        System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: Popup closed, triggering SelectionCompleted event");
                        SelectionCompleted?.Invoke(this, EventArgs.Empty);
                    };
                    System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: Popup.Closed event handler attached");
                }
                
                // 更新显示文本（设置 placeholder）
                UpdateDisplayText();
                System.Diagnostics.Debug.WriteLine($"CascadingDeptSelector: 更新显示文本为 '{_displayTextBlock?.Text}'");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CascadingDeptSelector: UI 元素已初始化，无需重复操作");
            }
        }
    }
}
