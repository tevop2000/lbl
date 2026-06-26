using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.ViewModels;
using AgentManagement.Avalonia.ViewModels.Controls;

namespace AgentManagement.Avalonia.ViewModels.rate.yuce
{
    public partial class indexViewModel : ViewModelBase
    {
        // KPI 卡片数据
        [ObservableProperty]
        private double _monthTargetSales = 0;

        [ObservableProperty]
        private double _achievementRate = 0;

        [ObservableProperty]
        private double _achievementRatePercent = 0;  // 用于进度条宽度
        
        [ObservableProperty]
        private double _progressValue = 0;
        
        [ObservableProperty]
        private double _progressBarWidth = 0;

        [ObservableProperty]
        private double _actualSales = 0;

        [ObservableProperty]
        private double _netProfit = 0;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        // 季度统计数据
        [ObservableProperty]
        private long _quarterTargetSales = 0;

        [ObservableProperty]
        private long _quarterBreakSales = 0;

        [ObservableProperty]
        private decimal _quarterTargetNetProfit = 0;

        [ObservableProperty]
        private decimal _quarterActualNetProfit = 0;

        [ObservableProperty]
        private decimal _quarterNetProfitDiff = 0;

        // 季度月度列表数据（用于利润进度看板）
        public ObservableCollection<RateMonthlyDetail> QuarterlyMonthlyList { get; } = new ObservableCollection<RateMonthlyDetail>();

        // 季度列表信息（用于月度明细看板）
        public ObservableCollection<RateMonthlyDetailQuarterly> QuarterlyListInfo { get; } = new ObservableCollection<RateMonthlyDetailQuarterly>();

        public GridLength Month1ProfitGridLength => CalculateProfitGridLength(0);
        public GridLength Month2ProfitGridLength => CalculateProfitGridLength(1);
        public GridLength Month3ProfitGridLength => CalculateProfitGridLength(2);

        public GridLength ProgressBarGridLength => CalculateProgressBarGridLength();
        public GridLength ProgressRemainingGridLength => CalculateProgressRemainingGridLength();

        public GridLength ProgressMonth1GridLength => CalculateProgressMonthGridLength(0);
        public GridLength ProgressMonth2GridLength => CalculateProgressMonthGridLength(1);
        public GridLength ProgressMonth3GridLength => CalculateProgressMonthGridLength(2);

        public string ProgressMonth1Color => GetProgressColor(0);
        public string ProgressMonth2Color => GetProgressColor(1);
        public string ProgressMonth3Color => GetProgressColor(2);

        private GridLength CalculateProfitGridLength(int index)
        {
            if (QuarterlyMonthlyList == null || QuarterlyMonthlyList.Count <= index)
                return new GridLength(1, GridUnitType.Star);

            var item = QuarterlyMonthlyList[index];
            if (item == null)
                return new GridLength(1, GridUnitType.Star);

            decimal total = 0;
            foreach (var listItem in QuarterlyMonthlyList)
            {
                if (listItem != null && listItem.AdjustedNetProfit.HasValue)
                    total += listItem.AdjustedNetProfit.Value;
            }

            if (total <= 0)
                return new GridLength(1, GridUnitType.Star);

            var current = item.AdjustedNetProfit ?? 0;
            double ratio = Math.Max((double)(current / total), 0.01);
            return new GridLength(ratio, GridUnitType.Star);
        }

        private GridLength CalculateProgressBarGridLength()
        {
            if (QuarterlyMonthlyList == null || QuarterlyListInfo == null)
                return new GridLength(1, GridUnitType.Star);

            decimal progressTotal = 0;
            foreach (var listItem in QuarterlyListInfo)
            {
                if (listItem != null && listItem.AdjustedNetProfit.HasValue)
                    progressTotal += listItem.AdjustedNetProfit.Value;
            }

            if (progressTotal <= 0)
                return new GridLength(0);

            return new GridLength((double)progressTotal, GridUnitType.Star);
        }

        private GridLength CalculateProgressRemainingGridLength()
        {
            if (QuarterlyMonthlyList == null || QuarterlyListInfo == null)
                return new GridLength(0);

            decimal targetTotal = 0;
            foreach (var listItem in QuarterlyMonthlyList)
            {
                if (listItem != null && listItem.AdjustedNetProfit.HasValue)
                    targetTotal += listItem.AdjustedNetProfit.Value;
            }

            if (targetTotal <= 0)
                return new GridLength(0);

            decimal progressTotal = 0;
            foreach (var listItem in QuarterlyListInfo)
            {
                if (listItem != null && listItem.AdjustedNetProfit.HasValue)
                    progressTotal += listItem.AdjustedNetProfit.Value;
            }

            decimal remaining = targetTotal - progressTotal;
            if (remaining <= 0)
                return new GridLength(0);

            return new GridLength((double)remaining, GridUnitType.Star);
        }

        private GridLength CalculateProgressMonthGridLength(int index)
        {
            if (QuarterlyListInfo == null || QuarterlyListInfo.Count <= index)
                return new GridLength(1, GridUnitType.Star);

            var item = QuarterlyListInfo[index];
            if (item == null)
                return new GridLength(1, GridUnitType.Star);

            var current = item.AdjustedNetProfit ?? 0;

            if (current <= 0)
                return new GridLength(0);

            decimal positiveSum = 0;
            foreach (var listItem in QuarterlyListInfo)
            {
                if (listItem != null && listItem.AdjustedNetProfit.HasValue)
                {
                    var val = listItem.AdjustedNetProfit.Value;
                    if (val > 0)
                        positiveSum += val;
                }
            }

            if (positiveSum <= 0)
                return new GridLength(1, GridUnitType.Star);

            double ratio = Math.Max((double)(current / positiveSum), 0.01);
            return new GridLength(ratio, GridUnitType.Star);
        }

        private string GetProgressColor(int index)
        {
            if (QuarterlyListInfo == null || QuarterlyListInfo.Count <= index)
                return "#DC2626";

            var item = QuarterlyListInfo[index];
            if (item == null)
                return "#DC2626";

            // isFinish == 1 表示可修改（红色），isFinish == 0 表示已结算（绿色）
            if (item.IsFinish == 1)
                return "#DC2626";
            else
                return "#10B981";
        }

        // 月度数据列表
        public ObservableCollection<MonthlyDataItem> MonthlyDataList { get; } = new ObservableCollection<MonthlyDataItem>();
        
        // 历史方案数据列表
        public ObservableCollection<HistoricalMonthlyDataItem> HistoricalMonthlyDataList { get; } = new ObservableCollection<HistoricalMonthlyDataItem>();
        
        // Tab选择（0=当前方案，1=历史方案）
        [ObservableProperty]
        private int _selectedTabIndex = 0;
        
        partial void OnSelectedTabIndexChanged(int value)
        {
            // 当切换到历史方案Tab时，加载历史数据
            if (value == 1 && CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadHistoricalDataAsync(CurrentAgentId.Value, $"{SelectedYear}-{SelectedMonth:D2}");
            }
        }

        // 基础配置
        [ObservableProperty]
        private ObservableCollection<int> _monthList = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        [ObservableProperty]
        private int _selectedMonth = DateTime.Now.Month;

        [ObservableProperty]
        private ObservableCollection<string> _quarterList = new() { "Q1", "Q2", "Q3", "Q4" };

        [ObservableProperty]
        private string _selectedQuarter = "Q1";

        partial void OnSelectedQuarterChanged(string value)
        {
            if (IsQuarterlyView && CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadQuarterlyStatsDataAsync(CurrentAgentId.Value);
                _ = LoadQuarterlyMonthlyListAsync(CurrentAgentId.Value);
                _ = LoadQuarterlyListInfoAsync(CurrentAgentId.Value);
            }
        }
        
        [RelayCommand]
        private async Task ConfirmPlan()
        {
            Logger.Info("确认方案按钮点击");
            
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("当前代理商无效");
                return;
            }
            
            try
            {
                string yearMonth = SelectedYear.ToString();
                string quarterly = SelectedQuarter;
                
                Logger.Info($"调用 saveQuarterlyListinfo 接口: agentId={CurrentAgentId.Value}, yearMonth={yearMonth}, quarterly={quarterly}");
                
                var requestData = new
                {
                    agentId = CurrentAgentId.Value,
                    yearMonth = yearMonth,
                    quarterly = quarterly
                };
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/detailQuarterly/saveQuarterlyListinfo",
                    requestData);
                
