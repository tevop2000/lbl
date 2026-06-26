using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AgentManagement.Avalonia.ViewModels.rate.marketfact;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Services;

namespace AgentManagement.Avalonia.Views.rate.marketfact
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


        private async void OnCwEmployeesLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleLongField(sender, "cwEmployees");
        }

        private async void OnCwVehiclesLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleLongField(sender, "cwVehicles");
        }

        private async void OnCwSpecialFundLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleDecimalField(sender, "cwSpecialFund");
        }

        private async void OnTnEmployeesLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleLongField(sender, "tnEmployees");
        }

        private async void OnTnVehiclesLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleLongField(sender, "tnVehicles");
        }

        private async void OnTnSpecialFundLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleDecimalField(sender, "tnSpecialFund");
        }

        private async void OnChannelCoverageLostFocus(object? sender, RoutedEventArgs e)
        {
            HandleDecimalField(sender, "channelCoverage");
        }

        private async void HandleLongField(object? sender, string fieldName)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                long value = 0;
                
                if (!string.IsNullOrEmpty(text))
                {
                    if (!long.TryParse(text, out value))
                    {
                        value = 0;
                    }
                }
                
                textBox.Text = value.ToString();
                await ViewModel.SaveAgentResourceAsync(fieldName, value);
            }
        }

        private async void HandleDecimalField(object? sender, string fieldName)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal value = 0;
                
                if (!string.IsNullOrEmpty(text))
                {
                    if (!decimal.TryParse(text, out value))
                    {
                        value = 0;
                    }
                }
                
                textBox.Text = value.ToString();
                await ViewModel.SaveAgentResourceAsync(fieldName, value);
            }
        }

        private async void OnSalesLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal value = 0;
                
                if (!string.IsNullOrEmpty(text))
                {
                    if (!decimal.TryParse(text, out value))
                    {
                        value = 0;
                    }
                }
                
                textBox.Text = value.ToString();
                
                var parent = textBox.Parent;
                while (parent != null && !(parent is Panel))
                {
                    parent = parent.Parent;
                }
                
                if (parent is Panel panel)
                {
                    var dataContext = panel.DataContext as SalesComparisonItem;
                    if (dataContext != null)
                    {
                        await ViewModel.SaveBrandSalesComparisonAsync(
                            dataContext.SeriesName,
                            dataContext.CwSales,
                            dataContext.TnSales,
                            dataContext.SmallBrandSales);
                    }
                }
            }
        }

        private async void ExcelImportButton_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new MarketFactImportDialog();
            if (App.MainWindow != null)
            {
                await dialog.ShowDialog(App.MainWindow);
            }
        }

        private async void TemplateDownloadButton_Click(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.DownloadTemplateAsync();
            }
        }
    }
}
