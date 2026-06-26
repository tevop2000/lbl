using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.ViewModels.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.ViewModels.rate.detailyear
{
    public class YearDataDetail : ObservableObject
    {
        private string _agentName;
        public string AgentName
        {
            get => _agentName;
            set => SetProperty(ref _agentName, value);
        }

        private int _year;
        public int Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        private long _annualTargetSales;
        public long AnnualTargetSales
        {
            get => _annualTargetSales;
            set => SetProperty(ref _annualTargetSales, value);
        }

        private string _achievementRate;
        public string AchievementRate
        {
            get => _achievementRate;
            set => SetProperty(ref _achievementRate, value);
        }

        private double _achievementRatePercent;
        public double AchievementRatePercent
        {
            get => _achievementRatePercent;
            set => SetProperty(ref _achievementRatePercent, value);
        }
        private long _achievementSales;
        public long AchievementSales
        {
            get => _achievementSales;
            set => SetProperty(ref _achievementSales, value);
        }

        private long _improvedSales;
        public long ImprovedSales
        {
            get => _improvedSales;
            set => SetProperty(ref _improvedSales, value);
        }

        private decimal _salesGrowthMargin;
        public decimal SalesGrowthMargin
        {
            get => _salesGrowthMargin;
            set => SetProperty(ref _salesGrowthMargin, value);
        }

        private decimal _structureOptimizationMargin;
        public decimal StructureOptimizationMargin
        {
            get => _structureOptimizationMargin;
            set => SetProperty(ref _structureOptimizationMargin, value);
        }

        private decimal _premiumMargin;
        public decimal PremiumMargin
        {
            get => _premiumMargin;
            set => SetProperty(ref _premiumMargin, value);
        }

        private decimal _totalExcessMargin;
        public decimal TotalExcessMargin
        {
            get => _totalExcessMargin;
            set => SetProperty(ref _totalExcessMargin, value);
        }

        private decimal _adjustedNetProfit;
        public decimal AdjustedNetProfit
        {
            get => _adjustedNetProfit;
            set => SetProperty(ref _adjustedNetProfit, value);
        }

        private decimal _totalCommission;
        public decimal TotalCommission
        {
            get => _totalCommission;
            set => SetProperty(ref _totalCommission, value);
        }

        private string _totalExpenses;
        public string TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }
    }

    public class HistoricalYearDataDetail : YearDataDetail
    {
        private long _versionId;
        public long VersionId
        {
            get => _versionId;
            set => SetProperty(ref _versionId, value);
        }

        private long _agentId;
        public long AgentId
        {
            get => _agentId;
            set => SetProperty(ref _agentId, value);
        }

        private string _yearMonth;
        public string YearMonth
        {
            get => _yearMonth;
            set => SetProperty(ref _yearMonth, value);
        }
    }

    public partial class indexViewModel : ViewModelBase
    {
        [ObservableProperty]
        private long _targetSales;

        [ObservableProperty]
        private string _achievementRate;

        [ObservableProperty]
        private double _achievementRatePercent = 0;

        [ObservableProperty]
        private long _breakSales;

        [ObservableProperty]
        private double _progressBarWidth;

        [ObservableProperty]
        private decimal _adjustedNetProfit;

        [ObservableProperty]
        private bool _isProfitable = true;

        [ObservableProperty]
        private string _configTargetSales;

        [ObservableProperty]
        private double _growthRate;

        [ObservableProperty]
        private double _progressValue = 57.7;

        [ObservableProperty]
        private string _agentName = "";

        [ObservableProperty]
        private string _yearMonth;

        [ObservableProperty]
        private decimal _fixedCost;

        [ObservableProperty]
        private decimal _rent;

        [ObservableProperty]
        private decimal _salary;

        [ObservableProperty]
        private decimal _vehicleCost;

        [ObservableProperty]
        private decimal _waterElectricity;

        [ObservableProperty]
        private decimal _pIncome;

        [ObservableProperty]
        private decimal _acceptanceIncome;

        [ObservableProperty]
        private decimal _afterSalesIncome;

        [ObservableProperty]
        private long? _currentAgentId = null;

        [ObservableProperty]
        private string _currentAgentName = "未选择";

        [ObservableProperty]
        private int _selectedYear;
        
        partial void OnSelectedYearChanged(int value)
        {
            Logger.Info($"[DetailYear] OnSelectedYearChanged: year={value}, CurrentAgentId={CurrentAgentId}, SelectedTabIndex={SelectedTabIndex}");
            System.Diagnostics.Debug.WriteLine($"[DetailYear] OnSelectedYearChanged: year={value}, CurrentAgentId={CurrentAgentId}, SelectedTabIndex={SelectedTabIndex}");
            
            YearMonth = value.ToString();
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                Logger.Info($"[DetailYear] 开始加载当前方案数据，年份: {value}");
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 开始加载当前方案数据，年份: {value}");
                _ = LoadDataAsync();
                
                // 如果当前显示的是历史方案Tab，同时刷新历史数据
                if (SelectedTabIndex == 1)
                {
                    Logger.Info($"[DetailYear] 当前为历史方案Tab，开始加载历史数据，年份: {value}");
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 当前为历史方案Tab，开始加载历史数据，年份: {value}");
                    _ = LoadHistoricalYearDataAsync(CurrentAgentId.Value, $"{value}");
                }
            }
            else
            {
                Logger.Warning($"[DetailYear] 当前代理商ID无效，跳过数据加载。CurrentAgentId={CurrentAgentId}");
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 当前代理商ID无效，跳过数据加载。CurrentAgentId={CurrentAgentId}");
            }
        }

        public ObservableCollection<YearDataDetail> YearDataDetails { get; } = new ObservableCollection<YearDataDetail>();
        
        public ObservableCollection<HistoricalYearDataDetail> HistoricalYearDataDetails { get; } = new ObservableCollection<HistoricalYearDataDetail>();
        
        [ObservableProperty]
        private int _selectedTabIndex = 0;
        
        partial void OnSelectedTabIndexChanged(int value)
        {
            if (value == 1 && CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadHistoricalYearDataAsync(CurrentAgentId.Value, $"{SelectedYear}");
            }
        }
        
        public ObservableCollection<ProductStructureDto> ProductStructures { get; } = new ObservableCollection<ProductStructureDto>();
        
        public ObservableCollection<ExpenseItemViewModel> FixedCostItems { get; } = new ObservableCollection<ExpenseItemViewModel>();
        
        public ObservableCollection<ExpenseItemViewModel> IncomeItems { get; } = new ObservableCollection<ExpenseItemViewModel>();
        
        // 产品占比总和显示
        [ObservableProperty]
        private string _proportionSumText = "产品占比总和: 0.00";
        
        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4"; // green-50
        
        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A"; // green-600

        // 结构优化总和显示
        [ObservableProperty]
        private string _optimizationSumText = "结构优化总和: 0.00";
        
        [ObservableProperty]
        private string _optimizationSumBackground = "#D1FAE5"; // green-100
        
        [ObservableProperty]
        private string _optimizationSumBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _optimizationSumForeground = "#059669"; // green-600
        
        // 销售量达成率显示
        [ObservableProperty]
        private string _achievementRateText = "销售量达成率: 100.00%";
        
        [ObservableProperty]
        private string _achievementRateBackground = "#D1FAE5"; // green-100
        
        [ObservableProperty]
        private string _achievementRateBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _achievementRateForeground = "#059669"; // green-600

        public indexViewModel()
        {
            // 设置默认年份（当前年份+1）
            int currentYear = DateTime.Now.Year;
            _selectedYear = currentYear + 1;
            _yearMonth = _selectedYear.ToString();
            
            // 初始化销售量达成率
            UpdateAchievementRate();
            
            // 构造函数中不加载数据，等待选择器初始化后回调
            
            // 监听产品结构列表变化
            ProductStructures.CollectionChanged += (s, e) =>
            {
                // 处理新增项
                if (e.NewItems != null)
                {
                    foreach (ProductStructureDto item in e.NewItems)
                    {
                        item.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(ProductStructureDto.StructureRatio))
                            {
                                UpdateProportionSum();
                            }
                            if (args.PropertyName == nameof(ProductStructureDto.PriceAdjustment))
                            {
                                UpdateOptimizationSum();
                            }
                        };
                    }
                }
                
                // 更新占比总和和结构优化总和
                UpdateProportionSum();
                UpdateOptimizationSum();
            };
        }
        
        partial void OnGrowthRateChanged(double value)
        {
            UpdateAchievementRate();
        }
        
        private void UpdateAchievementRate()
        {
            double achievementRate = 100 + GrowthRate;
            AchievementRateText = $"销售量达成率: {achievementRate:F2}%";
            
            if (Math.Abs(achievementRate - 100) < 0.005)
            {
                // 正好100%
                AchievementRateBackground = "#D1FAE5"; // green-100
                AchievementRateBorder = "#BBF7D0"; // green-200
                AchievementRateForeground = "#059669"; // green-600
                AchievementRateText = $"✓ {AchievementRateText}";
            }
            else if (achievementRate > 100)
            {
                // 超过100%
                AchievementRateBackground = "#D1FAE5"; // green-100
                AchievementRateBorder = "#BBF7D0"; // green-200
                AchievementRateForeground = "#059669"; // green-600
                AchievementRateText = $"✓ {AchievementRateText}";
            }
            else
            {
                // 不足100%
                AchievementRateBackground = "#FEF3C7"; // amber-100
                AchievementRateBorder = "#FDE68A"; // amber-200
                AchievementRateForeground = "#D97706"; // amber-600
                AchievementRateText = $"⚠ {AchievementRateText}";
            }
        }

        /// <summary>
        /// 当代理商选择变化时调用
        /// </summary>
        public async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {
            long? agentId = agent?.AgentId;

            // 更新显示的名称   
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
                await LoadDataAsync();
            }
            else
            {
                // 清空所有数据
                ClearData();
            }
        }

        /// <summary>
        /// 初始化加载数据（当页面首次加载时调用）
        /// </summary>
        public async Task InitializeDataAsync()
        {
            // 只有当 CurrentAgentId 有效时才加载数据
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                await LoadDataAsync();
            }
        }

        /// <summary>
        /// 切换年份
        /// </summary>
        [RelayCommand]
        private void SelectYear(int year)
        {
            SelectedYear = year;
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        private void ClearData()
        {
            YearDataDetails.Clear();
            ProductStructures.Clear();
            TargetSales = 0;
            AchievementRate = "0%";
            AchievementRatePercent = 0;
            BreakSales = 0;
            AdjustedNetProfit = 0;
            IsProfitable = true;
            ConfigTargetSales = "0";
            GrowthRate = 0;
            ProgressValue = 0;
            ProgressBarWidth = 0;
            FixedCost = 0;
            Rent = 0;
            Salary = 0;
            VehicleCost = 0;
            WaterElectricity = 0;
            PIncome = 0;
            AcceptanceIncome = 0;
            AfterSalesIncome = 0;
        }

        /// <summary>
        /// 刷新页面数据
        /// </summary>
        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }

        /// <summary>
        /// 加载所有数据
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载数据");
                return;
            }

            await Task.WhenAll(
                LoadDetailYearDataAsync(),
                LoadConfigDataAsync(),
                LoadExpenseYearDataAsync(CurrentAgentId.Value, SelectedYear.ToString()),
                LoadProductStructureDataAsync(CurrentAgentId.Value, SelectedYear.ToString())
            );
        }
       
        public async Task LoadConfigDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载配置数据");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("[DetailYear] 开始加载配置数据");
                
                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/saletargetyear/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 配置数据响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    
                    if (data != null)
                    {
                        ConfigTargetSales = (data["targetSales"]?.ToObject<long>() ?? 0).ToString();
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] ConfigTargetSales: {ConfigTargetSales}");
                        
                        GrowthRate = data["growthRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? (double)data["growthRate"].ToObject<decimal>() : 0;
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] GrowthRate: {GrowthRate}");
                    }
                    else
                    {
                        // 数据为空，重置配置
                        ResetConfigData();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 加载配置数据失败: {response.Message}");
                    // 加载失败，重置配置
                    ResetConfigData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 加载配置数据异常: {ex.Message}");
                // 异常，重置配置
                ResetConfigData();
            }
        }

        /// <summary>
        /// 重置配置数据
        /// </summary>
        private void ResetConfigData()
        {
            ConfigTargetSales = "0";
            GrowthRate = 0;
        }

        public async Task LoadDetailYearDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载年度盈亏预测数据");
                return;
            }

            try
            {
                StatusMessage = "正在加载年度盈亏预测数据...";
                System.Diagnostics.Debug.WriteLine("[DetailYear] 开始加载年度盈亏预测数据");
                
                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 请求参数: agentId={agentId}, yearMonth={yearMonth}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/detailyear/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 响应数据: {data?.ToString()}");
                    
                    if (data != null)
                    {
                        TargetSales = data["targetSales"]?.ToObject<long>() ?? 0;
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] TargetSales: {TargetSales}");
                        
                        var achievementRate = data["achievementRate"]?.ToObject<double>() ?? 0;
                        AchievementRate = $"{achievementRate:F2}%";

                        AchievementRatePercent = (double)achievementRate * 100;  // 乘以100

                        ProgressValue = achievementRate;
                        ProgressBarWidth = achievementRate;
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] AchievementRate: {AchievementRate}, ProgressValue: {ProgressValue}, ProgressBarWidth: {ProgressBarWidth}");
                        
                        BreakSales = data["breakSales"]?.ToObject<long?>() ?? 0;
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] BreakSales: {BreakSales}");
                        
                        var adjustedNetProfit = data["adjustedNetProfit"]?.ToObject<decimal>() ?? 0;
                        AdjustedNetProfit = adjustedNetProfit;
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] AdjustedNetProfit: {AdjustedNetProfit}");
                        
                        IsProfitable = adjustedNetProfit >= 0;
                        AgentName = data["agentName"]?.ToString() ?? "";
                        YearMonth = data["yearMonth"]?.ToString() ?? (DateTime.Now.Year + 1).ToString();
                        
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] AgentName: {AgentName}, YearMonth: {YearMonth}");
                        
                        YearDataDetails.Clear();
                        
                        var salesGrowth = data["salesGrowthProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["salesGrowthProfit"].ToObject<decimal>() : 0;
                        var structureOptimize = data["structureOptimizeProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["structureOptimizeProfit"].ToObject<decimal>() : 0;
                        var premium = data["premiumProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["premiumProfit"].ToObject<decimal>() : 0;
                        var totalExtra = data["totalExtraProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["totalExtraProfit"].ToObject<decimal>() : 0;
                        var adjustedNet = data["adjustedNetProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["adjustedNetProfit"].ToObject<decimal>() : 0;
                        
                        System.Diagnostics.Debug.WriteLine($"[DetailYear] 解析的数据: salesGrowth={salesGrowth}, structureOptimize={structureOptimize}, premium={premium}, totalExtra={totalExtra}, adjustedNet={adjustedNet}");
                        
                        YearDataDetails.Add(new YearDataDetail
                        {
                            AgentName = data["agentName"]?.ToString() ?? "",
                            Year = int.TryParse(data["yearMonth"]?.ToString(), out int y) ? y : DateTime.Now.Year + 1,
                            AnnualTargetSales = data["targetSales"]?.ToObject<long>() ?? 0,
                            AchievementRate = $"{achievementRate:F2}%",
                            AchievementRatePercent = AchievementRatePercent,
                            AchievementSales = data["breakSales"]?.ToObject<long?>() ?? 0,
                            ImprovedSales = data["improvedSales"]?.ToObject<long>() ?? 0,
                            SalesGrowthMargin = salesGrowth,
                            StructureOptimizationMargin = structureOptimize,
                            PremiumMargin = premium,
                            TotalExcessMargin = totalExtra,
                            AdjustedNetProfit = adjustedNet,
                            TotalCommission = data["totalCommission"]?.HasValues == true ? data["totalCommission"].ToObject<decimal>() : 0
                        });
                        
                        System.Diagnostics.Debug.WriteLine("[DetailYear] YearDataDetails count: " + YearDataDetails.Count);
                        
                        StatusMessage = "数据加载成功";
                        System.Diagnostics.Debug.WriteLine("[DetailYear] 数据加载成功");
                    }
                    else
                    {
                        StatusMessage = "响应数据为空";
                        System.Diagnostics.Debug.WriteLine("[DetailYear] 响应数据为空，重置数据");
                        ResetDetailYearData();
                    }
                }
                else
                {
                    StatusMessage = $"加载失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 加载失败: {response.Message}");
                    ResetDetailYearData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 异常堆栈: {ex.StackTrace}");
                ResetDetailYearData();
            }
        }

        /// <summary>
        /// 重置年度盈亏预测数据
        /// </summary>
        private void ResetDetailYearData()
        {
            TargetSales = 0;
            AchievementRate = "0%";
            AchievementRatePercent = 0;
            ProgressValue = 0;
            ProgressBarWidth = 0;
            BreakSales = 0;
            AdjustedNetProfit = 0;
            IsProfitable = true;
            AgentName = "未选择";
            YearDataDetails.Clear();
        }

        /// <summary>
        /// 加载历史方案年度数据
        /// </summary>
        private async Task LoadHistoricalYearDataAsync(long agentId, string year)
        {
            try
            {
                StatusMessage = "正在加载历史方案数据...";
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/detailyearhistory/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={year}")
                    .ConfigureAwait(true);
                
                if (response.Code == 200 && response.Data != null)
                {
                    HistoricalYearDataDetails.Clear();
                    
                    var jArray = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (jArray != null)
                    {
                        foreach (var item in jArray)
                        {
                            var dataItem = item as Newtonsoft.Json.Linq.JObject;
                            if (dataItem != null)
                            {
                                AddHistoricalYearDataItem(dataItem);
                            }
                        }
                        
                        StatusMessage = $"加载成功，共 {jArray.Count} 条历史记录";
                    }
                }
                else
                {
                    StatusMessage = "暂无历史方案数据";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载历史方案失败: {ex.Message}";
                Logger.Error($"加载历史方案数据异常: {ex}");
            }
        }

        private void AddHistoricalYearDataItem(Newtonsoft.Json.Linq.JObject data)
        {
            var yearMonthStr = data["yearMonth"]?.ToString() ?? "";
            int year = SelectedYear;
            if (!string.IsNullOrEmpty(yearMonthStr))
            {
                int.TryParse(yearMonthStr, out year);
            }

            var achievementRate = data["achievementRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["achievementRate"].ToObject<decimal>() : 0;
            var salesGrowth = data["salesGrowthProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["salesGrowthProfit"].ToObject<decimal>() : 0;
            var structureOptimize = data["structureOptimizeProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["structureOptimizeProfit"].ToObject<decimal>() : 0;
            var premium = data["premiumProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["premiumProfit"].ToObject<decimal>() : 0;
            var totalExtra = data["totalExtraProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["totalExtraProfit"].ToObject<decimal>() : 0;
            var adjustedNetProfit = data["adjustedNetProfit"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["adjustedNetProfit"].ToObject<decimal>() : 0;
            var versionId = data["versionId"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["versionId"].ToObject<long>() : 0;

            var agentId = data["agentId"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? data["agentId"].ToObject<long>() : 0;
            
            var item = new HistoricalYearDataDetail
            {
                AgentName = data["agentName"]?.ToString() ?? "",
                Year = year,
                AnnualTargetSales = data["targetSales"]?.ToObject<long>() ?? 0,
                AchievementRate = $"{achievementRate:F2}%",
                AchievementRatePercent = (double)achievementRate * 100,
                ImprovedSales = data["improvedSales"]?.ToObject<long>() ?? 0,
                SalesGrowthMargin = salesGrowth,
                StructureOptimizationMargin = structureOptimize,
                PremiumMargin = premium,
                TotalExcessMargin = totalExtra,
                AdjustedNetProfit = adjustedNetProfit,
                VersionId = versionId,
                AgentId = agentId,
                YearMonth = yearMonthStr
            };

            HistoricalYearDataDetails.Add(item);
        }

        [RelayCommand]
        private async Task ViewDetail(HistoricalYearDataDetail item)
        {
            try
            {
                Logger.Info($"查看年度历史方案详情: AgentId={item.AgentId}, YearMonth={item.YearMonth}, VersionId={item.VersionId}");
                
                var dialog = new AgentManagement.Avalonia.Views.rate.detailyear.DetailYearHistoryDialog();
                var viewModel = new DetailYearHistoryDialogViewModel(item.AgentId, item.YearMonth, item.VersionId);
                dialog.DataContext = viewModel;
                
                await viewModel.LoadDataAsync();
                await dialog.ShowDialog(App.MainWindow!);
            }
            catch (Exception ex)
            {
                Logger.Error($"查看年度历史方案详情异常: {ex.Message}", ex);
                StatusMessage = $"查看方案详情失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async void ExportData()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不导出数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                StatusMessage = "正在导出数据...";
                System.Diagnostics.Debug.WriteLine("[DetailYear] 开始导出数据");

                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth
                };

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/detailyear/export",
                    requestData);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                   
                    string fileName = $"detailyear_export_{yearMonth}.xlsx"; 

                    // 显示保存文件对话框，让用户选择保存位置
                    var filePath = await FileService.ShowSaveFileDialogAsync(
                        "保存数据",
                        fileName,
                        "Excel", "All"
                    );

                    if (string.IsNullOrEmpty(filePath))
                    {
                        // 用户取消了保存
                        Logger.Info("用户取消了保存操作");
                        return;
                    }

                    // 写入文件
                    System.IO.File.WriteAllBytes(filePath, fileBytes);

                    Logger.Success($"模板下载成功！保存位置: {filePath}");
                    StatusMessage = $"导出成功！文件已保存到：{filePath}";

                    var box = MessageBoxManager.GetMessageBoxStandard("导出成功", $"文件已成功保存到：\n{filePath}", ButtonEnum.Ok);
                    await box.ShowAsync();

                }
                else
                {
                    StatusMessage = "导出失败：未获取到文件数据";
                    System.Diagnostics.Debug.WriteLine("[DetailYear] 导出失败：未获取到文件数据");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出数据失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 导出异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 异常堆栈: {ex.StackTrace}");
            }
        }

        [RelayCommand]
        private void Calculate()
        {
            StatusMessage = "正在预测计算...";
        }

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [RelayCommand]
        private async void SaveSalesTargetYear()
        {
            await SaveSalesTargetYearAsync(
                long.TryParse(ConfigTargetSales?.Replace(",", "") ?? "0", out long t) ? t : 0,
                GrowthRate);
        }

        public async Task SaveSalesTargetYearAsync(long targetSales, double growthRate)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存销售目标数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("[DetailYear] 开始保存销售目标数据");
                
                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 接收到的参数 - targetSales={targetSales}, growthRate={growthRate}");
                
                decimal growthRateValue = (decimal)growthRate;
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 转换后 - targetSales={targetSales}, growthRateValue={growthRateValue}");
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    targetSales = targetSales,
                    growthRate = growthRateValue
                };
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存数据: agentId={agentId}, yearMonth={yearMonth}, targetSales={targetSales}, growthRate={growthRateValue}");
                System.Diagnostics.Debug.WriteLine($"[DetailYear] requestData对象: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/saletargetyear/saveRateSalesTargetYear",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[DetailYear] 保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存异常: {ex.Message}");
            }
        }

        public async Task SaveExpenseYearAsync(string expenseType, decimal amount)
        {
            await SaveExpenseYearAsync(expenseType, amount, 0);
        }

        public async Task LoadExpenseYearDataAsync(long agentId, string yearMonth)
        {
            try
            {
                var response = await NewApiClient.GetAsync<List<ExpenseYearDto>>(
                    $"/rate/expenseyear/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}");

                if (response?.Code == 200 && response.Data != null)
                {
                    // 先重置所有费用数据
                    ResetExpenseYearData();

                    foreach (var item in response.Data)
                    {
                        var expenseItem = new ExpenseItemViewModel
                        {
                            ExpenseType = item.ExpenseType,
                            Amount = item.Amount,
                            IsIncome = item.IsIncome
                        };

                        if (item.IsIncome == 1)
                        {
                            IncomeItems.Add(expenseItem);
                        }
                        else
                        {
                            FixedCostItems.Add(expenseItem);
                        }
                    }

                    // 如果没有数据，则初始化默认数据
                    if (FixedCostItems.Count == 0 && IncomeItems.Count == 0)
                    {
                        InitializeDefaultExpenseData();
                    }
                }
                else
                {
                    ResetExpenseYearData();
                    InitializeDefaultExpenseData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载年度费用数据失败: {ex.Message}";
                ResetExpenseYearData();
                InitializeDefaultExpenseData();
            }
        }

        /// <summary>
        /// 重置年度费用数据
        /// </summary>
        private void ResetExpenseYearData()
        {
            FixedCost = 0;
            Rent = 0;
            Salary = 0;
            VehicleCost = 0;
            WaterElectricity = 0;
            PIncome = 0;
            AcceptanceIncome = 0;
            AfterSalesIncome = 0;
            FixedCostItems.Clear();
            IncomeItems.Clear();
        }

        /// <summary>
        /// 初始化默认费用数据
        /// </summary>
        private void InitializeDefaultExpenseData()
        {
            // 初始化固定成本项目
            FixedCostItems.Add(new ExpenseItemViewModel { ExpenseType = "投入电池资金成本（贷款的资金利息）", Amount = 0, IsIncome = 0 });
            FixedCostItems.Add(new ExpenseItemViewModel { ExpenseType = "门面或仓库租金", Amount = 0, IsIncome = 0 });
            FixedCostItems.Add(new ExpenseItemViewModel { ExpenseType = "固定工资（内勤、业务、售后等）", Amount = 0, IsIncome = 0 });
            FixedCostItems.Add(new ExpenseItemViewModel { ExpenseType = "车辆油费、保险", Amount = 0, IsIncome = 0 });
            FixedCostItems.Add(new ExpenseItemViewModel { ExpenseType = "水电、招待费", Amount = 0, IsIncome = 0 });

            // 初始化收益项目
            IncomeItems.Add(new ExpenseItemViewModel { ExpenseType = "P", Amount = 0, IsIncome = 1 });
            IncomeItems.Add(new ExpenseItemViewModel { ExpenseType = "承兑收益", Amount = 0, IsIncome = 1 });
            IncomeItems.Add(new ExpenseItemViewModel { ExpenseType = "售后收益", Amount = 0, IsIncome = 1 });
        }

        /// <summary>
        /// 添加费用项目命令
        /// </summary>
        [RelayCommand]
        private async Task AddExpenseItem()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                var dialog = new AgentManagement.Avalonia.Views.rate.yuce.AddExpenseItemDialog();
                var viewModel = new AgentManagement.Avalonia.ViewModels.rate.yuce.AddExpenseItemDialogViewModel();
                dialog.DataContext = viewModel;
                
                bool isConfirmed = false;
                
                viewModel.OnConfirm += async (expenseType, amount, isIncome) =>
                {
                    isConfirmed = true;
                    dialog.Close();
                    
                    await SaveExpenseYearAsync(expenseType, amount, isIncome);
                    await LoadExpenseYearDataAsync(CurrentAgentId.Value, SelectedYear.ToString());
                    
                    StatusMessage = $"已添加{(isIncome == 1 ? "收益" : "固定成本")}项目: {expenseType}";
                };
                
                await dialog.ShowDialog(((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow!);
            }
            catch (Exception ex)
            {
                StatusMessage = $"添加失败: {ex.Message}";
            }
        }
        
        public async Task SaveExpenseYearAsync(string expenseType, decimal amount, int isIncome)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存费用数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 开始保存费用数据: expenseType={expenseType}, amount={amount}, isIncome={isIncome}");
                
                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    expenseType = expenseType,
                    amount = amount,
                    isIncome = isIncome
                };
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] requestData: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/expenseyear/saveRateMonthlyExpenseYear",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存费用响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[DetailYear] 费用保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 费用保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 费用保存异常: {ex.Message}");
            }
        }

        public async Task LoadProductStructureDataAsync(long agentId, string yearMonth)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DetailYear] 开始加载产品结构数据");
                
                var response = await NewApiClient.GetAsync<List<ProductStructureDto>>(
                    $"/rate/structureyear/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}");

                if (response?.Code == 200 && response.Data != null)
                {
                    ProductStructures.Clear();
                    foreach (var item in response.Data)
                    {
                        ProductStructures.Add(item);
                    }
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 加载产品结构数据成功，共 {ProductStructures.Count} 条");
                }
                else
                {
                    ProductStructures.Clear();
                }
                
                // 更新占比总和和结构优化总和
                UpdateProportionSum();
                UpdateOptimizationSum();
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载产品结构数据失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 加载产品结构数据失败: {ex.Message}");
                ProductStructures.Clear();
            }
        }

        public async Task SaveProductStructureYearAsync(ProductStructureDto structure)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存产品结构数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 开始保存产品结构数据: ModelId={structure.ModelId}, ItemModel={structure.ItemModel}");
                
                int yearMonth = SelectedYear;
                long agentId = CurrentAgentId.Value;
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    modelId = structure.ModelId,
                    structureRatio = structure.StructureRatio,
                    remiumPrice = structure.RemiumPrice,
                    remiumCost = structure.RemiumCost,
                    commission = structure.Commission,
                    priceAdjustment = structure.PriceAdjustment,
                    premiumDiscount = structure.PremiumDiscount
                };
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] requestData: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/structureyear/saveRateProductStructureYear",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 保存产品结构响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[DetailYear] 产品结构保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[DetailYear] 产品结构保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DetailYear] 产品结构保存异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新产品占比总和显示
        /// </summary>
        private void UpdateProportionSum()
        {
            var sum = ProductStructures.Sum(p => (double)p.StructureRatio);
            ProportionSumText = $"产品占比总和: {sum:F2}";
            
            if (Math.Abs(sum - 1.0) < 0.0005)
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

        /// <summary>
        /// 更新结构优化总和显示
        /// </summary>
        private void UpdateOptimizationSum()
        {
            var sum = ProductStructures.Sum(p => (double)p.PriceAdjustment);
            OptimizationSumText = $"结构优化总和: {sum:F2}";
            
            if (Math.Abs(sum - 0.0) < 0.005)
            {
                // 正好0
                OptimizationSumBackground = "#D1FAE5"; // green-100
                OptimizationSumBorder = "#BBF7D0"; // green-200
                OptimizationSumForeground = "#059669"; // green-600
                OptimizationSumText = $"✓ {OptimizationSumText} 已达标";
            }
            else if (sum > 0.0)
            {
                // 超过0
                OptimizationSumBackground = "#FEE2E2"; // red-100
                OptimizationSumBorder = "#FECACA"; // red-200
                OptimizationSumForeground = "#DC2626"; // red-600
                OptimizationSumText = $"✗ {OptimizationSumText} 超出{sum:F4}";
            }
            else
            {
                // 不足0
                OptimizationSumBackground = "#FEF3C7"; // amber-100
                OptimizationSumBorder = "#FDE68A"; // amber-200
                OptimizationSumForeground = "#D97706"; // amber-600
                OptimizationSumText = $"⚠ {OptimizationSumText} 还差{Math.Abs(sum):F4}";
            }
        }

        /// <summary>
        /// 添加产品
        /// </summary>
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
                    
                    // 创建新的产品结构项
                    var newStructure = new ProductStructureDto
                    {
                        ModelId = result.ItemId,
                        ItemModel = result.Model,
                        StructureRatio = (decimal)result.Proportion,
                        RemiumPrice = (decimal)result.GroupPrice,
                        RemiumCost = (decimal)result.PurchasePrice,
                        Commission = (decimal)result.Commission,
                        AgentId = CurrentAgentId.Value,
                        PremiumDiscount = 0.0m,
                        PriceAdjustment = 0.0m
                    };

                    //保存到数据库
                    await SaveProductStructureYearAsync(newStructure);

                    Logger.Info($"创建 ProductStructureDto 成功，准备添加到列表");
                    
                    // 检查是否在 UI 线程
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Logger.Info("当前在 UI 线程，直接添加");
                        ProductStructures.Add(newStructure);
                        
                        Logger.Success($"已添加产品: {result.Model}，当前产品数量: {ProductStructures.Count}");
                        StatusMessage = $"已添加产品: {result.Model}";
                        
                        // 更新占比总和和结构优化总和
                        UpdateProportionSum();
                        UpdateOptimizationSum();
                    }
                    else
                    {
                        Logger.Info("不在 UI 线程，使用 InvokeAsync");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ProductStructures.Add(newStructure);
                            
                            Logger.Success($"异步添加产品: {result.Model}，当前产品数量: {ProductStructures.Count}");
                            StatusMessage = $"已添加产品: {result.Model}";
                            
                            // 更新占比总和和结构优化总和
                            UpdateProportionSum();
                            UpdateOptimizationSum();
                        });
                    }
                    
                    // TODO: 调用 API 保存产品配置
                    // await SaveProductStructureAsync(newStructure);
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

        // ExpenseItemViewModel 类
        public partial class ExpenseItemViewModel : ObservableObject
        {
            [ObservableProperty]
            private string _expenseType = string.Empty;

            [ObservableProperty]
            private decimal _amount;

            [ObservableProperty]
            private int _isIncome; // 1: 收益项目, 0: 固定成本
        }

        // DTO类
        public partial class ExpenseYearDto
        {
            public string ExpenseType { get; set; }
            public decimal Amount { get; set; }
            public int IsIncome { get; set; }
        }

        public partial class ProductStructureDto : ObservableObject
        {
            [ObservableProperty]
            private long _structureId;
            
            [ObservableProperty]
            private long _modelId;
            
            [ObservableProperty] 
            private string _itemModel = string.Empty;
            
            [ObservableProperty]
            private long _agentId;
            
            [ObservableProperty]
            private string _agentName = string.Empty;
            
            [ObservableProperty]
            private string _yearMonth = string.Empty;
            
            [ObservableProperty]
            private decimal _structureRatio;
            
            [ObservableProperty]
            private decimal _remiumPrice;
            
            [ObservableProperty]
            private decimal _commission;
            
            [ObservableProperty]
            private decimal _remiumCost;
            
            [ObservableProperty]
            private decimal _premiumDiscount;
            
            [ObservableProperty]
            private decimal _priceAdjustment;
            
            public decimal SalesRatio => StructureRatio + PriceAdjustment;
        }

        [RelayCommand]
        public async Task DownloadTemplateAsync()
        {
            try
            {
                StatusMessage = "正在下载模板...";
                Logger.Info("开始下载销售目标导入模板");

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/saletargetyear/importTemplate",null);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = "saletargetyear_import_template.xlsx";

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

                    System.IO.File.WriteAllBytes(filePath, fileBytes);

                    Logger.Success($"模板下载成功！保存位置: {filePath}");
                    StatusMessage = $"模板下载成功！文件已保存到：{filePath}";

                    var box = MessageBoxManager.GetMessageBoxStandard("下载成功", $"模板已成功保存到：\n{filePath}", ButtonEnum.Ok);
                    await box.ShowAsync();
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
}