                if (response?.Code == 200)
                {
                    Logger.Success("方案更新成功");
                    var box = MessageBoxManager.GetMessageBoxStandard("成功", "方案更新完毕", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    Logger.Error($"方案更新失败: {response?.Message}");
                    var box = MessageBoxManager.GetMessageBoxStandard("失败", $"方案更新失败: {response?.Message}", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"调用 saveQuarterlyListinfo 接口异常: {ex.Message}");
                var box = MessageBoxManager.GetMessageBoxStandard("异常", $"调用接口失败: {ex.Message}", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }

        [ObservableProperty]
        private ObservableCollection<string> _viewModeOptions = new() { "月度", "季度", "年度" };

        [ObservableProperty]
        private string _selectedViewMode = "月度";

        partial void OnSelectedViewModeChanged(string value)
        {
            switch (value)
            {
                case "月度":
                    ViewMode = "monthly";
                    break;
                case "季度":
                    ViewMode = "quarterly";
                    break;
                case "年度":
                    ViewMode = "yearlycompare";
                    break;
            }
        }

        partial void OnSelectedYearChanged(int value)
        {
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadDataAsync();
                
                // 如果当前显示的是历史方案Tab，同时刷新历史数据
                if (SelectedTabIndex == 1)
                {
                    _ = LoadHistoricalDataAsync(CurrentAgentId.Value, $"{SelectedYear}-{SelectedMonth:D2}");
                }
            }
        }

        [ObservableProperty]
        private double _editableTargetSales = 0;

        [ObservableProperty]
        private double _editableActualSales = 0;

        // 基础配置
        [ObservableProperty]
        private string _configTargetSales = "0";

        [ObservableProperty]
        private double _growthRate = 0;

        // 固定成本
        [ObservableProperty]
        private decimal _fixedCost = 0;

        [ObservableProperty]
        private decimal _rent = 0;

        [ObservableProperty]
        private decimal _salary = 0;

        [ObservableProperty]
        private decimal _vehicleCost = 0;

        [ObservableProperty]
        private decimal _waterElectricity = 0;

        // 收益项目
        [ObservableProperty]
        private decimal _pIncome = 0;

        [ObservableProperty]
        private decimal _acceptanceIncome = 0;

        [ObservableProperty]
        private decimal _afterSalesIncome = 0;
        
        // 动态费用项目
        public ObservableCollection<ExpenseItemViewModel> FixedCostItems { get; } = new();
        public ObservableCollection<ExpenseItemViewModel> IncomeItems { get; } = new();
        
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

        // 产品列表
        [ObservableProperty]
        private ObservableCollection<AgentManagement.Avalonia.ViewModels.ProductItem> _productList = new();

        // 产品数量
        [ObservableProperty]
        private int _productCount = 0;

        // 产品结构列表
        public ObservableCollection<ProductStructureDto> ProductStructures { get; } = new();
        
        // 产品占比总和显示
        [ObservableProperty]
        private string _proportionSumText = "产品占比总和: 0.00";
        
        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4"; // green-50
        
        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A"; // green-60
        
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
        
        // 年度净利润对比图表数据
        [ObservableProperty]
        private ObservableCollection<NetProfitComparisonItem> _netProfitComparisonData = new();
        
        [ObservableProperty]
        private ObservableCollection<string> _netProfitComparisonYAxisLabels = new();
        
        [ObservableProperty]
        private string _lastYearLabel = "去年实际";
        
        [ObservableProperty]
        private string _currentYearLabel = "今年预测";

        // 代理商相关（通过选择器设置）
        [ObservableProperty]
        private long? _currentAgentId;

        [ObservableProperty]
        private string _currentAgentName = "未选择";

        [ObservableProperty]
        private int _selectedYear = DateTime.Now.Year;

        // 代理商列表（旧的，暂时保留）
        [ObservableProperty]
        private ObservableCollection<AgentUser> _agentList = new();

        [ObservableProperty]
        private AgentUser? _selectedAgent;

        // 部门树列表（战区、渠道部、大区经理、业务经理）
        [ObservableProperty]
        private ObservableCollection<DeptInfo> _warZoneList = new();

        [ObservableProperty]
        private DeptInfo? _selectedWarZone;

        partial void OnSelectedWarZoneChanged(DeptInfo? value)
        {
            // 当战区改变时，清空下级选择并更新渠道部列表
            SelectedChannelDept = null;
            SelectedRegionManager = null;
            SelectedManager = null;
            
            ChannelDeptList.Clear();
            RegionManagerList.Clear();
            ManagerList.Clear();
            
            if (value != null)
            {
                // 加载该战区下的渠道部
                foreach (var channelDept in value.Children)
                {
                    ChannelDeptList.Add(channelDept);
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<DeptInfo> _channelDeptList = new();

        [ObservableProperty]
        private DeptInfo? _selectedChannelDept;

        partial void OnSelectedChannelDeptChanged(DeptInfo? value)
        {
            // 当渠道部改变时，清空下级选择并更新大区经理列表
            SelectedRegionManager = null;
            SelectedManager = null;
            
            RegionManagerList.Clear();
            ManagerList.Clear();
            
            if (value != null)
            {
                // 加载该渠道部下的大区
                foreach (var region in value.Children)
                {
                    RegionManagerList.Add(region);
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<DeptInfo> _regionManagerList = new();

        [ObservableProperty]
        private DeptInfo? _selectedRegionManager;

        partial void OnSelectedRegionManagerChanged(DeptInfo? value)
        {
            Console.WriteLine($"\n========== 部门选择变化 ==========");
            Console.WriteLine($"选择的部门: {value?.DisplayName ?? "null"}");
            Console.WriteLine($"部门ID: {value?.Id ?? 0}");
            Console.WriteLine($"==================================\n");
            
            // 当大区经理改变时，清空下级选择
            SelectedManager = null;
            
            // 触发查询业务经理的接口
            _ = LoadManagersByDeptAsync();
        }
        
        /// <summary>
        /// 根据选择的部门查询业务经理列表
        /// </summary>
        private async Task LoadManagersByDeptAsync()
        {
            try
            {
                Console.WriteLine("\n\n========================================");
                Console.WriteLine("开始加载业务经理列表");
                Console.WriteLine("========================================\n");
                
                // 获取最终选择的部门ID（区域经理）
                if (SelectedRegionManager == null)
                {
                    Console.WriteLine("⚠ 未选择部门，清空业务经理列表");
                    Logger.Debug("未选择部门，清空业务经理列表");
                    ManagerList.Clear();
                    return;
                }

                var deptId = SelectedRegionManager.Id;
                var relativeUrl = $"/system/user/list?pageNum=1&pageSize=1000&deptId={deptId}";
                var fullUrl = $"{NewApiClient.BaseUrl}{relativeUrl}";
                
                Console.WriteLine($"📡 调用接口: {fullUrl}");
                Console.WriteLine($"📋 部门ID: {deptId}");
                Console.WriteLine($"📋 部门名称: {SelectedRegionManager.DisplayName}\n");
                
                Logger.Info($"开始加载业务经理列表 - 部门ID: {deptId}");
                
                // 调用接口：/system/user/list?pageNum=1&pageSize=1000&deptId={deptId}
                // 注意：该接口直接返回分页数据，不是标准的 ApiResponse 格式
                var httpResponse = await NewApiClient.GetHttpClient().GetAsync(fullUrl);
                var content = await httpResponse.Content.ReadAsStringAsync();
                
                Console.WriteLine("\n========== 原始响应内容 ==========");
                Console.WriteLine(content);
                Console.WriteLine("==================================\n");
                
                // 直接反序列化为 PageResult
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<PageResult<AgentUser>>(content);
                
                // 打印解析后的数据
                Console.WriteLine("\n========== 业务经理接口返回数据 ==========");
                if (response != null)
                {
                    Console.WriteLine($"Code: {response.Code}");
                    Console.WriteLine($"Msg: {response.Msg}");
                    Console.WriteLine($"Total: {response.Total}");
                    Console.WriteLine($"Rows 数量: {response.Rows?.Count ?? 0}");
                    
                    if (response.Rows != null && response.Rows.Count > 0)
                    {
                        ManagerList.Clear();
                        foreach (var user in response.Rows)
                        {
                            // 将 AgentUser 转换为 DeptInfo 格式用于显示
                            var deptInfo = new DeptInfo
                            {
                                Id = user.UserId,
                                Label = user.NickName ?? "",
                                DeptName = user.NickName ?? ""
                            };
                            ManagerList.Add(deptInfo);
                        }
                        
                        Console.WriteLine($"✅ 成功加载 {ManagerList.Count} 个业务经理\n");
                        Logger.Success($"成功加载 {ManagerList.Count} 个业务经理");
                    }
                    else
                    {
                        Console.WriteLine("⚠ 没有用户数据\n");
                        ManagerList.Clear();
                    }
                }
                else
                {
                    Console.WriteLine($"❌ 反序列化失败\n");
                    Logger.Warning("反序列化 PageResult 失败");
                    ManagerList.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 加载业务经理异常: {ex.Message}");
                Console.WriteLine($"堆栈跟踪: {ex.StackTrace}\n");
                Logger.Error($"加载业务经理异常: {ex.Message}", ex);
                ManagerList.Clear();
            }
        }

        [ObservableProperty]
        private ObservableCollection<DeptInfo> _managerList = new();

        [ObservableProperty]
        private DeptInfo? _selectedManager;

        // 视图模式
        [ObservableProperty]
        private string _viewMode = "monthly"; // monthly, quarterly, yearly, yearlycompare

        // 视图模式判断属性
        public bool IsMonthlyView => ViewMode == "monthly";
        public bool IsQuarterlyView => ViewMode == "quarterly";
        public bool IsYearlySimpleView => ViewMode == "yearly";
        public bool IsYearlyCompareView => ViewMode == "yearlycompare";

        partial void OnViewModeChanged(string value)
        {
            OnPropertyChanged(nameof(IsMonthlyView));
            OnPropertyChanged(nameof(IsQuarterlyView));
            OnPropertyChanged(nameof(IsYearlySimpleView));
            OnPropertyChanged(nameof(IsYearlyCompareView));
            // 当视图模式改变时，重新加载数据
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadDataAsync();
            }
        }

        // ========== 年度视图数据 ==========
        // 年度选择（使用上面定义的 SelectedYear 属性）

        [ObservableProperty]
        private ObservableCollection<int> _yearList = new();

        [ObservableProperty]
        private int _compareYear;

        partial void OnCompareYearChanged(int value)
        {
            if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0 && ViewMode == "yearlycompare")
            {
                _ = LoadDataAsync();
            }
        }

        // 年度 KPI 数据
        [ObservableProperty]
        private double _yearTargetSales = 120000;

        [ObservableProperty]
        private double _breakEvenRate = 85.5;

        [ObservableProperty]
        private double _optimizedSales = 135000;

        [ObservableProperty]
        private double _salesGrowthProfit = 45000;

        [ObservableProperty]
        private double _structureOptimizeProfit = 28000;

        [ObservableProperty]
        private double _premiumProfit = 15000;

        [ObservableProperty]
        private double _totalExtraProfit = 88000;

        [ObservableProperty]
        private double _adjustedNetProfit = 125000;

        [ObservableProperty]
        private double _totalExpense = 380000;

        // 年度产品优化配置
        [ObservableProperty]
        private ObservableCollection<ProductOptimizationItem> _productOptimizations = new();

        // 销量增长百分比（滑块）
        [ObservableProperty]
        private double _salesIncreasePercent = 12.5;

        // 验证状态
        [ObservableProperty]
        private bool _isProportionValid = true;

        [ObservableProperty]
        private bool _isStructureOptimizeSumValid = true;

        public indexViewModel()
        {
            Logger.Info("indexViewModel 构造函数被调用");
            // 初始化年份列表（当前年份及前后各2年）
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear - 2; year <= currentYear + 2; year++)
            {
                YearList.Add(year);
            }

            SelectedYear = currentYear;
            CompareYear = currentYear - 1;
            
            // 先不加载模拟数据，等待选择器初始化
            // LoadMockData();
            // _ = LoadAgentListAsync(); // 暂时不调用代理商列表接口
            // 部门树加载已移至 CascadingDeptSelector 控件内部
            
            // 初始化销售量达成率
            UpdateAchievementRate();
            
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
                
                // 更新占比总和
                UpdateProportionSum();
                UpdateOptimizationSum();
            };
        }

        partial void OnSelectedMonthChanged(int value)
        {
            // 当月份变化时，重新加载数据
            _ = LoadDataAsync();
            
            // 如果当前显示的是历史方案Tab，同时刷新历史数据
            if (SelectedTabIndex == 1 && CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
            {
                _ = LoadHistoricalDataAsync(CurrentAgentId.Value, $"{SelectedYear}-{SelectedMonth:D2}");
            }
        }

        /// <summary>
        /// 当代理商选择变化时调用
        /// </summary>
        public async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {
            Logger.Info($"OnAgentChangedAsync 被调用: agent={agent?.AgentName}, manager={manager?.NickName}");
            
            long? agentId = agent?.AgentId;

            // 更新显示的名称 - 优先使用代理商名称
            if (agent != null)
            {
                CurrentAgentName = agent.AgentName;
                Logger.Info($"使用代理商名称: {CurrentAgentName}");
            }        
            else
            {
                CurrentAgentName = "未选择";
                Logger.Warning("未选择任何代理商或业务经理");
            }

            CurrentAgentId = agentId;

            // 如果选择了业务经理，立即加载该业务经理下的代理商列表
            if (manager != null && manager.UserId > 0)
            {
                Logger.Info($"准备加载业务经理 (UserID={manager.UserId}) 的代理商列表");
                await LoadAgentsByManagerAsync(manager.UserId);
            }
            else
            {
                Logger.Warning("没有选择业务经理或业务经理 UserID 无效，清空代理商列表");
                AgentList.Clear();
            }

            // 更新代理商列表中的选中状态（无论是否选择了代理商）
            UpdateAgentSelectionStatus(agentId);

            // 只有当 agentId 有效时才加载其他数据
            if (agentId.HasValue && agentId.Value > 0)
            {
                // 加载产品配置列表
                await LoadProductConfigListAsync(agentId.Value);
                
                // 加载其他数据
                await LoadDataAsync();
                
                // 如果当前显示的是历史方案Tab，同时刷新历史数据
                if (SelectedTabIndex == 1)
                {
                    await LoadHistoricalDataAsync(agentId.Value, $"{SelectedYear}-{SelectedMonth:D2}");
                }
            }
            else
            {
                // 清空所有数据
                ClearData();
                ProductList.Clear();
                ProductCount = 0;
                HistoricalMonthlyDataList.Clear();
            }
        }
        
        /// <summary>
        /// 更新代理商列表中的选中状态
        /// </summary>
        private void UpdateAgentSelectionStatus(long? selectedAgentId)
        {
            foreach (var agent in AgentList)
            {
                agent.IsSelected = (selectedAgentId.HasValue && agent.AgentId == selectedAgentId.Value);
            }
        }
        
        /// <summary>
        /// 根据业务经理 ID 加载代理商列表
        /// </summary>
        private async Task LoadAgentsByManagerAsync(long managerUserId)
        {
            try
            {
                Logger.Info($"开始加载业务经理 (UserID={managerUserId}) 的代理商列表...");
                
                var response = await NewApiClient.GetAsync<ObservableCollection<AgentUser>>(
                    $"/rate/default/getMyAgentListWithCount?userId={managerUserId}");
                
                Logger.Info($"API 响应 - Code: {response.Code}, Message: {response.Message}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    Logger.Info($"API 返回数据条数: {response.Data.Count}");
                    
                    AgentList.Clear();
                    foreach (var agent in response.Data)
                    {
                        Logger.Debug($"添加代理商: AgentId={agent.AgentId}, Name={agent.AgentName}, ProductCount={agent.ProductDefaultCount}");
                        AgentList.Add(agent);
                    }
                    
                    Logger.Info($"AgentList 当前数量: {AgentList.Count}");
                    Logger.Success($"成功加载 {AgentList.Count} 个代理商");
                }
                else
                {
                    Logger.Error($"加载代理商列表失败: {response.Message}");
                    AgentList.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载代理商列表异常: {ex.Message}", ex);
                AgentList.Clear();
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        private void ClearData()
        {
            MonthlyDataList.Clear();
            ProductStructures.Clear();
            MonthTargetSales = 0;
            AchievementRate = 0;
            AchievementRatePercent = 0;
            ProgressValue = 0;
            ProgressBarWidth = 0;
            ActualSales = 0;
            NetProfit = 0;
        }

        /// <summary>
        /// 刷新页面数据
        /// </summary>
        public async Task RefreshDataAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载数据");
                return;
            }

            string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
            
            var tasks = new List<Task>
            {
                IsMonthlyView ? LoadDetailDataAsync() : LoadYearlyDetailDataAsync(),
                LoadConfigDataAsync(),
                LoadExpenseDataAsync(CurrentAgentId.Value, yearMonth),
                LoadProductStructureDataAsync(CurrentAgentId.Value, yearMonth)
            };
            
            // 如果是年度视图，同时加载年度净利润对比数据
            if ((IsYearlySimpleView || IsYearlyCompareView) && CurrentAgentId.Value > 0)
            {
                tasks.Add(LoadNetProfitComparisonDataAsync(CurrentAgentId.Value));
            }
            
            // 如果是季度视图，加载季度统计数据
            if (IsQuarterlyView && CurrentAgentId.Value > 0)
            {
                tasks.Add(LoadQuarterlyStatsDataAsync(CurrentAgentId.Value));
                tasks.Add(LoadQuarterlyMonthlyListAsync(CurrentAgentId.Value));
                tasks.Add(LoadQuarterlyListInfoAsync(CurrentAgentId.Value));
            }
            
            await Task.WhenAll(tasks);
        }

        public async Task SaveExpenseAsync(string expenseType, decimal amount, int isIncome = 1)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存费用数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始保存费用数据");
                
                string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
                long agentId = CurrentAgentId.Value;
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    expenseType = expenseType,
                    amount = amount,
                    isIncome = isIncome
                };
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] requestData: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/expense/saveRateMonthlyExpense",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[Yuce] 保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存异常: {ex.Message}");
            }
        }

        public async Task LoadExpenseDataAsync(long agentId, string yearMonth)
        {
            try
            {
                var response = await NewApiClient.GetAsync<List<ExpenseDto>>(
                    $"/rate/expense/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}");

                if (response?.Code == 200 && response.Data != null)
                {
                    // 先重置所有费用数据
                    ResetExpenseData();
                    
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
                    ResetExpenseData();
                    InitializeDefaultExpenseData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载费用数据失败: {ex.Message}";
                ResetExpenseData();
                InitializeDefaultExpenseData();
            }
        }

        /// <summary>
        /// 重置费用数据
        /// </summary>
        private void ResetExpenseData()
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
        
        public async Task LoadProductStructureDataAsync(long agentId, string yearMonth)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始加载产品结构数据");
                
                var response = await NewApiClient.GetAsync<List<ProductStructureDto>>(
                    $"/rate/structure/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}");

                if (response?.Code == 200 && response.Data != null)
                {
                    ProductStructures.Clear();
                    foreach (var item in response.Data)
                    {
                        ProductStructures.Add(item);
                    }
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 加载产品结构数据成功，共 {ProductStructures.Count} 条");
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
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载产品结构数据失败: {ex.Message}");
                ProductStructures.Clear();
            }
        }
        
        /// <summary>
        /// 加载产品配置列表（用于产品配置管理面板）
        /// </summary>
        public async Task LoadProductConfigListAsync(long agentId)
        {
            try
            {
                Logger.Info($"开始加载代理商 (ID={agentId}) 的产品配置列表...");
                
                // TODO: 调用 API 获取产品配置列表
                // 示例：/rate/product/getByAgentId?agentId={agentId}
                // 暂时使用模拟数据
                ProductList.Clear();
                
                // 模拟数据 - 实际应该从 API 获取
                /*
                var response = await NewApiClient.GetAsync<List<ProductItem>>(
                    $"/rate/product/getByAgentId?agentId={agentId}");
                
                if (response?.Code == 200 && response.Data != null)
                {
                    foreach (var product in response.Data)
                    {
                        ProductList.Add(product);
                    }
                }
                */
                
                ProductCount = ProductList.Count;
                Logger.Success($"成功加载 {ProductCount} 个产品配置");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载产品配置列表异常: {ex.Message}", ex);
                ProductList.Clear();
                ProductCount = 0;
            }
        }
        
        public async Task SaveProductStructureAsync(ProductStructureDto structure)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存产品结构数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 开始保存产品结构数据: ModelId={structure.ModelId}, ItemModel={structure.ItemModel}");
                
                string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
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
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] requestData: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/structure/saveRateProductStructure",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存产品结构响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[Yuce] 产品结构保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 产品结构保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Yuce] 产品结构保存异常: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 加载年度净利润对比数据
        /// </summary>
        private async Task LoadNetProfitComparisonDataAsync(long agentId)
        {
            try
            {
                int lastYear = CompareYear;
                int currentYear = SelectedYear;
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 开始加载年度净利润对比数据: agentId={agentId}, lastYear={lastYear}, currentYear={currentYear}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/enddetail/getNetProfitComparison?agentId={agentId}&lastYear={lastYear}&currentYear={currentYear}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 年度净利润对比数据响应码: {response.Code}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data;
                    BuildNetProfitComparisonChart(data, lastYear, currentYear);
                }
                else
                {
                    BuildEmptyNetProfitComparisonChart();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载年度净利润对比数据异常: {ex.Message}");
                BuildEmptyNetProfitComparisonChart();
            }
        }

