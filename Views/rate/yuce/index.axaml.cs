using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AgentManagement.Avalonia.ViewModels.rate.yuce;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.ViewModels;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Views;
using AgentManagement.Avalonia.Views.rate.enddetail;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AgentManagement.Avalonia.Views.rate.yuce
{
    public partial class index : UserControl
    {
        private Button? _monthlyButton;
        private Button? _yearlyButton;
        private Controls.DeptManagerAgentSelector? _selectorControl;
        private bool _isSelectorInitialized = false;

        public index()
        {
            InitializeComponent();
            
            // 获取按钮引用
            _monthlyButton = this.FindControl<Button>("MonthlyButton");
            _yearlyButton = this.FindControl<Button>("YearlyButton");
            _selectorControl = this.FindControl<Controls.DeptManagerAgentSelector>("SelectorControl");
            
            // 页面加载时初始化控件
            Loaded += async (s, e) => await InitializeSelectorAsync();
        }

        /// <summary>
        /// 初始化级联选择器控件
        /// </summary>
        private async Task InitializeSelectorAsync()
        {
            if (_isSelectorInitialized)
            {
                System.Diagnostics.Debug.WriteLine("[Yuce] 部门选择器已初始化，跳过");
                return;
            }
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始初始化部门选择器...");
                
                // 获取当前用户信息
                var userInfoResult = await AuthService.GetUserInfoAsync();
                
                if (userInfoResult.Success && _selectorControl != null)
                {
                    await _selectorControl.InitializeAsync(userInfoResult);
                    
                    // 设置回调方法
                    _selectorControl.OnAgentSelectedCallback = async (agent, manager, region, channel, warZone) =>
                    {
                        await OnAgentChangedAsync(agent, manager, region, channel, warZone);
                    };
                    
                    _isSelectorInitialized = true;
                    System.Diagnostics.Debug.WriteLine("[Yuce] 部门选择器初始化成功");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 部门选择器初始化失败: userInfoResult.Success={userInfoResult.Success}, _selectorControl={_selectorControl != null}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化级联选择器失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 当代理商变化时调用的回调方法
        /// </summary>
        private async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {
            var viewModel = DataContext as indexViewModel;
            if (viewModel != null)
            {
                await viewModel.OnAgentChangedAsync(agent, manager, region, channel, warZone);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            
            if (DataContext is indexViewModel vm)
            {
                // 监听 ViewMode 属性变化
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "ViewMode")
                    {
                        UpdateButtonStyles(vm.ViewMode);
                    }
                };
                
                // 监听编辑代理商产品配置请求
                vm.EditAgentProductsRequested += async (agent) =>
                {
                    await ShowAgentProductEditDialogAsync(agent);
                };
                
                // 初始化按钮样式
                UpdateButtonStyles(vm.ViewMode);
            }
        }

        private void UpdateButtonStyles(string viewMode)
        {
            if (_monthlyButton == null || _yearlyButton == null)
                return;

            if (viewMode == "monthly")
            {
                // 月度选中
                _monthlyButton.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                _monthlyButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _yearlyButton.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _yearlyButton.Foreground = new SolidColorBrush(Color.Parse("#6B7280"));
            }
            else
            {
                // 年度选中
                _monthlyButton.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _monthlyButton.Foreground = new SolidColorBrush(Color.Parse("#6B7280"));
                _yearlyButton.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                _yearlyButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
        }

        private CancellationTokenSource? _saveCts;

        private async void ProgressBarControl_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            // 取消之前的保存任务
            _saveCts?.Cancel();
            _saveCts = new CancellationTokenSource();

            try
            {
                // 等待 300ms，避免频繁值变化就立即保存
                await Task.Delay(300, _saveCts.Token);

                var viewModel = DataContext as indexViewModel;
                if (viewModel != null)
                {
                    double percentage = viewModel.GrowthRate;
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 停止拖动时保存 - percentage={percentage}");

                    long targetSalesValue = long.TryParse(viewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                    await viewModel.SaveSalesTargetAsync(targetSalesValue, percentage);
                }
            }
            catch (TaskCanceledException)
            {
                // 任务被取消，说明用户仍在拖动，不做任何事
                System.Diagnostics.Debug.WriteLine("[Yuce] 任务被取消，用户仍在拖动");
            }
        }

        private void OnUserControlPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var textBoxes = FindAllTextBoxes(this);
            foreach (var textBox in textBoxes)
            {
                if (textBox.IsFocused)
                {
                    textBox.IsEnabled = false;
                    textBox.IsEnabled = true;
                    break;
                }
            }
        }

        private List<TextBox> FindAllTextBoxes(Control parent)
        {
            var textBoxes = new List<TextBox>();
            foreach (var child in parent.GetVisualChildren())
            {
                if (child is TextBox textBox)
                {
                    textBoxes.Add(textBox);
                }
                else if (child is Control control)
                {
                    textBoxes.AddRange(FindAllTextBoxes(control));
                }
            }
            return textBoxes;
        }

        private async void TextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var viewModel = DataContext as indexViewModel;
                if (viewModel != null)
                {
                    string text = textBox.Text?.Trim() ?? string.Empty;
                    long value = 0;
                    
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (!long.TryParse(text.Replace(",", ""), out value))
                        {
                            value = 0;
                        }
                    }
                    
                    textBox.Text = value.ToString();
                    await viewModel.SaveSalesTargetAsync(value, viewModel.GrowthRate);
                }
            }
        }

        private async void HiddenSlider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            var viewModel = DataContext as indexViewModel;
            if (viewModel != null)
            {
                double percentage = viewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[Yuce] HiddenSlider_PointerReleased - percentage={percentage}");

                long targetSalesValue = long.TryParse(viewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await viewModel.SaveSalesTargetAsync(targetSalesValue, percentage);
            }
        }

        private async void Grid_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            var viewModel = DataContext as indexViewModel;
            if (viewModel != null)
            {
                double percentage = viewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[Yuce] 抬起鼠标 - percentage={percentage}");

                long targetSalesValue = long.TryParse(viewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await viewModel.SaveSalesTargetAsync(targetSalesValue, percentage);
            }
        }

        private async void HiddenSlider_ThumbDragCompleted(object? sender, EventArgs e)
        {
            var viewModel = DataContext as indexViewModel;
            if (viewModel != null)
            {
                double percentage = viewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[Yuce] 拖动完成，抬起鼠标 - percentage={percentage}");

                long targetSalesValue = long.TryParse(viewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await viewModel.SaveSalesTargetAsync(targetSalesValue, percentage);
            }
        }

        private async void ExpenseTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is indexViewModel viewModel)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal value = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    if (!decimal.TryParse(text.Replace(",", ""), out value))
                    {
                        value = 0;
                    }
                }
                textBox.Text = value.ToString();
                string expenseType = textBox.Tag?.ToString() ?? string.Empty;
                await viewModel.SaveExpenseAsync(expenseType, value);
            }
        }

        private async void ExpenseItemTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is indexViewModel viewModel)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal value = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    if (!decimal.TryParse(text.Replace(",", ""), out value))
                    {
                        value = 0;
                    }
                }
                textBox.Text = value.ToString();
                string expenseType = textBox.Tag?.ToString() ?? string.Empty;
                await viewModel.SaveExpenseAsync(expenseType, value);
            }
        }
        
        private async void ProductStructureTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is indexViewModel viewModel)
            {
                var structure = textBox.DataContext as ProductStructureDto;
                if (structure != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Yuce] ProductStructureTextBox_LostFocus - ItemModel={structure.ItemModel}");
                    await viewModel.SaveProductStructureAsync(structure);
                }
            }
        }

        private async void CalculatePreviewButton_Click(object? sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as indexViewModel;
            if (viewModel == null || !viewModel.CurrentAgentId.HasValue)
            {
                // 显示提示框
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "提示", 
                    "请先选择代理商", 
                    ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }

            string yearMonth = viewModel.IsMonthlyView 
                ? $"{viewModel.SelectedYear}-{viewModel.SelectedMonth:D2}" 
                : viewModel.SelectedYear.ToString();

            var dialog = new CalculateResultPreview();
            
            // 监听保存成功事件
            dialog.SaveSuccess += async (s, args) =>
            {
                // 刷新页面数据
                await viewModel.RefreshDataAsync();
            };
            
            await dialog.InitializeAsync(viewModel.CurrentAgentId.Value, yearMonth);
            
            if (App.MainWindow != null)
            {
                await dialog.ShowDialogAsync(App.MainWindow);
            }
        }
        
        /// <summary>
        /// 显示代理商产品配置编辑对话框
        /// </summary>
        private async Task ShowAgentProductEditDialogAsync(AgentUser agent)
        {
            try
            {
                Console.WriteLine($"[DEBUG] 打开代理商产品配置编辑对话框: {agent.DisplayName}, AgentId={agent.AgentId}");
                Console.WriteLine($"[DEBUG] index.axaml.cs - agent.AgentId type: {agent.AgentId.GetType().Name}, value: {agent.AgentId}");
                
                // 创建对话框（使用有参数构造函数，自动设置 DataContext 和事件监听）
                var dialog = new AgentProductEditDialog(agent.DisplayName, agent.AgentId);
                
                // 监听保存成功事件
                int? savedAgentId = null;
                dialog.SaveSuccess += (agentId) =>
                {
                    savedAgentId = agentId;
                    Console.WriteLine($"[DEBUG] 对话框保存成功，需要刷新代理商列表: agentId={agentId}");
                };
                
                // 显示对话框
                if (App.MainWindow != null)
                {
                    await dialog.ShowDialog(App.MainWindow);
                    
                    // 对话框关闭后，如果保存成功则提示用户
                    if (savedAgentId.HasValue)
                    {
                        Console.WriteLine($"[DEBUG] 产品配置保存成功: agentId={savedAgentId}");
                        // 注意：代理商列表的刷新由 CVPConfigViewModel 自动处理
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示产品配置编辑对话框失败: {ex.Message}");
            }
        }

        private async void ExcelImportButton_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new EndDetailImportDialog();
            if (App.MainWindow != null)
            {
                await dialog.ShowDialog(App.MainWindow);
            }
        }

        private async void TemplateDownloadButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is indexViewModel viewModel)
            {
                await viewModel.DownloadImportTemplateCommand.ExecuteAsync(null);
            }
        }
    }
}
