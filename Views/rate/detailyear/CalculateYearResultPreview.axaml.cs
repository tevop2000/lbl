using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Views.rate.detailyear
{
    public partial class CalculateYearResultPreview : Window
    {
        private long _agentId;
        private string _yearMonth = string.Empty;
        private RateMonthlyDetailYear? _currentData;

        // 保存成功事件
        public event EventHandler? SaveSuccess;

        public CalculateYearResultPreview()
        {
            InitializeComponent();
        }

        public async System.Threading.Tasks.Task InitializeAsync(long agentId, string yearMonth)
        {
            _agentId = agentId;
            _yearMonth = yearMonth;
            await LoadDataAsync();
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                var requestDto = new DetailCalculateYearDto
                {
                    AgentId = _agentId,
                    YearMonth = _yearMonth
                };

                var response = await NewApiClient.PostAsync<RateMonthlyDetailYear>(
                    "/rate/detailyear/calculateCvp",
                    requestDto);

                if (response?.Code == 200 && response.Data != null)
                {
                    _currentData = response.Data;
                    // 确保数据中的agentId和yearMonth也设置正确
                    _currentData.AgentId = _agentId;
                    _currentData.YearMonth = _yearMonth;
                    BindData();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载数据失败: {ex.Message}");
            }
        }

        private void BindData()
        {
            if (_currentData == null) return;

            // 绑定数据到界面
            AchievementRateText.Text = _currentData.AchievementRate.HasValue 
                ? $"{(_currentData.AchievementRate.Value * 100):F2}%" 
                : "0%";
            BreakSalesText.Text = _currentData.BreakSales?.ToString("N0") ?? "0";
            ImprovedSalesText.Text = _currentData.ImprovedSales?.ToString("N0") ?? "0";
            SalesGrowthProfitText.Text = _currentData.SalesGrowthProfit?.ToString("C0") ?? "¥0";
            StructureOptimizeProfitText.Text = _currentData.StructureOptimizeProfit?.ToString("C0") ?? "¥0";
            PremiumProfitText.Text = _currentData.PremiumProfit?.ToString("C0") ?? "¥0";
            TotalExtraProfitText.Text = _currentData.TotalExtraProfit?.ToString("C0") ?? "¥0";
            AdjustedNetProfitText.Text = _currentData.AdjustedNetProfit?.ToString("C0") ?? "¥0";
        }

        private void CloseButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ApplyButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_currentData == null) return;

                var requestDto = new DetailCalculateYearDto
                {
                    AgentId = _agentId,
                    YearMonth = _yearMonth,
                    RateMonthlyDetailYear = _currentData
                };

                // 输出调试信息，查看序列化后的内容
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestDto);
                System.Diagnostics.Debug.WriteLine($"发送请求数据: {json}");

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/detailyear/saveCvp",
                    requestDto);

                if (response?.Code == 200)
                {
                    // 触发保存成功事件
                    SaveSuccess?.Invoke(this, EventArgs.Empty);
                    Close();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存失败: {ex.Message}");
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        public async System.Threading.Tasks.Task ShowDialogAsync(Window owner)
        {
            await this.ShowDialog(owner);
        }
    }
}