        private async Task LoadQuarterlyStatsDataAsync(long agentId)
        {
            try
            {
                string yearMonth = SelectedYear.ToString();
                string quarterly = SelectedQuarter;
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 开始加载季度统计数据: agentId={agentId}, yearMonth={yearMonth}, quarterly={quarterly}");
                
                var response = await NewApiClient.GetAsync<RateQuarterlyStatsVO>(
                    $"/rate/detailQuarterly/getQuarterlyStats?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 季度统计数据响应码: {response.Code}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data;
                    QuarterTargetSales = data.QuarterTargetSales ?? 0;
                    QuarterBreakSales = data.QuarterBreakSales ?? 0;
                    QuarterTargetNetProfit = data.QuarterTargetNetProfit ?? 0;
                    // QuarterActualNetProfit 由 LoadQuarterlyListInfoAsync 计算，不要覆盖
                    QuarterNetProfitDiff = QuarterActualNetProfit - QuarterTargetNetProfit;
                }
                else
                {
                    QuarterTargetSales = 0;
                    QuarterBreakSales = 0;
                    QuarterTargetNetProfit = 0;
                    // 不要重置 QuarterActualNetProfit，由 LoadQuarterlyListInfoAsync 管理
                    QuarterNetProfitDiff = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载季度统计数据异常: {ex.Message}");
                QuarterTargetSales = 0;
                QuarterBreakSales = 0;
                QuarterTargetNetProfit = 0;
                // 不要重置 QuarterActualNetProfit
                QuarterNetProfitDiff = 0;
            }
        }

        private async Task LoadQuarterlyMonthlyListAsync(long agentId)
        {
            try
            {
                string yearMonth = SelectedYear.ToString();
                string quarterly = SelectedQuarter;
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 开始加载季度月度列表数据: agentId={agentId}, yearMonth={yearMonth}, quarterly={quarterly}");
                
                var response = await NewApiClient.GetAsync<List<RateMonthlyDetail>>(
                    $"/rate/detailQuarterly/getQuarterlyList?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 季度月度列表数据响应码: {response.Code}");
                
                QuarterlyMonthlyList.Clear();
                
                if (response.Code == 200 && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        QuarterlyMonthlyList.Add(item);
                    }
                }
                
                OnPropertyChanged(nameof(Month1ProfitGridLength));
                OnPropertyChanged(nameof(Month2ProfitGridLength));
                OnPropertyChanged(nameof(Month3ProfitGridLength));

                OnPropertyChanged(nameof(ProgressBarGridLength));
                OnPropertyChanged(nameof(ProgressRemainingGridLength));
                OnPropertyChanged(nameof(ProgressMonth1GridLength));
                OnPropertyChanged(nameof(ProgressMonth2GridLength));
                OnPropertyChanged(nameof(ProgressMonth3GridLength));

                OnPropertyChanged(nameof(ProgressMonth1Color));
                OnPropertyChanged(nameof(ProgressMonth2Color));
                OnPropertyChanged(nameof(ProgressMonth3Color));

                

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载季度月度列表数据异常: {ex.Message}");
                QuarterlyMonthlyList.Clear();
                
                OnPropertyChanged(nameof(Month1ProfitGridLength));
                OnPropertyChanged(nameof(Month2ProfitGridLength));
                OnPropertyChanged(nameof(Month3ProfitGridLength));

                OnPropertyChanged(nameof(ProgressBarGridLength));
                OnPropertyChanged(nameof(ProgressRemainingGridLength));
                OnPropertyChanged(nameof(ProgressMonth1GridLength));
                OnPropertyChanged(nameof(ProgressMonth2GridLength));
                OnPropertyChanged(nameof(ProgressMonth3GridLength));

                OnPropertyChanged(nameof(ProgressMonth1Color));
                OnPropertyChanged(nameof(ProgressMonth2Color));
                OnPropertyChanged(nameof(ProgressMonth3Color));
            }
        }

