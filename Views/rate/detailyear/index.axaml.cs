using AgentManagement.Avalonia.ViewModels.rate.detailyear;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Views;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;


namespace AgentManagement.Avalonia.Views.rate.detailyear
{
    public partial class index : UserControl
    {
        private readonly indexViewModel _vm = new indexViewModel();

        private CancellationTokenSource? _saveCts;
        private Controls.DeptManagerAgentSelector? _selectorControl;
        private indexViewModel? ViewModel => DataContext as indexViewModel;
        
        // 动态年份
        private readonly int _year1;
        private readonly int _year2;

        public index()
        {
            InitializeComponent();
            DataContext = _vm;
            
            // 计算动态年份（当前年份+1和+2）
            int currentYear = DateTime.Now.Year;
            _year1 = currentYear + 1;
            _year2 = currentYear + 2;
            
            // 获取选择器控件引用
            _selectorControl = this.FindControl<Controls.DeptManagerAgentSelector>("SelectorControl");
            
            Loaded += async (s, e) => await InitializeSelectorAsync();
            
            // 监听用户登录事件，退出登录后重新登录时重新初始化
            Services.AuthService.UserLoggedIn += async () => await InitializeSelectorAsync();
        }

        private async System.Threading.Tasks.Task InitializeSelectorAsync()
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
            
            // 初始化年份按钮
            InitializeYearButtons();
        }

        private void InitializeYearButtons()
        {
            // 更新按钮文本
            var txtYear1 = this.FindControl<TextBlock>("TxtYear1");
            var txtYear2 = this.FindControl<TextBlock>("TxtYear2");
            
            if (txtYear1 != null)
            {
                txtYear1.Text = _year1.ToString();
            }
            
            if (txtYear2 != null)
            {
                txtYear2.Text = _year2.ToString();
            }
            
            // 初始样式
            UpdateYearButtonStyles(_year1);
        }

        private void BtnYear1_Click(object? sender, RoutedEventArgs e)
        {
            UpdateYearButtonStyles(_year1);
            if (ViewModel != null)
            {
                ViewModel.SelectYearCommand.Execute(_year1);
            }
        }

        private void BtnYear2_Click(object? sender, RoutedEventArgs e)
        {
            UpdateYearButtonStyles(_year2);
            if (ViewModel != null)
            {
                ViewModel.SelectYearCommand.Execute(_year2);
            }
        }

        private void UpdateYearButtonStyles(int selectedYear)
        {
            var btnYear1 = this.FindControl<Button>("BtnYear1");
            var btnYear2 = this.FindControl<Button>("BtnYear2");
            
            // 年份1按钮
            if (btnYear1 != null)
            {
                if (selectedYear == _year1)
                {
                    btnYear1.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                    btnYear1.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    btnYear1.BorderBrush = new SolidColorBrush(Color.Parse("#3B82F6"));
                }
                else
                {
                    btnYear1.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    btnYear1.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
                    btnYear1.BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0"));
                }
            }

            // 年份2按钮
            if (btnYear2 != null)
            {
                if (selectedYear == _year2)
                {
                    btnYear2.Background = new SolidColorBrush(Color.Parse("#3B82F6"));
                    btnYear2.Foreground = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    btnYear2.BorderBrush = new SolidColorBrush(Color.Parse("#3B82F6"));
                }
                else
                {
                    btnYear2.Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
                    btnYear2.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
                    btnYear2.BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0"));
                }
            }
        }

        private async void ProgressBarControl_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            // ȡ��֮ǰ�ı�������
            _saveCts?.Cancel();
            _saveCts = new CancellationTokenSource();

            try
            {
                // �ȴ� 300ms������ڼ�����ֵ�仯���������ᱻȡ��
                await Task.Delay(300, _saveCts.Token);

                if (ViewModel != null)
                {
                    double percentage = ViewModel.GrowthRate;
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] ֹͣ�϶����������� - percentage={percentage}");

                    long targetSalesValue = long.TryParse(ViewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                    await ViewModel.SaveSalesTargetYearAsync(targetSalesValue, percentage);
                }
            }
            catch (TaskCanceledException)
            {
                // ����ȡ����˵���û������϶��������κδ���
                System.Diagnostics.Debug.WriteLine("[DetailYear] ��������ȡ�����û������϶�");
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
            if (sender is TextBox textBox && ViewModel != null)
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
                await ViewModel.SaveSalesTargetYearAsync(value, ViewModel.GrowthRate);
            }
        }

        private async void HiddenSlider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (ViewModel != null)
            {
                double percentage = ViewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[DetailYear] HiddenSlider_PointerReleased - percentage={percentage}");

                long targetSalesValue = long.TryParse(ViewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await ViewModel.SaveSalesTargetYearAsync(targetSalesValue, percentage);
            }
        }

        private async void Grid_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (ViewModel != null)
            {
                double percentage = ViewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[DetailYear] ����ͷ� - percentage={percentage}");

                long targetSalesValue = long.TryParse(ViewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await ViewModel.SaveSalesTargetYearAsync(targetSalesValue, percentage);
            }
        }

        private async void HiddenSlider_ThumbDragCompleted(object? sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                double percentage = ViewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[DetailYear] �϶���ɣ�����ͷ� - percentage={percentage}");

                long targetSalesValue = long.TryParse(ViewModel.ConfigTargetSales?.Replace(",", "") ?? "0", out long val) ? val : 0;
                await ViewModel.SaveSalesTargetYearAsync(targetSalesValue, percentage);
            }
        }

        private async void ExpenseTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string expenseType = textBox.Tag as string ?? string.Empty;
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal amount = decimal.TryParse(text.Replace(",", ""), out decimal val) ? val : 0;
                
                textBox.Text = amount.ToString();
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] ExpenseTextBox_LostFocus - expenseType={expenseType}, amount={amount}");
                
                await ViewModel.SaveExpenseYearAsync(expenseType, amount, 0);
            }
        }

        private async void ExpenseItemTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                string text = textBox.Text?.Trim() ?? string.Empty;
                decimal amount = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    if (!decimal.TryParse(text.Replace(",", ""), out amount))
                    {
                        amount = 0;
                    }
                }
                textBox.Text = amount.ToString();
                
                // 从 DataContext 获取完整的费用项目信息
                var expenseItem = textBox.DataContext as indexViewModel.ExpenseItemViewModel;
                if (expenseItem != null)
                {
                    string expenseType = expenseItem.ExpenseType;
                    int isIncome = expenseItem.IsIncome;
                    
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] ExpenseItemTextBox_LostFocus - expenseType={expenseType}, amount={amount}, isIncome={isIncome}");
                    
                    await ViewModel.SaveExpenseYearAsync(expenseType, amount, isIncome);
                }
            }
        }

        private async void ProductStructureTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                var structure = textBox.DataContext as indexViewModel.ProductStructureDto;
                if (structure != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] ProductStructureTextBox_LostFocus - ItemModel={structure.ItemModel}");
                    await ViewModel.SaveProductStructureYearAsync(structure);
                }
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

            string yearMonth = viewModel.SelectedYear.ToString();

            var dialog = new CalculateYearResultPreview();
            
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
            var dialog = new DetailYearImportDialog();
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
