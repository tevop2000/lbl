using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.ViewModels.rate.enddetail
{
    public partial class MonthlyCompareDialogViewModel : ViewModelBase
    {
        private readonly long _agentId;
        private readonly string _yearMonth;

        public string Title => "月度分析/预测对比";

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _loadingMessage = "正在加载数据...";

        [ObservableProperty]
        private string _agentName = "";

        // === 卡片对比数据 ===
        [ObservableProperty]
        private long _analysisActualSales;

        [ObservableProperty]
        private long _forecastSales;

        [ObservableProperty]
        private long _salesDiff;

        [ObservableProperty]
        private string _salesDiffText;

        [ObservableProperty]
        private string _salesDiffColor;

        [ObservableProperty]
        private double _analysisAchievementRate;

        [ObservableProperty]
        private double _forecastAchievementRate;

        [ObservableProperty]
        private double _achievementRateDiff;

        [ObservableProperty]
        private string _achievementRateDiffText;

        [ObservableProperty]
        private string _achievementRateDiffColor;

        [ObservableProperty]
        private decimal _analysisNetProfit;

        [ObservableProperty]
        private decimal _forecastNetProfit;

        [ObservableProperty]
        private decimal _netProfitDiff;

        [ObservableProperty]
        private string _netProfitDiffText;

        [ObservableProperty]
        private string _netProfitDiffColor;

        // === 月度数据明细对比 ===
        public ObservableCollection<MonthlyDetailCompareItem> MonthlyDetailCompareList { get; } = new();

        // === 固定成本对比 ===
        public ObservableCollection<CostCompareItem> FixedCostCompareList { get; } = new();

        // === 收益项目对比 ===
        public ObservableCollection<CostCompareItem> IncomeItemCompareList { get; } = new();

        // === 产品配置对比 ===
        public ObservableCollection<ProductCompareItem> ProductCompareList { get; } = new();
        public ObservableCollection<ProductCompareItem> FilteredProductCompareList { get; } = new();

        // === 产品筛选（下拉模式）===
        [ObservableProperty]
        private string? _selectedProductModel;

        [ObservableProperty]
        private string? _selectedSeriesName;

        [ObservableProperty]
        private string? _selectedItemName;

        public ObservableCollection<string> ProductModelOptions { get; } = new();
        public ObservableCollection<string> SeriesNameOptions { get; } = new();
        public ObservableCollection<string> ItemNameOptions { get; } = new();

        // 筛选状态属性
        public bool HasSelectedProductModel => !string.IsNullOrWhiteSpace(SelectedProductModel);
        public bool HasSelectedSeriesName => !string.IsNullOrWhiteSpace(SelectedSeriesName);
        public bool HasSelectedItemName => !string.IsNullOrWhiteSpace(SelectedItemName);
        public bool HasAnyFilter => HasSelectedProductModel || HasSelectedSeriesName || HasSelectedItemName;

        partial void OnSelectedProductModelChanged(string? value)
        {
            ApplyProductFilter();
            OnPropertyChanged(nameof(HasSelectedProductModel));
            OnPropertyChanged(nameof(HasAnyFilter));
        }

        partial void OnSelectedSeriesNameChanged(string? value)
        {
            ApplyProductFilter();
            OnPropertyChanged(nameof(HasSelectedSeriesName));
            OnPropertyChanged(nameof(HasAnyFilter));
        }

        partial void OnSelectedItemNameChanged(string? value)
        {
            ApplyProductFilter();
            OnPropertyChanged(nameof(HasSelectedItemName));
            OnPropertyChanged(nameof(HasAnyFilter));
        }

        [RelayCommand]
        private void ClearProductModel()
        {
            SelectedProductModel = null;
        }

        [RelayCommand]
        private void ClearSeriesName()
        {
            SelectedSeriesName = null;
        }

        [RelayCommand]
        private void ClearItemName()
        {
            SelectedItemName = null;
        }

        [RelayCommand]
        private void ClearAllFilters()
        {
            SelectedProductModel = null;
            SelectedSeriesName = null;
            SelectedItemName = null;
        }

        private void ApplyProductFilter()
        {
            FilteredProductCompareList.Clear();

            var filtered = ProductCompareList.Where(item =>
                (string.IsNullOrWhiteSpace(SelectedProductModel) || item.ItemModel == SelectedProductModel) &&
                (string.IsNullOrWhiteSpace(SelectedSeriesName) || item.SeriesName == SelectedSeriesName) &&
                (string.IsNullOrWhiteSpace(SelectedItemName) || item.ItemName == SelectedItemName)
            );

            // 按产品系列（ItemName）分组排序，同一组内按产品型号排序
            var sorted = filtered.OrderBy(item => item.ItemName).ThenBy(item => item.ItemModel);

            foreach (var item in sorted)
            {
                FilteredProductCompareList.Add(item);
            }
        }

        private void LoadProductFilterOptions()
        {
            ProductModelOptions.Clear();
            SeriesNameOptions.Clear();
            ItemNameOptions.Clear();

            foreach (var item in ProductCompareList)
            {
                if (!string.IsNullOrEmpty(item.ItemModel) && !ProductModelOptions.Contains(item.ItemModel))
                {
                    ProductModelOptions.Add(item.ItemModel);
                }
                if (!string.IsNullOrEmpty(item.SeriesName) && !SeriesNameOptions.Contains(item.SeriesName))
                {
                    SeriesNameOptions.Add(item.SeriesName);
                }
                if (!string.IsNullOrEmpty(item.ItemName) && !ItemNameOptions.Contains(item.ItemName))
                {
                    ItemNameOptions.Add(item.ItemName);
                }
            }
        }

        public MonthlyCompareDialogViewModel(long agentId, string yearMonth)
        {
            _agentId = agentId;
            _yearMonth = yearMonth;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            LoadingMessage = "正在加载对比数据...";

            try
            {
                await Task.WhenAll(
                    LoadCardDataAsync(),
                    LoadMonthlyDetailDataAsync(),
                    LoadExpenseDataAsync(),
                    LoadProductDataAsync()
                );

                CalculateDiffs();
            }
            catch (Exception ex)
            {
                Logger.Error($"加载对比数据失败: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadCardDataAsync()
        {
            LoadingMessage = "正在加载核心指标...";

            // 加载分析数据
            var analysisResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/endsaletarget/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            if (analysisResponse.Code == 200 && analysisResponse.Data != null)
            {
                var data = analysisResponse.Data as Newtonsoft.Json.Linq.JObject;
                if (data != null)
                {
                    AnalysisActualSales = data["actualSales"]?.ToObject<long>() ?? 0;
                    AgentName = data["agentName"]?.ToString() ?? "";
                }
            }

            // 加载预测数据
            var forecastResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/saletarget/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            if (forecastResponse.Code == 200 && forecastResponse.Data != null)
            {
                var data = forecastResponse.Data as Newtonsoft.Json.Linq.JObject;
                if (data != null)
                {
                    ForecastSales = data["targetSales"]?.ToObject<long>() ?? 0;                   
                }
            }

            // 计算达成率
            var enddetailResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/enddetail/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            if (enddetailResponse.Code == 200 && enddetailResponse.Data != null)
            {
                var data = enddetailResponse.Data as Newtonsoft.Json.Linq.JObject;
                if (data != null)
                {
                    AnalysisAchievementRate = (data["achievementRate"]?.ToObject<double>() ?? 0) * 100;
                    AnalysisNetProfit = data["adjustedNetProfit"]?.ToObject<decimal>() ?? 0;
                }
            }

            // 获取预测净利润
            var yuceResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/detail/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            if (yuceResponse.Code == 200 && yuceResponse.Data != null)
            {
                var data = yuceResponse.Data as Newtonsoft.Json.Linq.JObject;
                if (data != null)
                {  
                    ForecastAchievementRate = (data["achievementRate"]?.ToObject<double>() ?? 0) * 100;
                    ForecastNetProfit = data["adjustedNetProfit"]?.ToObject<decimal>() ?? 0;
                }
            }
        }

        private async Task LoadMonthlyDetailDataAsync()
        {
            LoadingMessage = "正在加载月度明细...";

            MonthlyDetailCompareList.Clear();

            // 加载分析数据
            var analysisResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/enddetail/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            dynamic analysisData = null;
            if (analysisResponse.Code == 200 && analysisResponse.Data != null)
            {
                analysisData = analysisResponse.Data as Newtonsoft.Json.Linq.JObject;
            }

            // 加载预测数据
            var forecastResponse = await NewApiClient.GetAsync<dynamic>(
                $"/rate/detail/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            dynamic forecastData = null;
            if (forecastResponse.Code == 200 && forecastResponse.Data != null)
            {
                forecastData = forecastResponse.Data as Newtonsoft.Json.Linq.JObject;
            }

            var yearMonthStr = _yearMonth;
            int month = 0;
            if (!string.IsNullOrEmpty(yearMonthStr) && yearMonthStr.Contains("-"))
            {
                var monthPart = yearMonthStr.Split('-')[1];
                int.TryParse(monthPart, out month);
            }

            // 创建对比项
            MonthlyDetailCompareList.Add(new MonthlyDetailCompareItem
            {
                Month = month,             
                AnalysisTargetSales = analysisData?["targetSales"]?.ToObject<long>() ?? 0,
                ForecastTargetSales = forecastData?["targetSales"]?.ToObject<long>() ?? 0,
                AnalysisAchievementRate = (analysisData?["achievementRate"]?.ToObject<double>() ?? 0) * 100,
                ForecastAchievementRate = (forecastData?["achievementRate"]?.ToObject<double>() ?? 0) * 100,
                AnalysisActualSales = analysisData?["actualSales"]?.ToObject<long>() ?? 0,
                ForecastActualSales = forecastData?["actualSales"]?.ToObject<long>() ?? 0,
                AnalysisNetProfit = analysisData?["adjustedNetProfit"]?.ToObject<decimal>() ?? analysisData?["netProfit"]?.ToObject<decimal>() ?? 0,
                ForecastNetProfit = forecastData?["adjustedNetProfit"]?.ToObject<decimal>() ?? forecastData?["netProfit"]?.ToObject<decimal>() ?? 0
            });
        }

        private async Task LoadExpenseDataAsync()
        {
            LoadingMessage = "正在加载费用数据...";

            FixedCostCompareList.Clear();
            IncomeItemCompareList.Clear();

            // 加载分析费用数据
            var analysisResponse = await NewApiClient.GetAsync<List<dynamic>>(
                $"/rate/endexpense/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            Dictionary<string, decimal> analysisFixedCosts = new();
            Dictionary<string, decimal> analysisIncomes = new();

            if (analysisResponse.Code == 200 && analysisResponse.Data != null)
            {
                foreach (var item in analysisResponse.Data)
                {
                    string expenseType = item["expenseType"]?.ToString() ?? "";
                    decimal amount = item["amount"]?.ToObject<decimal>() ?? 0;
                    int isIncome = item["isIncome"]?.ToObject<int>() ?? 0;

                    if (isIncome == 0)
                        analysisFixedCosts[expenseType] = amount;
                    else
                        analysisIncomes[expenseType] = amount;
                }
            }

            // 加载预测费用数据
            var forecastResponse = await NewApiClient.GetAsync<List<dynamic>>(
                $"/rate/expense/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            Dictionary<string, decimal> forecastFixedCosts = new();
            Dictionary<string, decimal> forecastIncomes = new();

            if (forecastResponse.Code == 200 && forecastResponse.Data != null)
            {
                foreach (var item in forecastResponse.Data)
                {
                    string expenseType = item["expenseType"]?.ToString() ?? "";
                    decimal amount = item["amount"]?.ToObject<decimal>() ?? 0;
                    int isIncome = item["isIncome"]?.ToObject<int>() ?? 0;

                    if (isIncome == 0)
                        forecastFixedCosts[expenseType] = amount;
                    else
                        forecastIncomes[expenseType] = amount;
                }
            }

            // 合并固定成本
            var allFixedCostTypes = analysisFixedCosts.Keys.Union(forecastFixedCosts.Keys).ToList();
            foreach (var type in allFixedCostTypes)
            {
                FixedCostCompareList.Add(new CostCompareItem
                {
                    ExpenseType = type,
                    AnalysisAmount = analysisFixedCosts.ContainsKey(type) ? analysisFixedCosts[type] : 0,
                    ForecastAmount = forecastFixedCosts.ContainsKey(type) ? forecastFixedCosts[type] : 0
                });
            }

            // 合并收益项目
            var allIncomeTypes = analysisIncomes.Keys.Union(forecastIncomes.Keys).ToList();
            foreach (var type in allIncomeTypes)
            {
                IncomeItemCompareList.Add(new CostCompareItem
                {
                    ExpenseType = type,
                    AnalysisAmount = analysisIncomes.ContainsKey(type) ? analysisIncomes[type] : 0,
                    ForecastAmount = forecastIncomes.ContainsKey(type) ? forecastIncomes[type] : 0
                });
            }
        }

        private async Task LoadProductDataAsync()
        {
            LoadingMessage = "正在加载产品配置...";

            ProductCompareList.Clear();

            // 加载分析产品数据
            var analysisResponse = await NewApiClient.GetAsync<List<dynamic>>(
                $"/rate/endstructure/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            Dictionary<string, dynamic> analysisProducts = new();

            if (analysisResponse.Code == 200 && analysisResponse.Data != null)
            {
                foreach (var item in analysisResponse.Data)
                {
                    string itemModel = item["itemModel"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(itemModel))
                        analysisProducts[itemModel] = item;
                }
            }

            // 加载预测产品数据
            var forecastResponse = await NewApiClient.GetAsync<List<dynamic>>(
                $"/rate/structure/getByAgentIdAndYearMonth?agentId={_agentId}&yearMonth={_yearMonth}")
                .ConfigureAwait(true);

            Dictionary<string, dynamic> forecastProducts = new();

            if (forecastResponse.Code == 200 && forecastResponse.Data != null)
            {
                foreach (var item in forecastResponse.Data)
                {
                    string itemModel = item["itemModel"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(itemModel))
                        forecastProducts[itemModel] = item;
                }
            }

            // 合并产品
            var allProductModels = analysisProducts.Keys.Union(forecastProducts.Keys).ToList();
            foreach (var model in allProductModels)
            {
                var analysisItem = analysisProducts.ContainsKey(model) ? analysisProducts[model] : null;
                var forecastItem = forecastProducts.ContainsKey(model) ? forecastProducts[model] : null;

                ProductCompareList.Add(new ProductCompareItem
                {
                    ItemModel = model,
                    SeriesName = analysisItem?["seriesName"]?.ToString() ?? forecastItem?["seriesName"]?.ToString() ?? "",
                    ItemName = analysisItem?["itemName"]?.ToString() ?? forecastItem?["itemName"]?.ToString() ?? "",
                    AnalysisStructureRatio = analysisItem?["structureRatio"]?.ToObject<decimal>() ?? 0,
                    ForecastStructureRatio = forecastItem?["structureRatio"]?.ToObject<decimal>() ?? 0,
                    AnalysisRemiumPrice = analysisItem?["remiumPrice"]?.ToObject<decimal>() ?? 0,
                    ForecastRemiumPrice = forecastItem?["remiumPrice"]?.ToObject<decimal>() ?? 0,
                    AnalysisRemiumCost = analysisItem?["remiumCost"]?.ToObject<decimal>() ?? 0,
                    ForecastRemiumCost = forecastItem?["remiumCost"]?.ToObject<decimal>() ?? 0,
                    AnalysisCommission = analysisItem?["commission"]?.ToObject<decimal>() ?? 0,
                    ForecastCommission = forecastItem?["commission"]?.ToObject<decimal>() ?? 0
                });
            }

            ApplyProductFilter();
            LoadProductFilterOptions();
        }

        private void CalculateDiffs()
        {
            // 销量差异
            SalesDiff = AnalysisActualSales - ForecastSales;
            SalesDiffText = SalesDiff >= 0 ? $"↑ {SalesDiff:N0}" : $"↓ {Math.Abs(SalesDiff):N0}";
            SalesDiffColor = SalesDiff >= 0 ? "#10B981" : "#EF4444";

            // 达成率差异
            AchievementRateDiff = AnalysisAchievementRate - ForecastAchievementRate;
            AchievementRateDiffText = AchievementRateDiff >= 0 ? $"↑ {AchievementRateDiff:F2}%" : $"↓ {Math.Abs(AchievementRateDiff):F2}%";
            AchievementRateDiffColor = AchievementRateDiff >= 0 ? "#10B981" : "#EF4444";

            // 净利润差异
            NetProfitDiff = AnalysisNetProfit - ForecastNetProfit;
            NetProfitDiffText = NetProfitDiff >= 0 ? $"↑ {NetProfitDiff:N2}" : $"↓ {Math.Abs(NetProfitDiff):N2}";
            NetProfitDiffColor = NetProfitDiff >= 0 ? "#10B981" : "#EF4444";
        }

        [RelayCommand]
        private void Close()
        {
            Logger.Info("关闭月度分析/预测对比对话框");
        }
    }

    public class MonthlyDetailCompareItem
    {
        public int Month { get; set; }
        public long AnalysisTargetSales { get; set; }
        public long ForecastTargetSales { get; set; }
        public double AnalysisAchievementRate { get; set; }
        public double ForecastAchievementRate { get; set; }
        public long AnalysisActualSales { get; set; }
        public long ForecastActualSales { get; set; }
        public decimal AnalysisNetProfit { get; set; }
        public decimal ForecastNetProfit { get; set; }
        
        public long SalesDiff => AnalysisActualSales - ForecastActualSales;
        public decimal ProfitDiff => AnalysisNetProfit - ForecastNetProfit;
        
        public string SalesDiffColor => SalesDiff >= 0 ? "#10B981" : "#EF4444";
        public string ProfitDiffColor => ProfitDiff >= 0 ? "#10B981" : "#EF4444";
    }

    public class CostCompareItem
    {
        public string ExpenseType { get; set; } = string.Empty;
        public decimal AnalysisAmount { get; set; }
        public decimal ForecastAmount { get; set; }
        public decimal Diff => AnalysisAmount - ForecastAmount;
        public string DiffText => Diff >= 0 ? $"↑ {Diff:N2}" : $"↓ {Math.Abs(Diff):N2}";
        public string DiffColor => Diff >= 0 ? "#10B981" : "#EF4444";
    }

    public class ProductCompareItem
    {
        public string ItemModel { get; set; } = string.Empty;
        public string SeriesName { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal AnalysisStructureRatio { get; set; }
        public decimal ForecastStructureRatio { get; set; }
        public decimal AnalysisRemiumPrice { get; set; }
        public decimal ForecastRemiumPrice { get; set; }
        public decimal AnalysisRemiumCost { get; set; }
        public decimal ForecastRemiumCost { get; set; }
        public decimal AnalysisCommission { get; set; }
        public decimal ForecastCommission { get; set; }
    }
}