        private async Task LoadQuarterlyListInfoAsync(long agentId)
        {
            try
            {
                string yearMonth = SelectedYear.ToString();
                string quarterly = SelectedQuarter;

                System.Diagnostics.Debug.WriteLine($"[Yuce] 开始加载季度列表信息: agentId={agentId}, yearMonth={yearMonth}, quarterly={quarterly}");

                var response = await NewApiClient.GetAsync<List<RateMonthlyDetailQuarterly>>(
                    $"/rate/detailQuarterly/getQuarterlyListinfo?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}")
                    .ConfigureAwait(true);

                System.Diagnostics.Debug.WriteLine($"[Yuce] 季度列表信息响应码: {response.Code}");

                QuarterlyListInfo.Clear();

                if (response.Code == 200 && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        QuarterlyListInfo.Add(item);
                    }
                    
                    decimal total = QuarterlyListInfo.Sum(item => item.AdjustedNetProfit ?? 0);
                    System.Diagnostics.Debug.WriteLine($"[Yuce] QuarterlyListInfo数量: {QuarterlyListInfo.Count}, AdjustedNetProfit总和: {total}");
                    foreach (var item in QuarterlyListInfo)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Yuce] Month: {item.YearMonth}, AdjustedNetProfit: {item.AdjustedNetProfit}");
                    }
                    QuarterActualNetProfit = total;
                    QuarterNetProfitDiff = QuarterActualNetProfit - QuarterTargetNetProfit;
                    System.Diagnostics.Debug.WriteLine($"[Yuce] QuarterActualNetProfit: {QuarterActualNetProfit}, QuarterTargetNetProfit: {QuarterTargetNetProfit}, QuarterNetProfitDiff: {QuarterNetProfitDiff}");
                    
                    OnPropertyChanged(nameof(ProgressBarGridLength));
                    OnPropertyChanged(nameof(ProgressRemainingGridLength));
                    OnPropertyChanged(nameof(ProgressMonth1GridLength));
                    OnPropertyChanged(nameof(ProgressMonth2GridLength));
                    OnPropertyChanged(nameof(ProgressMonth3GridLength));
                    OnPropertyChanged(nameof(ProgressMonth1Color));
                    OnPropertyChanged(nameof(ProgressMonth2Color));
                    OnPropertyChanged(nameof(ProgressMonth3Color));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载季度列表信息异常: {ex.Message}");
                QuarterlyListInfo.Clear();
                QuarterActualNetProfit = 0;
                QuarterNetProfitDiff = 0;
                
