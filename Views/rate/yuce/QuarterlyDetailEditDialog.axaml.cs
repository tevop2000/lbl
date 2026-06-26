using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Controls.Primitives;
using AgentManagement.Avalonia.ViewModels.rate.yuce;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgentManagement.Avalonia.Views.rate.yuce
{
    public partial class QuarterlyDetailEditDialog : Window
    {
        private QuarterlyDetailEditDialogViewModel? ViewModel => DataContext as QuarterlyDetailEditDialogViewModel;
        private CancellationTokenSource? _saveCts;

        public QuarterlyDetailEditDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void TargetSalesTextBox_LostFocus(object? sender, RoutedEventArgs e)
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
                ViewModel.TargetSales = value;
                await ViewModel.SaveSalesTargetAsync();
            }
        }

        private async void FixedCostTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
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
                var item = textBox.DataContext as FixedCostItem;
                if (item != null)
                {
                    item.Amount = value;
                    await ViewModel.SaveExpenseAsync(item.ExpenseType, value, 0);
                }
            }
        }

        private async void IncomeItemTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
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
                var item = textBox.DataContext as FixedCostItem;
                if (item != null)
                {
                    item.Amount = value;
                    await ViewModel.SaveExpenseAsync(item.ExpenseType, value, 1);
                }
            }
        }

        private async void ProductStructureTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && ViewModel != null)
            {
                var structure = textBox.DataContext as ProductStructure;
                if (structure != null)
                {
                    await ViewModel.SaveProductStructureAsync(structure);
                }
            }
        }

        private async void GrowthRateProgressBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            _saveCts?.Cancel();
            _saveCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _saveCts.Token);

                if (ViewModel != null)
                {
                    double growthRate = ViewModel.GrowthRate;
                    System.Diagnostics.Debug.WriteLine($"[QuarterlyDetailEditDialog] GrowthRate ValueChanged - growthRate={growthRate}");
                    await ViewModel.SaveSalesTargetAsync();
                }
            }
            catch (TaskCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[QuarterlyDetailEditDialog] GrowthRate保存任务被取消");
            }
        }

        private async void HiddenSlider_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (ViewModel != null)
            {
                double growthRate = ViewModel.GrowthRate;
                System.Diagnostics.Debug.WriteLine($"[QuarterlyDetailEditDialog] HiddenSlider_PointerReleased - growthRate={growthRate}");
                await ViewModel.SaveSalesTargetAsync();
            }
        }
    }
}
