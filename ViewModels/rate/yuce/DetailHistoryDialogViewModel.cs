using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.ViewModels.rate.yuce
{
    public partial class DetailHistoryDialogViewModel : ObservableObject
    {
        private readonly long _agentId;
        private readonly string _yearMonth;
        private readonly long _versionId;

        public DetailHistoryDialogViewModel(long agentId, string yearMonth, long versionId)
        {
            _agentId = agentId;
            _yearMonth = yearMonth;
            _versionId = versionId;
        }

        [ObservableProperty]
        private string _title = "方案详情";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = "加载中...";

        [ObservableProperty]
        private ObservableCollection<MonthlyDataItem> _monthlyDetailList = new();

        [ObservableProperty]
        private decimal _targetSales = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SalesAchievementRate))]
        private decimal _salesGrowthRate = 0;

        public decimal SalesAchievementRate => SalesGrowthRate + 100;

        [ObservableProperty]
        private ObservableCollection<ExpenseItemViewModel> _fixedCostItems = new();

        [ObservableProperty]
        private ObservableCollection<ExpenseItemViewModel> _incomeItems = new();

        public ObservableCollection<ProductStructureDto> ProductStructures { get; } = new();

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            LoadingMessage = "正在加载月度数据明细...";

            try
            {
                await LoadMonthlyDetailAsync();
                
                LoadingMessage = "正在加载参数详情...";
                await Task.WhenAll(
                    LoadSaleTargetHistoryAsync(),
                    LoadExpenseHistoryAsync(),
                    LoadStructureHistoryAsync()
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"加载方案详情失败: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMonthlyDetailAsync()
        {
            var url = $"/rate/detailhistory/getRateMonthlyDetailHistory?agentId={_agentId}&yearMonth={_yearMonth}&versionId={_versionId}";
            var response = await NewApiClient.GetAsync<dynamic>(url);
            
            if (response.Code == 200 && response.Data != null)
            {
                MonthlyDetailList.Clear();
                
                var jArray = response.Data as JArray;
                if (jArray != null)
                {
                    foreach (var item in jArray)
                    {
                        AddMonthlyDataItem(item as JObject);
                    }
                }
                else
                {
                    var jObject = response.Data as JObject;
                    if (jObject != null)
                    {
                        AddMonthlyDataItem(jObject);
                    }
                }
            }
        }

        private void AddMonthlyDataItem(JObject data)
        {
            if (data == null) return;

            var yearMonthStr = data["yearMonth"]?.ToString() ?? "";
            int month = 0;
            if (!string.IsNullOrEmpty(yearMonthStr) && yearMonthStr.Contains("-"))
            {
                var monthPart = yearMonthStr.Split('-')[1];
                int.TryParse(monthPart, out month);
            }
            var targetSales = GetSafeValue<double>(data, "targetSales", 0);
            var achievementRate = GetSafeValue<decimal>(data, "achievementRate", 0);
            var improvedSales = GetSafeValue<double>(data, "improvedSales", 0);
            var salesGrowth = GetSafeValue<decimal>(data, "salesGrowthProfit", 0);
            var structureOptimize = GetSafeValue<decimal>(data, "structureOptimizeProfit", 0);
            var premium = GetSafeValue<decimal>(data, "premiumProfit", 0);
            var totalExtra = GetSafeValue<decimal>(data, "totalExtraProfit", 0);
            var adjustedNetProfit = GetSafeValue<decimal>(data, "adjustedNetProfit", 0);

            var item = new MonthlyDataItem
            {
                AgentName = data["agentName"]?.ToString() ?? "",
                Month = month,
                TargetSales = targetSales,
                BreakEvenRate = (double)achievementRate,
                BreakEvenRatePercent = (double)achievementRate * 100,
                OptimizedSales = improvedSales,
                SalesGrowthProfit = (double)salesGrowth,
                StructureOptimizeProfit = (double)structureOptimize,
                PremiumProfit = (double)premium,
                TotalExtraProfit = (double)totalExtra,
                AdjustedNetProfit = (double)adjustedNetProfit
            };

            MonthlyDetailList.Add(item);
        }

        private async Task LoadSaleTargetHistoryAsync()
        {
            var url = $"/rate/saletargethistory/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}&versionId={_versionId}";
            var response = await NewApiClient.GetAsync<dynamic>(url);
            
            if (response.Code == 200 && response.Data != null)
            {
                var data = response.Data as JObject;
                if (data != null)
                {
                    TargetSales = GetSafeValue<decimal>(data, "targetSales", 0);
                    SalesGrowthRate = GetSafeValue<decimal>(data, "growthRate", 0);
                }
            }
        }

        private async Task LoadExpenseHistoryAsync()
        {
            var url = $"/rate/expensehistory/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}&versionId={_versionId}";
            var response = await NewApiClient.GetAsync<dynamic>(url);
            
            if (response.Code == 200 && response.Data != null)
            {
                var jArray = response.Data as JArray;
                if (jArray != null)
                {
                    FixedCostItems.Clear();
                    IncomeItems.Clear();
                    
                    foreach (var item in jArray)
                    {
                        var obj = item as JObject;
                        if (obj != null)
                        {
                            var expenseItem = new ExpenseItemViewModel
                            {
                                ExpenseType = obj["expenseType"]?.ToString() ?? "",
                                Amount = GetSafeValue<decimal>(obj, "amount", 0),
                                IsIncome = GetSafeValue<int>(obj, "isIncome", 0)
                            };
                            
                            if (expenseItem.IsIncome == 1)
                            {
                                IncomeItems.Add(expenseItem);
                            }
                            else
                            {
                                FixedCostItems.Add(expenseItem);
                            }
                        }
                    }
                }
            }
        }

        private async Task LoadStructureHistoryAsync()
        {
            var url = $"/rate/structurehistory/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}&versionId={_versionId}";
            var response = await NewApiClient.GetAsync<List<ProductStructureDto>>(url);

            if (response?.Code == 200 && response.Data != null)
            {
                ProductStructures.Clear();
                foreach (var item in response.Data)
                {
                    ProductStructures.Add(item);
                }
            }
            else
            {
                ProductStructures.Clear();
            }
        }

        private T GetSafeValue<T>(JObject data, string propertyName, T defaultValue)
        {
            if (data == null || !data.ContainsKey(propertyName))
                return defaultValue;

            var value = data[propertyName];
            if (value == null || value.Type == JTokenType.Null)
                return defaultValue;

            try
            {
                return value.ToObject<T>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}