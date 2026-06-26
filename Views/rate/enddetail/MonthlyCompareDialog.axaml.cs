using AgentManagement.Avalonia.ViewModels.rate.enddetail;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace AgentManagement.Avalonia.Views.rate.enddetail
{
    public partial class MonthlyCompareDialog : Window
    {
        private MonthlyCompareDialogViewModel? _viewModel;
        private TextBlock? _productModelFilterText;
        private TextBlock? _seriesNameFilterText;
        private TextBlock? _itemNameFilterText;

        public MonthlyCompareDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            _viewModel = DataContext as MonthlyCompareDialogViewModel;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            _productModelFilterText = this.FindControl<TextBlock>("ProductModelFilterText");
            _seriesNameFilterText = this.FindControl<TextBlock>("SeriesNameFilterText");
            _itemNameFilterText = this.FindControl<TextBlock>("ItemNameFilterText");
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        // === 产品型号筛选 ===
        private void ProductModelFlyout_Opened(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    comboBox.DataContext = _viewModel;
                    comboBox.SelectedItem = _viewModel.SelectedProductModel;
                }
            }
        }

        private void ProductModelFlyout_Closed(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    var selectedValue = comboBox.SelectedItem as string;
                    _viewModel.SelectedProductModel = selectedValue;
                }
            }
        }

        private void ProductModelComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.AddedItems.Count > 0)
            {
                var selectedValue = e.AddedItems[0] as string;
                _viewModel.SelectedProductModel = selectedValue;
                Dispatcher.UIThread.Post(() =>
                {
                    var textBlock = this.FindControl<TextBlock>("ProductModelFilterText");
                    if (textBlock != null)
                        textBlock.Text = selectedValue;
                });
            }
        }

        private void ClearProductModelFilter_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedProductModel = null;
                if (_productModelFilterText != null)
                    _productModelFilterText.Text = string.Empty;
            }
        }

        // === 系列名称筛选 ===
        private void SeriesNameFlyout_Opened(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    comboBox.DataContext = _viewModel;
                    comboBox.SelectedItem = _viewModel.SelectedSeriesName;
                }
            }
        }

        private void SeriesNameFlyout_Closed(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    var selectedValue = comboBox.SelectedItem as string;
                    _viewModel.SelectedSeriesName = selectedValue;
                }
            }
        }

        private void SeriesNameComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.AddedItems.Count > 0)
            {
                var selectedValue = e.AddedItems[0] as string;
                _viewModel.SelectedSeriesName = selectedValue;
                Dispatcher.UIThread.Post(() =>
                {
                    var textBlock = this.FindControl<TextBlock>("SeriesNameFilterText");
                    if (textBlock != null)
                        textBlock.Text = selectedValue;
                });
            }
        }

        private void ClearSeriesNameFilter_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedSeriesName = null;
                if (_seriesNameFilterText != null)
                    _seriesNameFilterText.Text = string.Empty;
            }
        }

        // === 产品系列筛选 ===
        private void ItemNameFlyout_Opened(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    comboBox.DataContext = _viewModel;
                    comboBox.SelectedItem = _viewModel.SelectedItemName;
                }
            }
        }

        private void ItemNameFlyout_Closed(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                var comboBox = FindComboBox(sender);
                if (comboBox != null)
                {
                    var selectedValue = comboBox.SelectedItem as string;
                    _viewModel.SelectedItemName = selectedValue;
                }
            }
        }

        private void ItemNameComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_viewModel != null && e.AddedItems.Count > 0)
            {
                var selectedValue = e.AddedItems[0] as string;
                _viewModel.SelectedItemName = selectedValue;
                Dispatcher.UIThread.Post(() =>
                {
                    var textBlock = this.FindControl<TextBlock>("ItemNameFilterText");
                    if (textBlock != null)
                        textBlock.Text = selectedValue;
                });
            }
        }

        private void ClearItemNameFilter_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedItemName = null;
                if (_itemNameFilterText != null)
                    _itemNameFilterText.Text = string.Empty;
            }
        }

        private void ClearAllFilters_Click(object? sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedProductModel = null;
                _viewModel.SelectedSeriesName = null;
                _viewModel.SelectedItemName = null;

                if (_productModelFilterText != null)
                    _productModelFilterText.Text = string.Empty;
                if (_seriesNameFilterText != null)
                    _seriesNameFilterText.Text = string.Empty;
                if (_itemNameFilterText != null)
                    _itemNameFilterText.Text = string.Empty;
            }
        }

        private static ComboBox? FindComboBox(object? sender)
        {
            if (sender is Flyout flyout && flyout.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is ComboBox comboBox)
                        return comboBox;
                }
            }
            return null;
        }
    }
}