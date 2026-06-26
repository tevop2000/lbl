using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Views.rate.enddetail
{
    public partial class EndResultPreview : Window
    {
        private long _agentId;
        private string _yearMonth = string.Empty;
        private RateMonthlyEndDetail? _currentData;

        // 保存成功事件
        public event EventHandler? SaveSuccess;

        public EndResultPreview()
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
                var requestDto = new CvpCalculateDto
                {
                    AgentId = _agentId,
                    YearMonth = _yearMonth
                };

                var response = await NewApiClient.PostAsync<RateMonthlyEndDetail>(
                    "/rate/enddetail/calculateEndOneCvp",
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
            TargetSalesText.Text = _currentData.TargetSales?.ToString("N0") ?? "0";
            AchievementRateText.Text = _currentData.AchievementRate.HasValue 
                ? $"{(_currentData.AchievementRate.Value * 100):F2}%" 
                : "0%";
            ActualSalesText.Text = _currentData.ActualSales?.ToString("N0") ?? "0";
            SalesAmountText.Text = _currentData.SalesAmount?.ToString("C0") ?? "¥0";
            TotalCostText.Text = _currentData.TotalCost?.ToString("C0") ?? "¥0";
            ExpenseRateText.Text = _currentData.ExpenseRate.HasValue 
                ? $"{(_currentData.ExpenseRate.Value * 100):F2}%" 
                : "0%";
            AdjustedNetProfitText.Text = _currentData.AdjustedNetProfit?.ToString("C0") ?? "¥0";
            NetProfitRateText.Text = _currentData.NetProfitRate.HasValue 
                ? $"{(_currentData.NetProfitRate.Value * 100):F2}%" 
                : "0%";
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

                var requestDto = new CvpCalculateDto
                {
                    AgentId = _agentId,
                    YearMonth = _yearMonth,
                    RateMonthlyEndDetail = _currentData
                };

                // 输出调试信息，查看序列化后的内容
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestDto);
                System.Diagnostics.Debug.WriteLine($"发送请求数据: {json}");

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/enddetail/saveEndCvp",
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
