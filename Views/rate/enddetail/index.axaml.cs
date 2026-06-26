using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AgentManagement.Avalonia.ViewModels.rate.enddetail;
using AgentManagement.Avalonia.Services;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AgentManagement.Avalonia.Views.rate.enddetail
{
    public partial class index : UserControl
    {
        private Controls.DeptManagerAgentSelector? _selectorControl;
        private indexViewModel? ViewModel => DataContext as indexViewModel;

        private Button? _monthlyButton;
        private Button? _yearlyButton;
        private TextBox? _configTargetSalesTextBox;
        private TextBox? _actualSalesTextBox;

        public index()
        {
            InitializeComponent();

            // Get selector control reference
            _selectorControl = this.FindControl<Controls.DeptManagerAgentSelector>("SelectorControl");

            // Get view mode buttons reference
            _monthlyButton = this.FindControl<Button>("MonthlyButton");
            _yearlyButton = this.FindControl<Button>("YearlyButton");
            
            // Get textbox references
            _configTargetSalesTextBox = this.FindControl<TextBox>("ConfigTargetSalesTextBox");
            _actualSalesTextBox = this.FindControl<TextBox>("ActualSalesTextBox");

            Loaded += async (s, e) => await InitializeSelectorAsync();
            
            // 监听用户登录事件，退出登录后重新登录时重新初始化
            Services.AuthService.UserLoggedIn += async () => await InitializeSelectorAsync();
        }

        private async System.Threading.Tasks.Task InitializeSelectorAsync()
        {
            try
            {
                // Get current user info
                var userInfoResult = await AuthService.GetUserInfoAsync();

                if (userInfoResult.Success && _selectorControl != null)
                {
                    await _selectorControl.InitializeAsync(userInfoResult);

                    // Set callback
                    _selectorControl.OnAgentSelectedCallback = async (agent, manager, region, channel, warZone) =>
                    {
                        if (ViewModel != null)
                        {
                            await ViewModel.OnAgentChangedAsync(agent, manager, region, channel, warZone);
                        }
                    };

                    // Initialize data
                    if (ViewModel != null)
                    {
                        await ViewModel.InitializeDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize cascading selector: {ex.Message}");
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (DataContext is indexViewModel vm)
            {
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "ViewMode")
                    {
                        UpdateViewModeButtonStyles(vm.ViewMode);
                    }
                };

                UpdateViewModeButtonStyles(vm.ViewMode);
            }
        }

        private void UpdateViewModeButtonStyles(string viewMode)
        {
            if (_monthlyButton == null || _yearlyButton == null)
                return;

            if (viewMode == "monthly")
            {
                _monthlyButton.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                _monthlyButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _yearlyButton.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _yearlyButton.Foreground = new SolidColorBrush(Color.Parse("#6B7280"));
            }
            else
            {
                _monthlyButton.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                _monthlyButton.Foreground = new SolidColorBrush(Color.Parse("#6B7280"));
                _yearlyButton.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                _yearlyButton.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
        }

        private async void ConfigTargetSalesTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveSaleTargetAsync();
            }
        }

        private async void ActualSalesTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.SaveSaleTargetAsync();
            }
        }
        
        private async void ExpenseTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string? expenseType = textBox.Tag as string;
                if (!string.IsNullOrEmpty(expenseType))
                {
                    // 尝试解析值
                    if (decimal.TryParse(textBox.Text, out decimal amount))
                    {
                        await ViewModel.SaveExpenseAsync(expenseType, amount);
                    }
                }
            }
        }

        private async void ProductStructureRatio_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                var product = textBox.Tag as ProductItem;
                if (product != null)
                {
                    await ViewModel.SaveSingleProductAsync(product);
                }
            }
        }

        private async void ProductRemiumPrice_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                var product = textBox.Tag as ProductItem;
                if (product != null)
                {
                    await ViewModel.SaveSingleProductAsync(product);
                }
            }
        }

        private async void ProductRemiumCost_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                var product = textBox.Tag as ProductItem;
                if (product != null)
                {
                    await ViewModel.SaveSingleProductAsync(product);
                }
            }
        }

        private async void MainPanel_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // 当点击主面板时，检查输入框是否有焦点，如果有则调用保存
            bool anyTextBoxFocused = 
                (_configTargetSalesTextBox != null && _configTargetSalesTextBox.IsFocused) ||
                (_actualSalesTextBox != null && _actualSalesTextBox.IsFocused);

            if (anyTextBoxFocused && ViewModel != null)
            {
                // 让焦点移到主面板，触发 LostFocus 事件
                MainPanel.Focus(NavigationMethod.Pointer);
            }
        }

        private async void PreviewButton_Click(object? sender, RoutedEventArgs e)
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

            var dialog = new EndResultPreview();
            
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

        private async void ExcelImportButton_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new EndDetailImportDialog("/rate/endsaletarget/importData");
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