                OnPropertyChanged(nameof(ProgressBarGridLength));
                OnPropertyChanged(nameof(ProgressRemainingGridLength));
                OnPropertyChanged(nameof(ProgressMonth1GridLength));
                OnPropertyChanged(nameof(ProgressMonth2GridLength));
                OnPropertyChanged(nameof(ProgressMonth3GridLength));
                OnPropertyChanged(nameof(ProgressMonth1Color));
                OnPropertyChanged(nameof(ProgressMonth2Color));
                OnPropertyChanged(nameof(ProgressMonth3Color));
            }
        }
        
        /// <summary>
        /// 构建年度净利润对比图表数据
        /// </summary>
        private void BuildNetProfitComparisonChart(dynamic data, int lastYear, int currentYear)
        {
            try
            {
                NetProfitComparisonData.Clear();
                NetProfitComparisonYAxisLabels.Clear();
                
                LastYearLabel = $"{lastYear}年实际";
                CurrentYearLabel = $"{currentYear}年预测";
                
                decimal maxValue = 0;
                decimal minValue = 0;
                
                // 解析月度数据 - 新的数据结构
                var monthlyDataList = new List<NetProfitComparisonItem>();
                
                if (data is Newtonsoft.Json.Linq.JArray dataArray)
                {
                    foreach (var itemData in dataArray)
                    {
                        var item = new NetProfitComparisonItem
                        {
                            MonthLabel = GetSafeValue<string>(itemData as Newtonsoft.Json.Linq.JObject, "month", string.Empty),
                            AnalysisNetProfit = GetSafeValue<decimal>(itemData as Newtonsoft.Json.Linq.JObject, "analysisNetProfit", 0),
                            ForecastNetProfit = GetSafeValue<decimal>(itemData as Newtonsoft.Json.Linq.JObject, "forecastNetProfit", 0)
                        };
                        
                        // 更新最大值和最小值
                        if (item.AnalysisNetProfit > maxValue) maxValue = item.AnalysisNetProfit;
                        if (item.ForecastNetProfit > maxValue) maxValue = item.ForecastNetProfit;
                        if (item.AnalysisNetProfit < minValue) minValue = item.AnalysisNetProfit;
                        if (item.ForecastNetProfit < minValue) minValue = item.ForecastNetProfit;
                        
                        monthlyDataList.Add(item);
                    }
                }
                
                // 如果没有数据，创建空的
                if (monthlyDataList.Count == 0)
                {
                    for (int month = 1; month <= 12; month++)
                    {
                        var item = new NetProfitComparisonItem
                        {
                            MonthLabel = $"{month}月",
                            AnalysisNetProfit = 0,
                            ForecastNetProfit = 0
                        };
                        monthlyDataList.Add(item);
                    }
                }
                
                // 计算Y轴范围
                if (maxValue == 0 && minValue == 0)
                {
                    maxValue = 100000;
                    minValue = -100000;
                }
                else if (maxValue == 0)
                {
                    maxValue = Math.Abs(minValue);
                }
                else if (minValue == 0)
                {
                    minValue = -maxValue;
                }
                
                // 为了美观，添加一些边距
                decimal range = maxValue - minValue;
                decimal padding = range * 0.1m;
                decimal displayMax = maxValue + padding;
                decimal displayMin = minValue - padding;
                decimal displayRange = displayMax - displayMin;
                
                // 构建Y轴标签
                for (int i = 5; i >= 0; i--)
                {
                    decimal value = displayMin + displayRange * i / 5;
                    string label = Math.Abs(value) >= 10000 
                        ? $"{(value / 10000):F1}万" 
                        : $"{value:F0}";
                    NetProfitComparisonYAxisLabels.Add(label);
                }
                
                // 计算柱子高度
                double chartHeight = 240;
                
                foreach (var item in monthlyDataList)
                {
                    // 计算分析值高度和位置
                    if (item.AnalysisNetProfit >= 0)
                    {
                        // 正值，从基准线向上
                        double ratio = displayRange > 0 
                            ? (double)((item.AnalysisNetProfit - displayMin) / displayRange) 
                            : 0;
                        double zeroRatio = displayRange > 0 
                            ? (double)((0 - displayMin) / displayRange) 
                            : 0.5;
                        item.AnalysisHeight = (ratio - zeroRatio) * chartHeight;
                        item.AnalysisMarginTop = (1 - ratio) * chartHeight;
                    }
                    else
                    {
                        // 负值，从基准线向下
                        double ratio = displayRange > 0 
                            ? (double)((item.AnalysisNetProfit - displayMin) / displayRange) 
                            : 0;
                        double zeroRatio = displayRange > 0 
                            ? (double)((0 - displayMin) / displayRange) 
                            : 0.5;
                        item.AnalysisHeight = (zeroRatio - ratio) * chartHeight;
                        item.AnalysisMarginTop = (1 - zeroRatio) * chartHeight;
                    }
                    
                    // 计算预测值高度和位置
                    if (item.ForecastNetProfit >= 0)
                    {
                        // 正值，从基准线向上
                        double ratio = displayRange > 0 
                            ? (double)((item.ForecastNetProfit - displayMin) / displayRange) 
                            : 0;
                        double zeroRatio = displayRange > 0 
                            ? (double)((0 - displayMin) / displayRange) 
                            : 0.5;
                        item.ForecastHeight = (ratio - zeroRatio) * chartHeight;
                        item.ForecastMarginTop = (1 - ratio) * chartHeight;
                    }
                    else
                    {
                        // 负值，从基准线向下
                        double ratio = displayRange > 0 
                            ? (double)((item.ForecastNetProfit - displayMin) / displayRange) 
                            : 0;
                        double zeroRatio = displayRange > 0 
                            ? (double)((0 - displayMin) / displayRange) 
                            : 0.5;
                        item.ForecastHeight = (zeroRatio - ratio) * chartHeight;
                        item.ForecastMarginTop = (1 - zeroRatio) * chartHeight;
                    }
                    
                    // 设置完整的 Margin 属性
                    item.AnalysisMargin = new Thickness(0, item.AnalysisMarginTop, 2, 0);
                    item.ForecastMargin = new Thickness(2, item.ForecastMarginTop, 0, 0);
                    
                    // 设置工具提示文本
                    item.AnalysisTooltipText = $"{lastYear}年{item.MonthLabel}: ¥{item.AnalysisNetProfit:N0}";
                    item.ForecastTooltipText = $"{currentYear}年{item.MonthLabel}: ¥{item.ForecastNetProfit:N0}";
                    
                    NetProfitComparisonData.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 构建年度净利润对比图表异常: {ex.Message}");
                BuildEmptyNetProfitComparisonChart();
            }
        }
        
        /// <summary>
        /// 构建空的图表数据
        /// </summary>
        private void BuildEmptyNetProfitComparisonChart()
        {
            NetProfitComparisonData.Clear();
            NetProfitComparisonYAxisLabels.Clear();
            
            decimal displayMax = 100000;
            decimal displayMin = -100000;
            decimal displayRange = displayMax - displayMin;
            
            // 构建默认Y轴标签
            for (int i = 5; i >= 0; i--)
            {
                decimal value = displayMin + displayRange * i / 5;
                string label = Math.Abs(value) >= 10000 
                    ? $"{(value / 10000):F1}万" 
                    : $"{value:F0}";
                NetProfitComparisonYAxisLabels.Add(label);
            }
            
            // 添加12个空月份
            for (int month = 1; month <= 12; month++)
            {
                var item = new NetProfitComparisonItem
                {
                    MonthLabel = $"{month}月",
                    AnalysisNetProfit = 0,
                    ForecastNetProfit = 0,
                    AnalysisHeight = 0,
                    ForecastHeight = 0,
                    AnalysisMarginTop = 120, // 基准线在中间
                    ForecastMarginTop = 120,
                    AnalysisMargin = new Thickness(0, 120, 2, 0),
                    ForecastMargin = new Thickness(2, 120, 0, 0)
                };
                NetProfitComparisonData.Add(item);
            }
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
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始加载配置数据");
                
                string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
                long agentId = CurrentAgentId.Value;
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/saletarget/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 配置数据响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    
                    if (data != null)
                    {
                        ConfigTargetSales = (data["targetSales"]?.ToObject<long>() ?? 0).ToString();
                        System.Diagnostics.Debug.WriteLine($"[Yuce] ConfigTargetSales: {ConfigTargetSales}");
                        
                        GrowthRate = data["growthRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? (double)data["growthRate"].ToObject<decimal>() : 0;
                        System.Diagnostics.Debug.WriteLine($"[Yuce] GrowthRate: {GrowthRate}");
                    }
                    else
                    {
                        ResetConfigData();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 加载配置数据失败: {response.Message}");
                    ResetConfigData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yuce] 加载配置数据异常: {ex.Message}");
                ResetConfigData();
            }
        }

        private void ResetConfigData()
        {
            ConfigTargetSales = "0";
            GrowthRate = 0;
        }

        public async Task SaveSalesTargetAsync(long targetSales, double growthRate)
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不保存销售目标数据");
                StatusMessage = "请先选择代理商";
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始保存销售目标数据");
                
                string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
                long agentId = CurrentAgentId.Value;
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 接收到的参数 - targetSales={targetSales}, growthRate={growthRate}");
                
                decimal growthRateValue = (decimal)growthRate;
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 转换后 - targetSales={targetSales}, growthRateValue={growthRateValue}");
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    targetSales = targetSales,
                    growthRate = growthRateValue
                };
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存数据: agentId={agentId}, yearMonth={yearMonth}, targetSales={targetSales}, growthRate={growthRateValue}");
                System.Diagnostics.Debug.WriteLine($"[Yuce] requestData对象: {Newtonsoft.Json.JsonConvert.SerializeObject(requestData)}");
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/saletarget/saveRateSalesTarget",
                    requestData)
                    .ConfigureAwait(true);
                
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存响应码: {response.Code}, 响应消息: {response.Message}");
                
                if (response.Code == 200)
                {
                    StatusMessage = "保存成功";
                    System.Diagnostics.Debug.WriteLine("[Yuce] 保存成功");
                }
                else
                {
                    StatusMessage = $"保存失败: {response.Message}";
                    System.Diagnostics.Debug.WriteLine($"[Yuce] 保存失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Yuce] 保存异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载月度明细数据
        /// </summary>
        private async Task LoadDetailDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载月度明细数据");
                return;
            }

            try
            {
                StatusMessage = "正在加载数据...";
                Logger.Info($"开始加载月度明细数据");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                Logger.Info($"调用接口: /rate/detail/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/detail/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);

                Logger.Info($"响应码: {response.Code}, 响应消息: {response.Message}");
                Logger.Info($"响应数据类型: {response.Data?.GetType()?.FullName ?? "null"}");
                Logger.Info($"响应原始数据: {Newtonsoft.Json.JsonConvert.SerializeObject(response.Data)}");

                if (response.Code == 200 && response.Data != null)
                {
                    // 先清空列表
                    MonthlyDataList.Clear();
                    
                    // 检查返回的是数组还是单个对象
                    var jArray = response.Data as Newtonsoft.Json.Linq.JArray;
                    var jObject = response.Data as Newtonsoft.Json.Linq.JObject;

                    if (jArray != null)
                    {
                        // 返回的是数组
                        Logger.Info($"检测到返回数据为数组，共 {jArray.Count} 条记录");
                        
                        foreach (var item in jArray)
                        {
                            var dataItem = item as Newtonsoft.Json.Linq.JObject;
                            if (dataItem != null)
                            {
                                AddMonthlyDataItem(dataItem);
                            }
                        }
                    }
                    else if (jObject != null)
                    {
                        // 返回的是单个对象
                        Logger.Info($"检测到返回数据为单个对象");
                        
                        // 更新 KPI 卡片数据
                        MonthTargetSales = GetSafeValue<long>(jObject, "targetSales", 0);

                        var achievementRate = GetSafeValue<decimal>(jObject, "achievementRate", 0);
                        AchievementRate = (double)achievementRate;
                        AchievementRatePercent = (double)achievementRate * 100;  // 乘以100                      
                        ProgressValue = (double)achievementRate;
                        ProgressBarWidth = (double)achievementRate;

                        ActualSales = GetSafeValue<long>(jObject, "breakSales", 0);

                        var adjustedNetProfit = GetSafeValue<decimal>(jObject, "adjustedNetProfit", 0);
                        NetProfit = (double)adjustedNetProfit;

                        Logger.Info($"目标销量: {MonthTargetSales}, 达成率: {AchievementRate}, 实际销量: {ActualSales}, 净利润: {NetProfit}");

                        AddMonthlyDataItem(jObject);
                    }
                    else
                    {
                        StatusMessage = "响应数据格式不正确";
                        Logger.Warning("响应数据格式不正确，重置数据");
                        ResetDetailData();
                    }

                    Logger.Info($"数据加载完成，MonthlyDataList 条数: {MonthlyDataList.Count}");
                    StatusMessage = MonthlyDataList.Count > 0 ? "数据加载成功" : "暂无数据";
                }
                else
                {
                    StatusMessage = $"加载失败: {response.Message}";
                    Logger.Warning($"加载失败: {response.Message}");
                    ResetDetailData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据异常: {ex.Message}";
                Logger.Error($"加载数据异常: {ex.Message}", ex);
                Logger.Error($"异常堆栈: {ex.StackTrace}");
                ResetDetailData();
            }
        }
        
        /// <summary>
        /// 从JObject添加一条月度数据
        /// </summary>
        private void AddMonthlyDataItem(Newtonsoft.Json.Linq.JObject data)
        {
            var yearMonthStr = data["yearMonth"]?.ToString() ?? "";
            int month = SelectedMonth;
            if (!string.IsNullOrEmpty(yearMonthStr) && yearMonthStr.Contains("-"))
            {
                var monthPart = yearMonthStr.Split('-')[1];
                int.TryParse(monthPart, out month);
            }

            var targetSales = GetSafeValue<long>(data, "targetSales", 0);
            var achievementRate = GetSafeValue<decimal>(data, "achievementRate", 0);
            var improvedSales = GetSafeValue<long>(data, "improvedSales", 0);
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

            Logger.Info($"添加一条记录: AgentName={item.AgentName}, Month={item.Month}, TargetSales={item.TargetSales}");
            MonthlyDataList.Add(item);
        }

        /// <summary>
        /// 加载历史方案数据
        /// </summary>
        private async Task LoadHistoricalDataAsync(long agentId, string yearMonth)
        {
            try
            {
                StatusMessage = "正在加载历史方案数据...";
                Logger.Info($"调用接口: /rate/detailhistory/getByAgentIdAndYearMonth, agentId={agentId}, yearMonth={yearMonth}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/detailhistory/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}")
                    .ConfigureAwait(true);
                
                Logger.Info($"响应码: {response.Code}, 响应消息: {response.Message}");
                Logger.Info($"响应数据类型: {response.Data?.GetType()?.FullName ?? "null"}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    // 先清空历史数据列表
                    HistoricalMonthlyDataList.Clear();
                    
                    var jArray = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (jArray != null)
                    {
                        Logger.Info($"检测到返回数据为数组，共 {jArray.Count} 条记录");
                        
                        foreach (var item in jArray)
                        {
                            var dataItem = item as Newtonsoft.Json.Linq.JObject;
                            if (dataItem != null)
                            {
                                AddHistoricalDataItem(agentId, yearMonth, dataItem);
                            }
                        }
                        
                        StatusMessage = $"加载成功，共 {jArray.Count} 条历史记录";
                        Logger.Info($"历史方案数据加载完成");
                    }
                }
                else
                {
                    StatusMessage = "暂无历史方案数据";
                    Logger.Warning($"历史方案数据为空或加载失败");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载历史方案失败: {ex.Message}";
                Logger.Error($"加载历史方案数据异常: {ex}");
            }
        }

        /// <summary>
        /// 添加历史方案数据项
        /// </summary>
        private void AddHistoricalDataItem(long agentId, string yearMonth, Newtonsoft.Json.Linq.JObject data)
        {
            var yearMonthStr = data["yearMonth"]?.ToString() ?? yearMonth;
            int month = SelectedMonth;
            if (!string.IsNullOrEmpty(yearMonthStr) && yearMonthStr.Contains("-"))
            {
                var monthPart = yearMonthStr.Split('-')[1];
                int.TryParse(monthPart, out month);
            }

            var targetSales = GetSafeValue<long>(data, "targetSales", 0);
            var achievementRate = GetSafeValue<decimal>(data, "achievementRate", 0);
            var improvedSales = GetSafeValue<long>(data, "improvedSales", 0);
            var salesGrowth = GetSafeValue<decimal>(data, "salesGrowthProfit", 0);
            var structureOptimize = GetSafeValue<decimal>(data, "structureOptimizeProfit", 0);
            var premium = GetSafeValue<decimal>(data, "premiumProfit", 0);
            var totalExtra = GetSafeValue<decimal>(data, "totalExtraProfit", 0);
            var adjustedNetProfit = GetSafeValue<decimal>(data, "adjustedNetProfit", 0);
            var versionId = GetSafeValue<long>(data, "versionId", 0);
            var createTime = GetSafeValue<DateTime>(data, "createTime", DateTime.Now);
            var item = new HistoricalMonthlyDataItem
            {
                AgentId = agentId,
                YearMonth = yearMonthStr,
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
                AdjustedNetProfit = (double)adjustedNetProfit,
                VersionId = versionId,
                CreateTime = createTime
            };

            Logger.Info($"添加一条历史记录: AgentName={item.AgentName}, Month={item.Month}, VersionId={item.VersionId}");
            HistoricalMonthlyDataList.Add(item);
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        private void ResetDetailData()
        {
            MonthTargetSales = 0;
            AchievementRate = 0;
            AchievementRatePercent = 0;
            ProgressValue = 0;
            ProgressBarWidth = 0;
            ActualSales = 0;
            NetProfit = 0;
            MonthlyDataList.Clear();
        }

        /// <summary>
        /// 加载年度明细数据
        /// </summary>
        private async Task LoadYearlyDetailDataAsync()
        {
            if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
            {
                Logger.Warning("CurrentAgentId 无效，不加载年度明细数据");
                return;
            }

            try
            {
                StatusMessage = "正在加载年度数据...";
                Logger.Info($"开始加载年度明细数据");

                int year = SelectedYear;
                long agentId = CurrentAgentId.Value;

                Logger.Info($"调用接口: /rate/detail/getByAgentIdAndYear, agentId={agentId}, year={year}");

                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/detail/getByAgentIdAndYear?agentId={agentId}&year={year}")
                    .ConfigureAwait(true);

                Logger.Info($"响应码: {response.Code}, 响应消息: {response.Message}");
                Logger.Info($"响应数据类型: {response.Data?.GetType()?.FullName ?? "null"}");
                Logger.Info($"响应原始数据: {Newtonsoft.Json.JsonConvert.SerializeObject(response.Data)}");

                if (response.Code == 200 && response.Data != null)
                {
                    // 先清空列表
                    MonthlyDataList.Clear();
                    
                    // 检查返回的是数组还是单个对象
                    var jArray = response.Data as Newtonsoft.Json.Linq.JArray;
                    var jObject = response.Data as Newtonsoft.Json.Linq.JObject;

                    if (jArray != null)
                    {
                        // 返回的是数组
                        Logger.Info($"检测到返回数据为数组，共 {jArray.Count} 条记录");
                        
                        foreach (var item in jArray)
                        {
                            var dataItem = item as Newtonsoft.Json.Linq.JObject;
                            if (dataItem != null)
                            {
                                AddYearlyDataItem(dataItem);
                            }
                        }
                    }
                    else if (jObject != null)
                    {
                        // 返回的是单个对象
                        Logger.Info($"检测到返回数据为单个对象");
                        
                        AddYearlyDataItem(jObject);
                    }
                    else
                    {
                        StatusMessage = "响应数据格式不正确";
                        Logger.Warning("响应数据格式不正确，重置数据");
                        ResetYearlyDetailData();
                    }

                    Logger.Info($"数据加载完成，MonthlyDataList 条数: {MonthlyDataList.Count}");
                    StatusMessage = MonthlyDataList.Count > 0 ? "数据加载成功" : "暂无数据";
                }
                else
                {
                    StatusMessage = $"加载失败: {response.Message}";
                    Logger.Warning($"加载失败: {response.Message}");
                    ResetYearlyDetailData();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据异常: {ex.Message}";
                Logger.Error($"加载数据异常: {ex.Message}", ex);
                Logger.Error($"异常堆栈: {ex.StackTrace}");
                ResetYearlyDetailData();
            }
        }
        
        /// <summary>
        /// 从JObject添加一条年度数据
        /// </summary>
        private void AddYearlyDataItem(Newtonsoft.Json.Linq.JObject data)
        {
            // 更新 KPI 卡片数据 - 使用更安全的方式访问 JObject 属性
            YearTargetSales = GetSafeValue<long>(data, "targetSales", 0);

            var breakEvenRate = GetSafeValue<decimal>(data, "achievementRate", 0);
            BreakEvenRate = (double)breakEvenRate;

            OptimizedSales = GetSafeValue<long>(data, "improvedSales", 0);

            var adjustedNetProfit = GetSafeValue<decimal>(data, "adjustedNetProfit", 0);
            AdjustedNetProfit = (double)adjustedNetProfit;

            Logger.Info($"年目标销量: {YearTargetSales}, 达成率: {BreakEvenRate}, 提升后销量: {OptimizedSales}, 调整后净利润: {AdjustedNetProfit}");

            var salesGrowth = GetSafeValue<decimal>(data, "salesGrowthProfit", 0);
            var structureOptimize = GetSafeValue<decimal>(data, "structureOptimizeProfit", 0);
            var premium = GetSafeValue<decimal>(data, "premiumProfit", 0);
            var totalExtra = GetSafeValue<decimal>(data, "totalExtraProfit", 0);
            var totalExpense = GetSafeValue<decimal>(data, "totalExpense", 0);

            // 从 yearMonth 中提取月份
            var yearMonthStr = data["yearMonth"]?.ToString() ?? "";
            int month = 0;
            if (!string.IsNullOrEmpty(yearMonthStr))
            {
                if (yearMonthStr.Contains("-"))
                {
                    var monthPart = yearMonthStr.Split('-')[1];
                    int.TryParse(monthPart, out month);
                }
                else if (int.TryParse(yearMonthStr, out int yearNum))
                {
                    // 可能是纯年份，设为0表示汇总
                    month = 0;
                }
            }

            // 添加数据
            var item = new MonthlyDataItem
            {
                AgentName = data["agentName"]?.ToString() ?? "",
                Month = month,
                YearTargetSales = YearTargetSales,
                BreakEvenRate = BreakEvenRate,
                OptimizedSales = OptimizedSales,
                SalesGrowthProfit = (double)salesGrowth,
                StructureOptimizeProfit = (double)structureOptimize,
                PremiumProfit = (double)premium,
                TotalExtraProfit = (double)totalExtra,
                AdjustedNetProfit = (double)adjustedNetProfit,
                TotalExpense = (double)totalExpense
            };

            Logger.Info($"添加一条记录: AgentName={item.AgentName}, Month={item.Month}");
            MonthlyDataList.Add(item);
        }

        /// <summary>
        /// 重置年度数据
        /// </summary>
        private void ResetYearlyDetailData()
        {
            YearTargetSales = 0;
            BreakEvenRate = 0;
            OptimizedSales = 0;
            AdjustedNetProfit = 0;
            TotalExpense = 0;
            MonthlyDataList.Clear();
        }

        /// <summary>
        /// 加载代理商列表
        /// </summary>
        private async Task LoadAgentListAsync()
        {
            try
            {
                Console.WriteLine("\n\n========================================");
                Console.WriteLine("开始加载代理商列表");
                Console.WriteLine("========================================\n");
                
                Logger.Separator("加载代理商列表");
                Logger.Info("正在加载代理商列表...");
                
                // 检查是否有Token
                var currentToken = NewApiClient.GetAuthToken();
                if (string.IsNullOrEmpty(currentToken))
                {
                    Logger.Warning("当前没有认证Token，请先登录");
                    Console.WriteLine("⚠ 当前没有认证Token，请先登录");
                    return;
                }
                Logger.Info($"当前Token: {currentToken.Substring(0, Math.Min(30, currentToken.Length))}...");
                
                var response = await NewApiClient.GetAsync<ObservableCollection<AgentUser>>("/rate/default/getMyAgentListWithCount");
                
                // 打印原始返回数据 - 使用Console.WriteLine确保一定能看到
                Console.WriteLine("\n========== 代理商列表接口返回数据 ==========");
                Logger.Info("代理商列表接口返回数据:");
                if (response.Data != null)
                {
                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data, Newtonsoft.Json.Formatting.Indented);
                    Console.WriteLine(jsonData);
                    Logger.Info(jsonData);
                }
                else
                {
                    Console.WriteLine("⚠ 返回数据为空");
                    Logger.Warning("返回数据为空");
                }
                Console.WriteLine("=========================================\n");
                
                if (response.Code == 200 && response.Data != null)
                {
                    AgentList.Clear();
                    foreach (var agent in response.Data)
                    {
                        AgentList.Add(agent);
                    }
                    
                    // 默认选中第一个
                    if (AgentList.Count > 0)
                    {
                        SelectedAgent = AgentList[0];
                        Logger.Info($"默认选中代理商: {SelectedAgent.DisplayName}");
                        Console.WriteLine($"✓ 默认选中代理商: {SelectedAgent.DisplayName}");
                    }
                    
                    Logger.Success($"成功加载 {AgentList.Count} 个代理商");
                    Console.WriteLine($"✅ 成功加载 {AgentList.Count} 个代理商");
                    Logger.Separator("加载代理商列表完成");
                }
                else
                {
                    Logger.Error($"加载代理商列表失败: {response.Message}");
                    Console.WriteLine($"❌ 加载代理商列表失败: {response.Message}");
                    Logger.Separator("加载代理商列表完成");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载代理商列表异常: {ex.Message}", ex);
                Console.WriteLine($"❌ 加载代理商列表异常: {ex.Message}");
                Logger.Separator("加载代理商列表完成");
            }
        }

        /// <summary>
        /// 加载部门树（战区、渠道部、业务经理）
        /// </summary>
        private async Task LoadDeptTreeAsync()
        {
            try
            {
                Console.WriteLine("\n========== 开始加载部门树 ==========");
                Logger.Info("正在加载部门树...");
                
                var response = await NewApiClient.GetAsync<ObservableCollection<DeptInfo>>("/system/user/deptTree");
                
                Console.WriteLine("\n========== 部门树接口返回数据 ==========");
                if (response.Data != null)
                {
                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(response.Data, Newtonsoft.Json.Formatting.Indented);
                    Console.WriteLine(jsonData);
                    Logger.Info(jsonData);
                }
                Console.WriteLine("=========================================\n");
                
                if (response.Code == 200 && response.Data != null)
                {
                    // 清空现有数据
                    WarZoneList.Clear();
                    ChannelDeptList.Clear();
                    RegionManagerList.Clear();
                    ManagerList.Clear();
                    
                    // 解析部门树结构
                    // 第1级：公司（超威科技）- 跳过
                    // 第2级：部门（营销中心）- 跳过
                    // 第3级：战区 - 加载到 WarZoneList
                    // 第4级：渠道部 - 根据选中的战区动态加载
                    // 第5级：大区（大区经理）- 根据选中的渠道部动态加载
                    // 第6级及以下：业务经理 - 根据前面选择的部门查询
                    
                    foreach (var company in response.Data)  // 第1级：公司
                    {
                        foreach (var dept in company.Children)  // 第2级：部门
                        {
                            // 从第3级开始处理 - 只加载战区列表
                            foreach (var warZone in dept.Children)  // 第3级：战区
                            {
                                WarZoneList.Add(warZone);
                                Console.WriteLine($"  添加战区: {warZone.DisplayName}");
                            }
                        }
                    }
                    
                    Console.WriteLine($"\nWarZoneList 最终数量: {WarZoneList.Count}");
                    Logger.Success($"成功加载部门树: {WarZoneList.Count} 个战区");
                    Console.WriteLine($"✅ 成功加载: {WarZoneList.Count} 个战区");
                }
                else
                {
                    Logger.Error($"加载部门树失败: {response.Message}");
                    Console.WriteLine($"❌ 加载部门树失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载部门树异常: {ex.Message}", ex);
                Console.WriteLine($"❌ 加载部门树异常: {ex.Message}");
            }
        }



        [RelayCommand]
        private void SaveBasicConfig()
        {
            Logger.Info("保存基础配置...");
            // TODO: 调用 API 保存基础配置
            Logger.Success("基础配置保存成功！");
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
                var result = await dialog.ShowDialog(App.MainWindow!);
                
                if (result != null)
                {
                    Logger.Info($"产品选择器返回: Model={result.Model}, ItemId={result.ItemId}");
                    
                    // 创建新的产品结构项（而不是 ProductItem）
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
                    await SaveProductStructureAsync(newStructure);
                    Logger.Info($"创建 ProductStructureDto 成功，准备添加到列表");
                    
                    // 检查是否在 UI 线程
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Logger.Info("当前在 UI 线程，直接添加");
                        ProductStructures.Add(newStructure);
                        
                        ProductCount = ProductStructures.Count;
                        Logger.Success($"已添加产品: {result.Model}，当前产品数量: {ProductCount}");
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
                            
                            ProductCount = ProductStructures.Count;
                            Logger.Success($"异步添加产品: {result.Model}，当前产品数量: {ProductCount}");
                            StatusMessage = $"已添加产品: {result.Model}";
                            
                            // 更新占比总和和结构优化总和
                            UpdateProportionSum();
                            UpdateOptimizationSum();
                        });
                    }
                    
                    // TODO: 调用 API 保存产品配置
                    // await SaveProductConfigAsync(newStructure);
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

        [RelayCommand]
        private async Task AddExpenseItem()
        {
            try
            {
                Logger.Info("开始添加收益项目");
                
                // 检查是否选择了代理商
                if (!CurrentAgentId.HasValue || CurrentAgentId.Value <= 0)
                {
                    Logger.Warning("未选择代理商，无法添加收益项目");
                    StatusMessage = "请先选择代理商";
                    return;
                }
                
                // 显示对话框
                var dialog = new AgentManagement.Avalonia.Views.rate.yuce.AddExpenseItemDialog();
                var viewModel = new AddExpenseItemDialogViewModel();
                dialog.DataContext = viewModel;
                
                bool isConfirmed = false;
                
                viewModel.OnConfirm += async (expenseType, amount, isIncome) =>
                {
                    isConfirmed = true;
                    dialog.Close();
                    
                    // 保存收益项目
                    await SaveExpenseAsync(expenseType, amount, isIncome);
                    
                    // 刷新数据
                    string yearMonth = IsMonthlyView ? $"{SelectedYear}-{SelectedMonth:D2}" : SelectedYear.ToString();
                    await LoadExpenseDataAsync(CurrentAgentId.Value, yearMonth);
                    
                    Logger.Success($"已添加项目: {expenseType}, 金额: {amount}, 类型: {(isIncome == 1 ? "收益" : "固定成本")}");
                    StatusMessage = $"已添加项目: {expenseType}";
                };
                
                await dialog.ShowDialog(App.MainWindow!);
            }
            catch (Exception ex)
            {
                Logger.Error($"添加收益项目异常: {ex.Message}", ex);
                StatusMessage = $"添加收益项目失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ViewDetail(HistoricalMonthlyDataItem item)
        {
            try
            {
                Logger.Info($"查看历史方案详情: AgentId={item.AgentId}, YearMonth={item.YearMonth}, VersionId={item.VersionId}");
                
                var dialog = new AgentManagement.Avalonia.Views.rate.yuce.DetailHistoryDialog();
                var viewModel = new DetailHistoryDialogViewModel(item.AgentId, item.YearMonth, item.VersionId);
                dialog.DataContext = viewModel;
                
                await viewModel.LoadDataAsync();
                await dialog.ShowDialog(App.MainWindow!);
            }
            catch (Exception ex)
            {
                Logger.Error($"查看历史方案详情异常: {ex.Message}", ex);
                StatusMessage = $"查看方案详情失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 更新产品占比总和显示
        /// </summary>
        private void UpdateProportionSum()
        {
            var sum = ProductStructures.Sum(p => (double)p.StructureRatio);
            ProportionSumText = $"产品占比总和: {sum:F2}";
            
            if (Math.Abs(sum - 1.0) < 0.005)//这儿有浮点误差，先放大
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
            
            if (Math.Abs(sum - 0.0) < 0.005)//这儿有浮点误差，先放大
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
        /// 编辑代理商产品配置事件
        /// </summary>
        public event Action<AgentUser>? EditAgentProductsRequested;

        [RelayCommand]
        private void EditAgentProducts(AgentUser agent)
        {
            if (agent != null)
            {
                Logger.Info($"请求编辑代理商产品配置: {agent.DisplayName}");
                
                // 触发事件，由 View 层处理对话框显示
                EditAgentProductsRequested?.Invoke(agent);
            }
        }

        [RelayCommand]
        private void DeleteProduct(ProductStructureDto product)
        {
            if (product != null && ProductStructures.Contains(product))
            {
                ProductStructures.Remove(product);
                ProductCount = ProductStructures.Count;
                Logger.Info($"已删除产品: {product.ItemModel}");
            }
        }

        [RelayCommand]
        private void SaveProductConfig()
        {
            Logger.Info("保存产品配置...");
            // TODO: 调用 API 保存产品配置
            Logger.Success("产品配置保存成功！");
        }



        [RelayCommand]
        private void PreviewYearlyChanges()
        {
            Logger.Info("预览年度变更效果...");
            // TODO: 计算并显示预览结果
            Logger.Success("预览完成！");
        }

        [RelayCommand]
        private void ApplyYearlyChanges()
        {
            Logger.Info("应用年度变更...");
            // TODO: 应用变更到正式数据
            Logger.Success("变更已应用！");
        }

        [RelayCommand]
        private async Task ExportYearlyData()
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
                Logger.Info("开始导出年度数据...");
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始导出年度数据");

                int year = SelectedYear;
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    year = year
                };

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/detail/export",
                    requestData);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = $"detailyear_export_{year}.xlsx";

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

                    Logger.Success($"数据导出成功！保存位置: {filePath}");
                    StatusMessage = $"导出成功！文件已保存到：{filePath}";

                    var box = MessageBoxManager.GetMessageBoxStandard("导出成功", $"文件已成功保存到：\n{filePath}", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    StatusMessage = "导出失败：未获取到文件数据";
                    System.Diagnostics.Debug.WriteLine("[Yuce] 导出失败：未获取到文件数据");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出数据失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Yuce] 导出异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Yuce] 异常堆栈: {ex.StackTrace}");
                Logger.Error($"导出年度数据异常: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        private void Calculate()
        {
            StatusMessage = "正在预测计算...";
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        [RelayCommand]
        private async Task ImportExcel()
        {
            try
            {
                Logger.Info("开始 Excel 导入流程...");

                // 1. 先弹出文件选择对话框
                var filePath = await FileService.ShowOpenFileDialogAsync("选择Excel文件", "Excel", "All");
                if (string.IsNullOrEmpty(filePath))
                {
                    Logger.Info("用户取消了文件选择");
                    return;
                }

                Logger.Info($"选择的文件: {filePath}");

                // 2. 弹出导入选项对话框（选择年份和覆盖选项）
                var dialog = new Views.ImportOptionsDialog();
                var dialogResult = await dialog.ShowDialogAsync(App.MainWindow);

                if (dialogResult != true || dialog.Result == null)
                {
                    Logger.Info("用户取消了导入选项");
                    return;
                }

                // 3. 获取用户选择的年份和覆盖选项
                var selectedYear = dialog.Result.Year;
                var isOverwrite = dialog.Result.Overwrite;

                Logger.Info($"用户选择：年份={selectedYear}，是否覆盖={isOverwrite}");

                // 4. 构造 multipart/form-data 请求
                var content = new MultipartFormDataContent();
                var fileStream = File.OpenRead(filePath);
                content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
                content.Add(new StringContent(isOverwrite ? "1" : "0"), "updateSupport");
                content.Add(new StringContent(selectedYear.ToString()), "year");

                // 5. 调用接口上传文件
                var apiClient = NewApiClient.GetHttpClient();
                var response = await apiClient.PostAsync($"{NewApiClient.BaseUrl}/rate/saletarget/importData", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Logger.Success($"Excel 导入成功！年份: {selectedYear}，覆盖: {isOverwrite}");
                    Logger.Info($"响应内容: {responseContent}");
                    
                    // 解析响应消息，提取关键信息
                    try
                    {
                        var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);
                        string msg = jsonResponse?.msg?.ToString() ?? "导入成功";
                        
                        // 提取关键信息（共多少条）
                        string summary = "数据导入成功！";
                        if (msg.Contains("共") && msg.Contains("条"))
                        {
                            // 提取 "共 X 条" 的信息
                            var match = System.Text.RegularExpressions.Regex.Match(msg, @"共\s*(\d+)\s*条");
                            if (match.Success)
                            {
                                summary = $"数据导入成功！共 {match.Groups[1].Value} 条记录";
                            }
                        }
                        
                        // 显示简洁的成功提示框
                        await Views.MessageDialog.ShowSuccessAsync(
                            App.MainWindow!,
                            "导入成功",
                            summary
                        );
                    }
                    catch
                    {
                        // 如果解析失败，显示简单提示
                        await Views.MessageDialog.ShowSuccessAsync(
                            App.MainWindow!,
                            "导入成功",
                            "数据导入成功！"
                        );
                    }
                    
                    // TODO: 导入成功后可以刷新数据
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Excel 导入失败：{(int)response.StatusCode} - {errorContent}");
                    
                    // 显示错误提示框
                    await Views.MessageDialog.ShowErrorAsync(
                        App.MainWindow!,
                        "导入失败",
                        $"导入失败：{(int)response.StatusCode}\n{errorContent}"
                    );
                }

                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Excel 导入异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 下载导入模板
        /// </summary>
        [RelayCommand]
        private async Task DownloadImportTemplate()
        {
            try
            {
                Logger.Info("开始下载导入模板...");
                
                // 调用接口下载模板（使用新的 DownloadFileAsync 方法）
                var fileBytes = await NewApiClient.DownloadFileAsync("/rate/saletarget/importTemplate", "POST");
                
                // 显示保存文件对话框，让用户选择保存位置
                var filePath = await FileService.ShowSaveFileDialogAsync(
                    "保存导入模板",
                    "导入模板.xlsx",
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
                
                // 打开文件所在文件夹
                try
                {
                    var directory = System.IO.Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"\"{directory}\"");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"无法打开文件夹: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"模板下载异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 导出月度数据
        /// </summary>
        [RelayCommand]
        private async Task ExportMonthlyData()
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
                Logger.Info("开始导出月度数据...");
                System.Diagnostics.Debug.WriteLine("[Yuce] 开始导出月度数据");

                string yearMonth = $"{SelectedYear}-{SelectedMonth:D2}";
                long agentId = CurrentAgentId.Value;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth
                };

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/detail/export",
                    requestData);

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = $"detail_export_{yearMonth}.xlsx";

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

                    Logger.Success($"数据导出成功！保存位置: {filePath}");
                    StatusMessage = $"导出成功！文件已保存到：{filePath}";

                    var box = MessageBoxManager.GetMessageBoxStandard("导出成功", $"文件已成功保存到：\n{filePath}", ButtonEnum.Ok);
                    await box.ShowAsync();
                }
                else
                {
                    StatusMessage = "导出失败：未获取到文件数据";
                    System.Diagnostics.Debug.WriteLine("[Yuce] 导出失败：未获取到文件数据");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出数据失败: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[Yuce] 导出异常: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Yuce] 异常堆栈: {ex.StackTrace}");
                Logger.Error($"导出月度数据异常: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 查看归因瀑布
        /// </summary>
        [RelayCommand]
        private async Task ViewAttributionWaterfall()
        {
            Logger.Info("查看归因瀑布图...");
                
            // TODO: 从选中行获取数据，目前使用模拟数据
            if (MonthlyDataList == null || MonthlyDataList.Count == 0)
            {
                Logger.Warning("没有数据可展示");
                return;
            }
                
            // 使用第一行数据作为示例
            var firstMonth = MonthlyDataList[0];
                
            // 创建抽屉面板 ViewModel
            var drawerViewModel = new MonthDetailDrawerViewModel();
            drawerViewModel.LoadMockData(firstMonth);
                
            // 创建窗口
            var window = new Views.rate.yuce.MonthDetailDrawer
            {
                DataContext = drawerViewModel
            };
                
            // 显示窗口
            await window.ShowDialog(App.MainWindow!);
        }

        [RelayCommand]
        private async Task EditQuarterlyDetail(RateMonthlyDetailQuarterly data)
        {
            Logger.Info($"编辑季度明细: {data.YearMonth}");

            var dialog = new Views.rate.yuce.QuarterlyDetailEditDialog();
            var viewModel = new QuarterlyDetailEditDialogViewModel(data);
            await viewModel.LoadDataAsync();
            
            viewModel.OnSave = async () =>
            {
                Logger.Info("保存季度明细修改，刷新季度视图数据");
                if (CurrentAgentId.HasValue && CurrentAgentId.Value > 0)
                {
                    await LoadQuarterlyStatsDataAsync(CurrentAgentId.Value);
                    await LoadQuarterlyMonthlyListAsync(CurrentAgentId.Value);
                    await LoadQuarterlyListInfoAsync(CurrentAgentId.Value);
                }
            };
            viewModel.OnCancel = () =>
            {
                Logger.Info("取消季度明细修改");
            };
            viewModel.OnClose = () =>
            {
                dialog.Close();
            };

            dialog.DataContext = viewModel;

            await dialog.ShowDialog(App.MainWindow!);
        }
        
        
        /// <summary>
        /// 从 JObject 安全获取值，防止空引用异常
        /// </summary>
        private T GetSafeValue<T>(Newtonsoft.Json.Linq.JObject data, string propertyName, T defaultValue)
        {
            try
            {
                var token = data[propertyName];
                if (token == null || token.Type == Newtonsoft.Json.Linq.JTokenType.Null)
                {
                    return defaultValue;
                }
                
                return token.ToObject<T>();
            }
            catch (Exception ex)
            {
                Logger.Warning($"获取属性 {propertyName} 失败: {ex.Message}");
                return defaultValue;
            }
        }
    }

    public partial class MonthlyDataItem : ObservableObject
    {
        private string _agentName = string.Empty;
        public string AgentName
        {
            get => _agentName;
            set => SetProperty(ref _agentName, value);
        }

        private int _month;
        public int Month
        {
            get => _month;
            set
            {
                SetProperty(ref _month, value);
                OnPropertyChanged(nameof(MonthDisplay));
            }
        }

        public string MonthDisplay
        {
            get
            {
                if (Month == 0)
                {
                    return "年度汇总";
                }
                return $"{Month}月";
            }
        }

        private double _yearTargetSales;
        public double YearTargetSales
        {
            get => _yearTargetSales;
            set => SetProperty(ref _yearTargetSales, value);
        }

        private double _targetSales;
        public double TargetSales
        {
            get => _targetSales;
            set => SetProperty(ref _targetSales, value);
        }

        private double _breakEvenRate;
        public double BreakEvenRate
        {
            get => _breakEvenRate;
            set => SetProperty(ref _breakEvenRate, value);
        }

        private double _breakEvenRatePercent;
        public double BreakEvenRatePercent
        {
            get => _breakEvenRatePercent;
            set => SetProperty(ref _breakEvenRatePercent, value);
        }
        private double _optimizedSales;
        public double OptimizedSales
        {
            get => _optimizedSales;
            set => SetProperty(ref _optimizedSales, value);
        }

        private double _salesGrowthProfit;
        public double SalesGrowthProfit
        {
            get => _salesGrowthProfit;
            set => SetProperty(ref _salesGrowthProfit, value);
        }

        private double _structureOptimizeProfit;
        public double StructureOptimizeProfit
        {
            get => _structureOptimizeProfit;
            set => SetProperty(ref _structureOptimizeProfit, value);
        }

        private double _premiumProfit;
        public double PremiumProfit
        {
            get => _premiumProfit;
            set => SetProperty(ref _premiumProfit, value);
        }

        private double _totalExtraProfit;
        public double TotalExtraProfit
        {
            get => _totalExtraProfit;
            set => SetProperty(ref _totalExtraProfit, value);
        }

        private double _adjustedNetProfit;
        public double AdjustedNetProfit
        {
            get => _adjustedNetProfit;
            set => SetProperty(ref _adjustedNetProfit, value);
        }

        private double _totalExpense;
        public double TotalExpense
        {
            get => _totalExpense;
            set => SetProperty(ref _totalExpense, value);
        }
    }

    public partial class ProductOptimizationItem : ObservableObject
    {
        [ObservableProperty]
        private string _model = string.Empty;

        [ObservableProperty]
        private double _currentProportion;

        [ObservableProperty]
        private double _optimizedProportion;

        [ObservableProperty]
        private double _currentGroupPrice;

        [ObservableProperty]
        private double _optimizedGroupPrice;

        [ObservableProperty]
        private double _commission;
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

    public partial class ExpenseItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _expenseType = string.Empty;

        [ObservableProperty]
        private decimal _amount = 0;

        [ObservableProperty]
        private int _isIncome = 0;
    }

    public partial class ExpenseDto
    {
        public string ExpenseType { get; set; }
        public decimal Amount { get; set; }
        public int IsIncome { get; set; }
    }
    
    public partial class HistoricalMonthlyDataItem : ObservableObject
    {
        // 继承MonthlyDataItem的所有字段
        private string _agentName = string.Empty;
        public string AgentName
        {
            get => _agentName;
            set => SetProperty(ref _agentName, value);
        }

        private int _month;
        public int Month
        {
            get => _month;
            set
            {
                SetProperty(ref _month, value);
                OnPropertyChanged(nameof(MonthDisplay));
            }
        }

        public string MonthDisplay
        {
            get
            {
                if (Month == 0)
                {
                    return "年度汇总";
                }
                return $"{Month}月";
            }
        }

        private double _yearTargetSales;
        public double YearTargetSales
        {
            get => _yearTargetSales;
            set => SetProperty(ref _yearTargetSales, value);
        }

        private double _targetSales;
        public double TargetSales
        {
            get => _targetSales;
            set => SetProperty(ref _targetSales, value);
        }

        private double _breakEvenRate;
        public double BreakEvenRate
        {
            get => _breakEvenRate;
            set => SetProperty(ref _breakEvenRate, value);
        }

        private double _breakEvenRatePercent;
        public double BreakEvenRatePercent
        {
            get => _breakEvenRatePercent;
            set => SetProperty(ref _breakEvenRatePercent, value);
        }

        private double _optimizedSales;
        public double OptimizedSales
        {
            get => _optimizedSales;
            set => SetProperty(ref _optimizedSales, value);
        }

        private double _salesGrowthProfit;
        public double SalesGrowthProfit
        {
            get => _salesGrowthProfit;
            set => SetProperty(ref _salesGrowthProfit, value);
        }

        private double _structureOptimizeProfit;
        public double StructureOptimizeProfit
        {
            get => _structureOptimizeProfit;
            set => SetProperty(ref _structureOptimizeProfit, value);
        }

        private double _premiumProfit;
        public double PremiumProfit
        {
            get => _premiumProfit;
            set => SetProperty(ref _premiumProfit, value);
        }

        private double _totalExtraProfit;
        public double TotalExtraProfit
        {
            get => _totalExtraProfit;
            set => SetProperty(ref _totalExtraProfit, value);
        }

        private double _adjustedNetProfit;
        public double AdjustedNetProfit
        {
            get => _adjustedNetProfit;
            set => SetProperty(ref _adjustedNetProfit, value);
        }
        
        // 历史方案新增的字段
        private long _agentId;
        public long AgentId
        {
            get => _agentId;
            set => SetProperty(ref _agentId, value);
        }

        private string _yearMonth = string.Empty;
        public string YearMonth
        {
            get => _yearMonth;
            set => SetProperty(ref _yearMonth, value);
        }

        private long _versionId;
        public long VersionId
        {
            get => _versionId;
            set => SetProperty(ref _versionId, value);
        }

        private DateTime _createTime;
        public DateTime CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }
    }
    
    public partial class NetProfitComparisonItem : ObservableObject
    {
        [ObservableProperty]
        private string _monthLabel = string.Empty;
        
        [ObservableProperty]
        private decimal _analysisNetProfit;
        
        [ObservableProperty]
        private decimal _forecastNetProfit;
        
        [ObservableProperty]
        private double _analysisHeight;
        
        [ObservableProperty]
        private double _forecastHeight;
        
        [ObservableProperty]
        private double _analysisMarginTop; // 负值时为正，正值时为0
        
        [ObservableProperty]
        private double _forecastMarginTop; // 负值时为正，正值时为0
        
        [ObservableProperty]
        private Thickness _analysisMargin;
        
        [ObservableProperty]
        private Thickness _forecastMargin;
        
        [ObservableProperty]
        private string _analysisTooltipText = string.Empty;
        
        [ObservableProperty]
        private string _forecastTooltipText = string.Empty;
    }
}
