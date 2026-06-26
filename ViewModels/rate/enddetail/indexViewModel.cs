using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.Models;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace AgentManagement.Avalonia.ViewModels.rate.enddetail
{
    public partial class indexViewModel : ViewModelBase
    {
        // KPI 卡片数据
        [ObservableProperty]
        private long _targetSales = 0;

        [ObservableProperty]
        private decimal _achievementRate = 0;

        [ObservableProperty]
        private double _achievementRatePercent = 0;

        [ObservableProperty]
        private long _actualSales = 0;

        [ObservableProperty]
        private decimal _netProfit = 0;

        [ObservableProperty]
        private bool _isProfitable = true;

        // 月度数据列表
        [ObservableProperty]
        private ObservableCollection<MonthlyDataItem> _monthlyDataList = new();

        // 当前代理商信息
        [ObservableProperty]
        private string _currentAgentName = "未选择";

        [ObservableProperty]
        private long? _currentAgentId = null;

        // 年份和月份
        // 年份列表
        [ObservableProperty]
        private ObservableCollection<int> _yearList = new();

        // 当前年份
        [ObservableProperty]
        private int _selectedYear = DateTime.Now.Year - 1;

        // 月份列表
        [ObservableProperty]
        private ObservableCollection<int> _monthList = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        [ObservableProperty]
        private int _selectedMonth = DateTime.Now.Month;

        partial void OnSelectedYearChanged(int value)
        {
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                if (ViewMode == "yearly")
                {
                    // 如果是年度视图，加载年度数据
                    LoadYearlyDataAsync().ConfigureAwait(false);
                }
                else
                {
                    // 如果是月度视图，加载月度数据
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        partial void OnSelectedMonthChanged(int value)
        {
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                if (ViewMode == "yearly")
                {
                    // 如果是年度视图，加载年度数据
                    LoadYearlyDataAsync().ConfigureAwait(false);
                }
                else
                {
                    // 如果是月度视图，加载月度数据
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        // 视图模式
        [ObservableProperty]
        private string _viewMode = "monthly"; // monthly 或 yearly

        // 视图模式判断属性
        public bool IsMonthlyView => ViewMode == "monthly";
        public bool IsYearlyView => ViewMode == "yearly";

        partial void OnViewModeChanged(string value)
        {
            OnPropertyChanged(nameof(IsMonthlyView));
            OnPropertyChanged(nameof(IsYearlyView));
            
            // 切换到年度视图时加载数据
            if (value == "yearly" && CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                LoadYearlyDataAsync().ConfigureAwait(false);
            }
        }

        // ========== 基础配置 ==========
        [ObservableProperty]
        private long _configTargetSales = 0;

        [ObservableProperty]
        private string _actualSalesStatus = "未录入";

        [ObservableProperty]
        private long _fixedCost = 0;

        [ObservableProperty]
        private decimal _batteryCost = 0;

        [ObservableProperty]
        private decimal _rentCost = 0;

        [ObservableProperty]
        private decimal _salaryCost = 0;

        [ObservableProperty]
        private decimal _vehicleCost = 0;

        [ObservableProperty]
        private decimal _utilityCost = 0;

        [ObservableProperty]
        private decimal _pIncome = 0;

        [ObservableProperty]
        private decimal _acceptanceIncome = 0;

        [ObservableProperty]
        private decimal _afterSalesIncome = 0;

        // ========== 产品配置 ==========
        [ObservableProperty]
        private ObservableCollection<ProductItem> _productList = new();
        
        // 产品占比总和显示
        [ObservableProperty]
        private string _proportionSumText = "产品占比总和: 0.00";
        
        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4"; // green-50
        
        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A"; // green-600

        // ========== 年度视图 - KPI 数据 ==========
        [ObservableProperty]
        private long _yearlyAvgMonthlySales = 0;

        [ObservableProperty]
        private long _yearlyTotalSales = 0;

        [ObservableProperty]
        private decimal _yearlyTotalSalesAmount = 0;

        [ObservableProperty]
        private decimal _yearlyTotalCost = 0;

        [ObservableProperty]
        private decimal _yearlyTotalProfit = 0;

        // ========== 年度视图 - 年度汇总数据 ==========
        [ObservableProperty]
        private ObservableCollection<YearlySummaryItem> _yearlySummaryList = new();

        // ========== 年度视图 - 年度同比分析数据 ==========
    [ObservableProperty]
    private ObservableCollection<YearlyComparisonItem> _yearlyComparisonList = new();

    // ========== 图表相关 ==========
    [ObservableProperty]
    private ObservableCollection<MonthlyTrendItem> _monthlyTrendData = new();

    [ObservableProperty]
    private ObservableCollection<string> _yAxisLabels = new();

        public indexViewModel()
        {
            // 初始化年份列表（当前年份及前后各2年）
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear - 2; year <= currentYear + 2; year++)
            {
                YearList.Add(year);
            }
            
            // 构造函数中初始化图表
            BuildMonthlyTrendChart();
            
            // 监听产品列表变化
            ProductList.CollectionChanged += (s, e) =>
            {
                // 处理新增项
                if (e.NewItems != null)
                {
                    foreach (ProductItem item in e.NewItems)
                    {
                        item.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(ProductItem.StructureRatio))
                            {
                                UpdateProportionSum();
                            }
                        };
                    }
                }
                
                // 更新占比总和
                UpdateProportionSum();
            };
        }

        // ========== 接口调用相关方法 ==========
        /// <summary>
        /// 当代理商选择变化时调用
        /// </summary>
        public async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {
            long? agentId = agent?.AgentId;

            // 更新显示的名称 - 优先使用业务经理的昵称
            if (agent != null)
            {
                CurrentAgentName = agent.AgentName;
            }
            else
            {
                CurrentAgentName = "未选择";
            }

            CurrentAgentId = agentId;

            // 只有当 agentId 有效时才加载数据
            if (agentId.HasValue && agentId.Value > 0)
            {
                // 根据视图模式加载对应的数据
                if (ViewMode == "yearly")
                {
                    await LoadYearlyDataAsync();
                }
                else
                {
                    await LoadDataAsync();
                }
            }
            else
            {
                ClearData();
                ResetYearlyData();
            }
        }

        /// <summary>
        /// 初始化加载数据
        /// </summary>
        public async Task InitializeDataAsync()
        {
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                await LoadDataAsync();
            }
        }

        // ========== 视图模式切换 ==========
        [RelayCommand]
        private void SetMonthlyView()
        {
            ViewMode = "monthly";
            Logger.Info("切换到月度视图");
        }

        [RelayCommand]
        private void SetYearlyView()
        {
            ViewMode = "yearly";
            Logger.Info("切换到年度视图");
        }

        // ========== 数据导入相关 ==========
        [RelayCommand]
        private async Task ImportExcel()
        {
            Logger.Info("Excel 导入");
            // TODO: 实现 Excel 导入
        }

        [RelayCommand]
        private async Task DownloadImportTemplate()
        {
            Logger.Info("下载导入模板");
            // TODO: 实现模板下载
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        private void ClearData()
        {
            // 月度视图数据
            MonthlyDataList.Clear();
            TargetSales = 0;
            AchievementRate = 0;
            AchievementRatePercent = 0;
            ActualSales = 0;
            NetProfit = 0;
            IsProfitable = true;

            // 基础配置数据
            ConfigTargetSales = 0;
            ActualSalesStatus = "未录入";
            FixedCost = 0;
            BatteryCost = 0;
            RentCost = 0;
            SalaryCost = 0;
            VehicleCost = 0;
            UtilityCost = 0;
            PIncome = 0;
            AcceptanceIncome = 0;
            AfterSalesIncome = 0;

            // 产品配置数据
            ProductList.Clear();

            // 年度视图数据
            YearlyAvgMonthlySales = 0;
            YearlyTotalSales = 0;
            YearlyTotalSalesAmount = 0;
            YearlyTotalCost = 0;
            YearlyTotalProfit = 0;
            YearlySummaryList.Clear();
            YearlyComparisonList.Clear();
        }

        /// <summary>
        /// 刷新页面数据
        /// </summary>
        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载数据");
                ClearData();
                return;
            }

            try
            {
                Logger.Info("开始加载月度明细年度数据...");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                // 先加载销售目标数据
                await LoadSaleTargetDataAsync(agentId, yearMonth);
                
                // 再加载费用数据
                await LoadExpenseDataAsync(agentId, yearMonth);
                
                // 再加载产品结构数据
                await LoadProductStructureDataAsync(agentId, yearMonth);

                // 再加载明细数据
                Logger.Info($"调用接口: /rate/enddetail/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/enddetail/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    if (data != null)
                    {
                        // 更新卡片数据（目标销量和实际销量从销售目标接口获取）
                        AchievementRate = data["achievementRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                            ? data["achievementRate"].ToObject<decimal>() : 0;
                        AchievementRatePercent = (double)AchievementRate * 100;  // 乘以100

                        var netProfitValue = data["adjustedNetProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                            ? data["adjustedNetProfit"].ToObject<decimal>() : 0;
                        NetProfit = netProfitValue;
                        IsProfitable = netProfitValue >= 0;

                        // 先获取 CostRate 值
                        decimal costRate = data["expenseRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null
                            ? data["expenseRate"].ToObject<decimal>() : 0;
                        decimal netProfitRate = data["netProfitRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null
                                ? data["netProfitRate"].ToObject<decimal>() : 0;

                        // 更新月度数据列表
                        MonthlyDataList.Clear();
                        var item = new MonthlyDataItem
                        {
                            AgentName = data["agentName"]?.ToString() ?? CurrentAgentName,
                            Month = SelectedMonth,
                            TargetSales = ConfigTargetSales,
                            AchievementRate = AchievementRate,
                            AchievementRatePercent = AchievementRatePercent,
                            ActualSales = ActualSales,
                            SalesAmount = data["salesAmount"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["salesAmount"].ToObject<decimal>() : 0,
                            TotalCost = data["totalCost"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["totalCost"].ToObject<decimal>() : 0,
                            CostRate = costRate,
                            CostRatePercent = (double)costRate * 100,
                            NetProfit = netProfitValue,
                            NetProfitRate = netProfitRate,
                            NetProfitRatePercent = (double)netProfitRate * 100
                        };
                        MonthlyDataList.Add(item);

                        // 更新状态
                        ActualSalesStatus = ActualSales > 0 ? "已录入" : "未录入";

                        Logger.Success("数据加载成功");
                    }
                    else
                    {
                        Logger.Warning("返回的数据为空");
                        ResetDetailData();
                    }
                }
                else
                {
                    Logger.Warning($"接口返回失败: {response.Message}");
                    ResetDetailData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载数据异常: {ex.Message}", ex);
                ResetDetailData();
            }
        }
        
        /// <summary>
        /// 重置明细数据
        /// </summary>
        private void ResetDetailData()
        {
            AchievementRate = 0;
            AchievementRatePercent = 0;
            NetProfit = 0;
            IsProfitable = false;
            MonthlyDataList.Clear();
            ActualSalesStatus = "未录入";
        }
        
        /// <summary>
        /// 加载产品结构数据
        /// </summary>
        private async Task LoadProductStructureDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Logger.Info($"调用接口: /rate/endstructure/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.GetAsync<List<RateProductStructureDto>>(
                    $"/rate/endstructure/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    ProductList.Clear();
                    foreach (var item in response.Data)
                    {
                        var productItem = new ProductItem
                        {
                            ModelId = item.ModelId,
                            ItemModel = item.ItemModel ?? string.Empty,
                            StructureRatio = item.StructureRatio,
                            RemiumPrice = item.RemiumPrice,
                            RemiumCost = item.RemiumCost,
                            PremiumDiscount = item.PremiumDiscount,
                            PriceAdjustment = item.PriceAdjustment,
                            Commission = item.Commission
                        };
                        ProductList.Add(productItem);
                    }
                    Logger.Success("产品结构数据加载成功");
                }
                else
                {
                    Logger.Warning($"产品结构接口返回失败: {response.Message}");
                    ResetProductStructureData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载产品结构数据异常: {ex.Message}", ex);
                ResetProductStructureData();
            }
        }
        
        /// <summary>
        /// 重置产品结构数据
        /// </summary>
        private void ResetProductStructureData()
        {
            ProductList.Clear();
        }

        /// <summary>
        /// 加载年度数据
        /// </summary>
        private async Task LoadYearlyDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载年度数据");
                ResetYearlyData();
                return;
            }

            try
            {
                // 先重置数据
                ResetYearlyData();
                
                // 并行加载三个接口
                var task1 = LoadYearlyStatsAsync();
                var task2 = LoadYearlySummaryAsync();
                var task3 = LoadYearlyComparisonAsync();
                
                await Task.WhenAll(task1, task2, task3);
                
                Logger.Success("年度数据加载成功");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载年度数据异常: {ex.Message}", ex);
                ResetYearlyData();
            }
        }

        /// <summary>
        /// 加载年度KPI数据
        /// </summary>
        private async Task LoadYearlyStatsAsync()
        {
            try
            {
                Logger.Info($"调用年度KPI接口: /rate/enddetail/getYearlyStats, year={SelectedYear}, agentId={CurrentAgentId.Value}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/enddetail/getYearlyStats?agentId={CurrentAgentId.Value}&year={SelectedYear}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data;
                    YearlyAvgMonthlySales = data["averageMonthlySales"]?.ToObject<long>() ?? 0;
                    YearlyTotalSales = data["yearlyTotalSales"]?.ToObject<long>() ?? 0;
                    YearlyTotalSalesAmount = data["totalSalesAmount"]?.ToObject<decimal>() ?? 0;
                    YearlyTotalCost = data["totalCost"]?.ToObject<decimal>() ?? 0;
                    YearlyTotalProfit = data["totalProfit"]?.ToObject<decimal>() ?? 0;
                    Logger.Success("年度KPI数据加载成功");
                }
                else
                {
                    Logger.Warning($"年度KPI接口返回失败: {response.Message}");
                    // 清空KPI数据
                    YearlyAvgMonthlySales = 0;
                    YearlyTotalSales = 0;
                    YearlyTotalSalesAmount = 0;
                    YearlyTotalCost = 0;
                    YearlyTotalProfit = 0;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载年度KPI数据异常: {ex.Message}", ex);
                // 清空KPI数据
                YearlyAvgMonthlySales = 0;
                YearlyTotalSales = 0;
                YearlyTotalSalesAmount = 0;
                YearlyTotalCost = 0;
                YearlyTotalProfit = 0;
            }
        }

        /// <summary>
        /// 加载年度汇总数据
        /// </summary>
        private async Task LoadYearlySummaryAsync()
        {
            try
            {
                Logger.Info($"调用年度汇总接口: /rate/enddetail/getByAgentIdAndYear, year={SelectedYear}, agentId={CurrentAgentId.Value}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/enddetail/getByAgentIdAndYear?agentId={CurrentAgentId.Value}&year={SelectedYear}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    YearlySummaryList.Clear();
                    var summaryList = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (summaryList != null)
                    {
                        foreach (var item in summaryList)
                        {
                            // 从 yearMonth 中提取月份
                            var yearMonthStr = item["yearMonth"]?.ToString() ?? "";
                            int month = 0;
                            if (!string.IsNullOrEmpty(yearMonthStr) && yearMonthStr.Contains("-"))
                            {
                                var monthPart = yearMonthStr.Split('-')[1];
                                int.TryParse(monthPart, out month);
                            }

                            var summaryItem = new YearlySummaryItem
                            {
                                AgentName = item["agentName"]?.ToString() ?? CurrentAgentName,
                                Month = month,
                                TargetSales = item["targetSales"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["targetSales"].ToObject<long>() : 0,
                                AchievementRate = item["achievementRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["achievementRate"].ToObject<decimal>() : 0,
                                ActualSales = item["actualSales"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["actualSales"].ToObject<long>() : 0,
                                SalesAmount = item["salesAmount"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["salesAmount"].ToObject<decimal>() : 0,
                                TotalCost = item["totalCost"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["totalCost"].ToObject<decimal>() : 0,
                                CostRate = item["expenseRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["expenseRate"].ToObject<decimal>() : 0,
                                NetProfit = item["adjustedNetProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["adjustedNetProfit"].ToObject<decimal>() : 0,
                                NetProfitRate = item["netProfitRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                    ? item["netProfitRate"].ToObject<decimal>() : 0
                            };
                            YearlySummaryList.Add(summaryItem);
                        }
                    }
                    Logger.Success("年度汇总数据加载成功");
                    
                    // 构建图表
                    BuildMonthlyTrendChart();
                }
                else
                {
                    Logger.Warning($"年度汇总接口返回失败: {response.Message}");
                    YearlySummaryList.Clear();
                    BuildMonthlyTrendChart();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载年度汇总数据异常: {ex.Message}", ex);
                YearlySummaryList.Clear();
                BuildMonthlyTrendChart();
            }
        }
        
        /// <summary>
        /// 构建月度趋势柱状图
        /// </summary>
        private void BuildMonthlyTrendChart()
        {
            MonthlyTrendData.Clear();
            YAxisLabels.Clear();
            
            // 找出最大值和最小值
            double maxValue = 100;
            double minValue = 0;
            
            foreach (var item in YearlySummaryList)
            {
                var value = (double)item.NetProfit;
                if (value > maxValue) maxValue = value;
                if (value < minValue) minValue = value;
            }
            
            // Y轴从0开始
            if (minValue > 0) minValue = 0;
            
            // 计算合适的Y轴范围
            var positiveMax = maxValue > 0 ? maxValue : 100;
            var negativeMin = minValue < 0 ? minValue : 0;
            
            // 让最大值更规整
            positiveMax = CeilingToNiceNumber(positiveMax);
            if (negativeMin < 0) negativeMin = FloorToNiceNumber(negativeMin);
            
            // 生成Y轴标签（从上到下）
            var totalRange = positiveMax - negativeMin;
            var step = totalRange / 5;
            
            for (int i = 5; i >= 0; i--)
            {
                var value = negativeMin + step * i;
                YAxisLabels.Add(FormatCurrency(value));
            }
            
            // 生成12个月的数据
            for (int month = 1; month <= 12; month++)
            {
                var item = YearlySummaryList.FirstOrDefault(x => x.Month == month);
                var netProfit = item != null ? (double)item.NetProfit : 0;
                
                // 计算高度，0点位置
                var totalHeight = 240; // 总高度
                var zeroPosition = ((positiveMax) / totalRange) * totalHeight; // 0点的位置（从顶部计算）
                
                double height;
                double topMargin;
                
                if (netProfit >= 0)
                {
                    // 正数：从0点向下延伸
                    height = (netProfit / positiveMax) * zeroPosition;
                    topMargin = zeroPosition - height;
                }
                else
                {
                    // 负数：从0点向上延伸
                    height = (Math.Abs(netProfit) / Math.Abs(negativeMin)) * (totalHeight - zeroPosition);
                    topMargin = zeroPosition;
                }
                
                MonthlyTrendData.Add(new MonthlyTrendItem
                {
                    MonthLabel = $"{month}月",
                    NetProfit = netProfit,
                    Height = Math.Max(height, 0),
                    TopMargin = topMargin,
                    Color = netProfit >= 0 ? "#3B82F6" : "#EF4444", // 正蓝负红
                    TooltipText = $"{month}月: {FormatCurrency(netProfit)}"
                });
            }
        }
        
        /// <summary>
        /// 向上取整到规整数字
        /// </summary>
        private static double CeilingToNiceNumber(double value)
        {
            if (value <= 0) return 100;
            
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            var normalized = value / magnitude;
            
            if (normalized <= 1) return magnitude;
            if (normalized <= 2) return 2 * magnitude;
            if (normalized <= 5) return 5 * magnitude;
            return 10 * magnitude;
        }
        
        /// <summary>
        /// 向下取整到规整数字
        /// </summary>
        private static double FloorToNiceNumber(double value)
        {
            if (value >= 0) return 0;
            
            var absValue = Math.Abs(value);
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(absValue)));
            var normalized = absValue / magnitude;
            
            double result;
            if (normalized <= 1) result = -magnitude;
            else if (normalized <= 2) result = -2 * magnitude;
            else if (normalized <= 5) result = -5 * magnitude;
            else result = -10 * magnitude;
            
            return result;
        }
        
        /// <summary>
        /// 格式化货币显示
        /// </summary>
        private static string FormatCurrency(double value)
        {
            if (Math.Abs(value) >= 10000)
            {
                return $"¥{value / 10000:F2}万";
            }
            return $"¥{value:F0}";
        }

        /// <summary>
        /// 加载年度同比分析数据
        /// </summary>
        private async Task LoadYearlyComparisonAsync()
        {
            try
            {
                int lastYear = SelectedYear - 1;
                Logger.Info($"调用年度同比接口: /rate/enddetail/getYearlyComparison, lastYear={lastYear}, currentYear={SelectedYear}, agentId={CurrentAgentId.Value}");

                var response = await NewApiClient.GetAsync<List<YearlyComparisonItem>>(
                    $"/rate/enddetail/getYearlyComparison?agentId={CurrentAgentId.Value}&lastYear={lastYear}&currentYear={SelectedYear}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    YearlyComparisonList.Clear();
                    foreach (var item in response.Data)
                    {
                        YearlyComparisonList.Add(item);
                    }
                    Logger.Success("年度同比数据加载成功");
                }
                else
                {
                    Logger.Warning($"年度同比接口返回失败: {response.Message}");
                    YearlyComparisonList.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载年度同比数据异常: {ex.Message}", ex);
                YearlyComparisonList.Clear();
            }
        }

        /// <summary>
        /// 重置年度数据
        /// </summary>
        private void ResetYearlyData()
        {
            YearlyAvgMonthlySales = 0;
            YearlyTotalSales = 0;
            YearlyTotalSalesAmount = 0;
            YearlyTotalCost = 0;
            YearlyTotalProfit = 0;
            YearlySummaryList.Clear();
            YearlyComparisonList.Clear();
        }
        
        /// <summary>
        /// 保存单个产品配置
        /// </summary>
        public async Task SaveSingleProductAsync(ProductItem product)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0 || product == null)
            {
                Logger.Warning("参数无效，不保存产品数据");
                return;
            }

            try
            {
                Logger.Info($"保存单个产品配置: {product.ItemModel}");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    modelId = product.ModelId,
                    itemModel = product.ItemModel,
                    structureRatio = product.StructureRatio,
                    remiumPrice = product.RemiumPrice,
                    remiumCost = product.RemiumCost,
                    premiumDiscount = product.PremiumDiscount,
                    priceAdjustment = product.PriceAdjustment,
                    commission = product.Commission
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/endstructure/saveRateProductStructure",
                    requestData)
                    .ConfigureAwait(true);

                if (response.Code == 200)
                {
                    Logger.Success("单个产品配置保存成功！");
                }
                else
                {
                    Logger.Warning($"单个产品保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"保存单个产品配置异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 保存产品配置
        /// </summary>
        public async Task SaveProductStructureAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存产品结构数据");
                return;
            }

            try
            {
                Logger.Info($"保存产品配置，共 {ProductList.Count} 条记录");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = ProductList.Select(p => new
                {
                    modelId = p.ModelId,
                    itemModel = p.ItemModel,
                    structureRatio = p.StructureRatio,
                    remiumPrice = p.RemiumPrice,
                    remiumCost = p.RemiumCost,
                    premiumDiscount = p.PremiumDiscount,
                    priceAdjustment = p.PriceAdjustment,
                    commission = p.Commission
                }).ToList();

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/endstructure/saveRateProductStructure",
                    new
                    {
                        agentId = agentId,
                        yearMonth = yearMonth,
                        list = requestData
                    })
                    .ConfigureAwait(true);

                if (response.Code == 200)
                {
                    Logger.Success("产品配置保存成功！");
                    var box = MessageBoxManager.GetMessageBoxStandard("保存成功", "产品配置保存成功！", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    Logger.Warning($"保存失败: {response.Message}");
                    var box = MessageBoxManager.GetMessageBoxStandard("保存失败", response.Message ?? "保存失败，请重试", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"保存产品配置异常: {ex.Message}", ex);
                var box = MessageBoxManager.GetMessageBoxStandard("保存失败", $"保存失败：{ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

        /// <summary>
        /// 加载销售目标数据
        /// </summary>
        private async Task LoadSaleTargetDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Logger.Info($"调用接口: /rate/endsaletarget/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/endsaletarget/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    if (data != null)
                    {
                        ConfigTargetSales = data["targetSales"]?.ToObject<long>() ?? 0;
                        ActualSales = data["actualSales"]?.ToObject<long>() ?? 0;
                        Logger.Success("销售目标数据加载成功");
                    }
                    else
                    {
                        ResetSaleTargetData();
                    }
                }
                else
                {
                    Logger.Warning($"销售目标接口返回失败: {response.Message}");
                    ResetSaleTargetData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载销售目标数据异常: {ex.Message}", ex);
                ResetSaleTargetData();
            }
        }
        
        /// <summary>
        /// 重置销售目标数据
        /// </summary>
        private void ResetSaleTargetData()
        {
            ConfigTargetSales = 0;
            ActualSales = 0;
            ActualSalesStatus = "未录入";
        }
        
        /// <summary>
        /// 加载费用数据
        /// </summary>
        private async Task LoadExpenseDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Logger.Info($"调用接口: /rate/endexpense/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");
                
                var response = await NewApiClient.GetAsync<List<ExpenseDto>>(
                    $"/rate/endexpense/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);
                
                if (response.Code == 200 && response.Data != null)
                {
                    // 先重置所有费用数据
                    ResetExpenseData();

                    foreach (var item in response.Data)
                    {
                        switch (item.ExpenseType)
                        {
                            case "投入电池资金成本（贷款的资金利息）":
                                BatteryCost = item.Amount;
                                break;
                            case "门面或仓库租金":
                                RentCost = item.Amount;
                                break;
                            case "固定工资（内勤、业务、售后等）":
                                SalaryCost = item.Amount;
                                break;
                            case "车辆油费、保险":
                                VehicleCost = item.Amount;
                                break;
                            case "水电、招待费":
                                UtilityCost = item.Amount;
                                break;
                            case "P":
                                PIncome = item.Amount;
                                break;
                            case "承兑收益":
                                AcceptanceIncome = item.Amount;
                                break;
                            case "售后收益（含零售以旧换新利润收益）":
                                AfterSalesIncome = item.Amount;
                                break;
                        }
                    }
                    Logger.Success("费用数据加载成功");
                }
                else
                {
                    Logger.Warning($"费用接口返回失败: {response.Message}");
                    ResetExpenseData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载费用数据异常: {ex.Message}", ex);
                ResetExpenseData();
            }
        }
        
        /// <summary>
        /// 重置费用数据
        /// </summary>
        private void ResetExpenseData()
        {
            BatteryCost = 0;
            RentCost = 0;
            SalaryCost = 0;
            VehicleCost = 0;
            UtilityCost = 0;
            PIncome = 0;
            AcceptanceIncome = 0;
            AfterSalesIncome = 0;
        }
        
        /// <summary>
        /// 保存费用数据（供外部调用）
        /// </summary>
        public async Task SaveExpenseAsync(string expenseType, decimal amount)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存费用数据");
                return;
            }
            
            try
            {
                Logger.Info($"保存费用数据: expenseType={expenseType}, amount={amount}");
                
                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    expenseType = expenseType,
                    amount = amount
                };
                
                Logger.Info($"调用接口: /rate/endexpense/saveRateMonthlyExpense");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/endexpense/saveRateMonthlyExpense",
                    requestData)
                    .ConfigureAwait(true);
                
                if (response.Code == 200)
                {
                    Logger.Success("费用数据保存成功");
                }
                else
                {
                    Logger.Warning($"费用保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"费用保存异常: {ex.Message}", ex);
            }
        }

        // ========== 配置相关方法 ==========
        
        /// <summary>
        /// 公共保存方法，用于外部调用（如失去焦点时）
        /// </summary>
        public async Task SaveSaleTargetAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存数据");
                return;
            }

            try
            {
                Logger.Info("自动保存销售目标...");

                // 保存用户输入的当前值
                long tempTargetSales = ConfigTargetSales;
                long tempActualSales = ActualSales;

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    targetSales = tempTargetSales,
                    actualSales = tempActualSales
                };

                Logger.Info($"调用接口: /rate/endsaletarget/saveRateSalesTarget, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/endsaletarget/saveRateSalesTarget",
                    requestData)
                    .ConfigureAwait(true);

                if (response.Code == 200)
                {
                    // 保存成功后，更新KPI卡片的数据（直接使用用户输入的值）
                    TargetSales = tempTargetSales;
                    // ActualSales 不需要再赋值，因为它已经是用户输入的值，而且界面也绑定了它
                    
                    // 不重新加载所有数据，只重新加载其他必要的数据，避免覆盖ActualSales
                    // 先加载费用和产品结构数据
                    await LoadExpenseDataAsync(agentId, yearMonth);
                    await LoadProductStructureDataAsync(agentId, yearMonth);
                    
                    // 重新加载明细数据
                    await ReloadDetailDataAsync(agentId, yearMonth, tempTargetSales, tempActualSales);
                    
                    Logger.Success("销售目标数据保存成功！");
                }
                else
                {
                    Logger.Warning($"保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"保存异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 重新加载明细数据，使用已知的目标销量和实际销量
        /// </summary>
        private async Task ReloadDetailDataAsync(long agentId, string yearMonth, long targetSales, long actualSales)
        {
            try
            {
                Logger.Info($"调用接口: /rate/enddetail/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/enddetail/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);

                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    if (data != null)
                    {
                        // 更新卡片数据
                        AchievementRate = data["achievementRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                            ? data["achievementRate"].ToObject<decimal>() : 0;
                        AchievementRatePercent = (double)AchievementRate * 100;

                        var netProfitValue = data["adjustedNetProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                            ? data["adjustedNetProfit"].ToObject<decimal>() : 0;
                        NetProfit = netProfitValue;
                        IsProfitable = netProfitValue >= 0;

                        // 更新月度数据列表，使用已知的targetSales和actualSales
                        MonthlyDataList.Clear();
                        var item = new MonthlyDataItem
                        {
                            AgentName = data["agentName"]?.ToString() ?? CurrentAgentName,
                            Month = SelectedMonth,
                            TargetSales = targetSales,
                            AchievementRate = AchievementRate,
                            AchievementRatePercent = AchievementRatePercent,
                            ActualSales = actualSales,
                            SalesAmount = data["salesAmount"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["salesAmount"].ToObject<decimal>() : 0,
                            TotalCost = data["totalCost"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["totalCost"].ToObject<decimal>() : 0,
                            CostRate = data["expenseRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["expenseRate"].ToObject<decimal>() : 0,
                            NetProfit = netProfitValue,
                            NetProfitRate = data["netProfitRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null 
                                ? data["netProfitRate"].ToObject<decimal>() : 0
                        };
                        MonthlyDataList.Add(item);

                        // 更新状态
                        ActualSalesStatus = actualSales > 0 ? "已录入" : "未录入";

                        Logger.Success("数据加载成功");
                    }
                    else
                    {
                        Logger.Warning("返回的数据为空");
                        ResetDetailData();
                    }
                }
                else
                {
                    Logger.Warning($"接口返回失败: {response.Message}");
                    ResetDetailData();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载数据异常: {ex.Message}", ex);
                ResetDetailData();
            }
        }

        [RelayCommand]
        private async Task SaveBasicConfig()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存数据");
                return;
            }

            try
            {
                Logger.Info("保存基础配置...");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    targetSales = ConfigTargetSales,
                    actualSales = ActualSales
                };

                Logger.Info($"调用接口: /rate/endsaletarget/saveRateSalesTarget, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/endsaletarget/saveRateSalesTarget",
                    requestData)
                    .ConfigureAwait(true);

                if (response.Code == 200)
                {
                    // 保存成功后，更新KPI卡片的数据
                    TargetSales = ConfigTargetSales;
                    
                    // 重新加载数据以更新界面
                    await LoadDataAsync();
                    
                    Logger.Success("基础配置保存成功！");
                    
                    var box = MessageBoxManager.GetMessageBoxStandard("保存成功", "销售目标数据保存成功！", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    Logger.Warning($"保存失败: {response.Message}");
                    var box = MessageBoxManager.GetMessageBoxStandard("保存失败", response.Message ?? "保存失败，请重试", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"保存异常: {ex.Message}", ex);
                var box = MessageBoxManager.GetMessageBoxStandard("保存失败", $"保存失败：{ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

        [RelayCommand]
        private async Task SaveProductConfig()
        {
            Logger.Info("保存产品配置...");
            await SaveProductStructureAsync();
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            try
            {
                Logger.Info("开始添加产品...");
                
                // 检查是否选择了代理商
                if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
                {
                    Logger.Warning("未选择代理商，无法添加产品");
                    StatusMessage = "请先选择代理商";
                    return;
                }
                
                // 显示产品选择器对话框
                var dialog = new AgentManagement.Avalonia.Controls.ProductSelectorDialog();
                var result = await dialog.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow!);
                
                if (result != null)
                {
                    Logger.Info($"产品选择器返回: Model={result.Model}, ItemId={result.ItemId}");
                    
                    // 创建新的产品项
                    var newProduct = new ProductItem
                    {
                        ModelId = result.ItemId,
                        ItemModel = result.Model,
                        StructureRatio = (decimal)result.Proportion,
                        RemiumPrice = (decimal)result.GroupPrice,
                        RemiumCost = (decimal)result.PurchasePrice,
                        Commission = (decimal)result.Commission,
                        PremiumDiscount = 0.0m,
                        PriceAdjustment = 0.0m
                    };

                    //保存到数据库
                    await SaveSingleProductAsync(newProduct);

                    Logger.Info($"创建 ProductItem 成功，准备添加到列表");
                    
                    // 检查是否在 UI 线程
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Logger.Info("当前在 UI 线程，直接添加");
                        ProductList.Add(newProduct);
                        
                        Logger.Success($"已添加产品: {result.Model}，当前产品数量: {ProductList.Count}");
                        StatusMessage = $"已添加产品: {result.Model}";
                    }
                    else
                    {
                        Logger.Info("不在 UI 线程，使用 InvokeAsync");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ProductList.Add(newProduct);
                            
                            Logger.Success($"异步添加产品: {result.Model}，当前产品数量: {ProductList.Count}");
                            StatusMessage = $"已添加产品: {result.Model}";
                        });
                    }
                }
                else
                {
                    Logger.Info("用户取消了产品选择");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"添加产品异常: {ex.Message}", ex);
                StatusMessage = $"添加产品失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 更新产品占比总和显示
        /// </summary>
        private void UpdateProportionSum()
        {
            var sum = ProductList.Sum(p => (double)p.StructureRatio);
            ProportionSumText = $"产品占比总和: {sum:F2}";
            
            if (Math.Abs(sum - 1.0) < 0.0001)
            {
                // 正好1
                ProportionSumBackground = "#D1FAE5"; // green-100
                ProportionSumBorder = "#BBF7D0"; // green-200
                ProportionSumForeground = "#059669"; // green-600
                ProportionSumText = $"✓ {ProportionSumText} 已达标";
            }
            else if (sum > 1.0)
            {
                // 超过1
                ProportionSumBackground = "#FEE2E2"; // red-100
                ProportionSumBorder = "#FECACA"; // red-200
                ProportionSumForeground = "#DC2626"; // red-600
                ProportionSumText = $"✗ {ProportionSumText} 超出{(sum - 1.0):F4}";
            }
            else
            {
                // 不足1
                ProportionSumBackground = "#FEF3C7"; // amber-100
                ProportionSumBorder = "#FDE68A"; // amber-200
                ProportionSumForeground = "#D97706"; // amber-600
                ProportionSumText = $"⚠ {ProportionSumText} 还差{(1.0 - sum):F4}";
            }
        }

        [RelayCommand]
        private void DeleteProduct(ProductItem product)
        {
            if (product != null && ProductList.Contains(product))
            {
                ProductList.Remove(product);
                Logger.Info($"已删除产品: {product.ItemModel}");
            }
        }

        [RelayCommand]
        private void Calculate()
        {
            Logger.Info("开始计算盈亏达成率...");
            // TODO: 实现计算逻辑
            Logger.Success("计算完成！");
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        [RelayCommand]
        private async Task ExportData()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不导出数据");
                return;
            }

            try
            {
                Logger.Info("开始导出数据...");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth
                };

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/enddetail/export",
                    requestData);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = $"enddetail_export_{yearMonth}.xlsx";

                    // 显示保存文件对话框，让用户选择保存位置
                    var filePath = await FileService.ShowSaveFileDialogAsync(
                        "保存数据",
                        fileName,
                        "Excel", "All");

                    if (string.IsNullOrEmpty(filePath))
                    {
                        // 用户取消了保存
                        Logger.Info("用户取消了保存操作");
                        return;
                    }

                    // 写入文件
                    System.IO.File.WriteAllBytes(filePath, fileBytes);

                    Logger.Success($"导出成功！保存位置: {filePath}");

                    var box = MessageBoxManager.GetMessageBoxStandard("导出成功", $"文件已成功保存到：\n{filePath}", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    Logger.Warning("导出失败，未获取到文件");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"导出异常: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private async Task CompareMonthlyData()
        {
            try
            {
                if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
                {
                    Logger.Warning("未选择代理商，无法进行对比");
                    var box = MessageBoxManager.GetMessageBoxStandard("提示", "请先选择代理商", ButtonEnum.Ok);
                    await box.ShowAsync();
                    return;
                }

                Logger.Info("点击月度分析/预测对比按钮");
                
                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var dialog = new AgentManagement.Avalonia.Views.rate.enddetail.MonthlyCompareDialog();
                var viewModel = new MonthlyCompareDialogViewModel(agentId, yearMonth);
                dialog.DataContext = viewModel;
                
                await viewModel.LoadDataAsync();
                await dialog.ShowDialog(App.MainWindow!);
            }
            catch (Exception ex)
            {
                Logger.Error($"月度分析/预测对比异常: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        public async Task DownloadTemplateAsync()
        {
            try
            {
                StatusMessage = "正在下载模板...";
                Logger.Info("开始下载销售目标导入模板");

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/saletarget/importTemplate",null);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = "saletarget_import_template.xlsx";

                    var filePath = await FileService.ShowSaveFileDialogAsync(
                        "保存模板",
                        fileName,
                        "Excel", "All");

                    if (string.IsNullOrEmpty(filePath))
                    {
                        Logger.Info("用户取消了保存操作");
                        StatusMessage = "已取消";
                        return;
                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(filePath, fileBytes);

                        Logger.Success($"模板下载成功！保存位置: {filePath}");
                        StatusMessage = $"模板下载成功！文件已保存到：{filePath}";

                        var box = MessageBoxManager.GetMessageBoxStandard("下载成功", $"模板已成功保存到：\n{filePath}", ButtonEnum.Ok);
                        await box.ShowAsync();
                    }
                }
                else
                {
                    StatusMessage = "下载失败：未获取到文件数据";
                    Logger.Warning("模板下载失败：未获取到文件数据");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"下载模板失败: {ex.Message}";
                Logger.Error($"下载模板异常: {ex.Message}", ex);
            }
        }
    }

    public class MonthlyDataItem
    {
        public string AgentName { get; set; } = string.Empty;
        public int Month { get; set; }
        public long TargetSales { get; set; }
        public decimal AchievementRate { get; set; }

        public double AchievementRatePercent { get; set; }
        public double CostRatePercent { get; set; }
        public double NetProfitRatePercent { get; set; }
        public long ActualSales { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostRate { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitRate { get; set; }
    }

    public partial class ProductItem : ObservableObject
    {
        [ObservableProperty]
        private long? _modelId;

        [ObservableProperty]
        private string _itemModel = string.Empty;

        [ObservableProperty]
        private decimal _structureRatio;

        [ObservableProperty]
        private decimal _remiumPrice;

        [ObservableProperty]
        private decimal _remiumCost;

        [ObservableProperty]
        private decimal _premiumDiscount;

        [ObservableProperty]
        private decimal _priceAdjustment;

        [ObservableProperty]
        private decimal _commission;
    }
    
    public partial class RateProductStructureDto
    {
        public long? StructureId { get; set; }
        public long? ModelId { get; set; }
        public string ItemModel { get; set; }
        public long? AgentId { get; set; }
        public string AgentName { get; set; }
        public string YearMonth { get; set; }
        public decimal StructureRatio { get; set; }
        public decimal RemiumPrice { get; set; }
        public decimal RemiumCost { get; set; }
        public decimal PremiumDiscount { get; set; }
        public decimal PriceAdjustment { get; set; }
        public decimal Commission { get; set; }
    }

    public class YearlySummaryItem
    {
        public string AgentName { get; set; } = string.Empty;
        public int Month { get; set; }
        public long TargetSales { get; set; }
        public decimal AchievementRate { get; set; }
        public long ActualSales { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CostRate { get; set; }
        public decimal NetProfit { get; set; }
        public decimal NetProfitRate { get; set; }
    }

    public class YearlyComparisonItem
    {
        public string Month { get; set; }
        public decimal LastYearNetProfit { get; set; }
        public decimal CurrentYearNetProfit { get; set; }
        public decimal NetProfitDifference { get; set; }
        public decimal LastYearAchievementRate { get; set; }
        public decimal CurrentYearAchievementRate { get; set; }
        public decimal AchievementRateDifference { get; set; }
    }
    
    public partial class ExpenseDto
    {
        public string ExpenseType { get; set; }
        public decimal Amount { get; set; }
    }
    
    public partial class MonthlyTrendItem : ObservableObject
    {
        [ObservableProperty]
        private string _monthLabel = string.Empty;
        
        [ObservableProperty]
        private double _netProfit;
        
        [ObservableProperty]
        private double _height;
        
        [ObservableProperty]
        private double _topMargin;
        
        [ObservableProperty]
        private string _color = string.Empty;
        
        [ObservableProperty]
        private string _tooltipText = string.Empty;
    }
}
