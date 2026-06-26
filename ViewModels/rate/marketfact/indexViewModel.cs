using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.ViewModels.rate.marketfact
{
    public class SalesComparisonItem : ObservableObject
    {
        private string _seriesName;
        public string SeriesName
        {
            get => _seriesName;
            set => SetProperty(ref _seriesName, value);
        }

        private decimal _marketTotal;
        public decimal MarketTotal
        {
            get => _marketTotal;
            set => SetProperty(ref _marketTotal, value);
        }

        private decimal _cwSales;
        public decimal CwSales
        {
            get => _cwSales;
            set => SetProperty(ref _cwSales, value);
        }

        private string _cwRatio = "--";
        public string CwRatio
        {
            get => _cwRatio;
            set => SetProperty(ref _cwRatio, value);
        }

        private decimal _tnSales;
        public decimal TnSales
        {
            get => _tnSales;
            set => SetProperty(ref _tnSales, value);
        }

        private string _tnRatio = "--";
        public string TnRatio
        {
            get => _tnRatio;
            set => SetProperty(ref _tnRatio, value);
        }

        private decimal _smallBrandSales;
        public decimal SmallBrandSales
        {
            get => _smallBrandSales;
            set => SetProperty(ref _smallBrandSales, value);
        }

        private string _smallBrandRatio = "--";
        public string SmallBrandRatio
        {
            get => _smallBrandRatio;
            set => SetProperty(ref _smallBrandRatio, value);
        }

        private string _cwMinusTn = "--";
        public string CwMinusTn
        {
            get => _cwMinusTn;
            set
            {
                SetProperty(ref _cwMinusTn, value);
                CwMinusTnColor = (!string.IsNullOrEmpty(value) && value.StartsWith("-")) ? "#EF4444" : "#10B981";
            }
        }

        private string _cwMinusTnColor = "#10B981";
        public string CwMinusTnColor
        {
            get => _cwMinusTnColor;
            set => SetProperty(ref _cwMinusTnColor, value);
        }

        private string _tnMinusSmallBrand = "--";
        public string TnMinusSmallBrand
        {
            get => _tnMinusSmallBrand;
            set
            {
                SetProperty(ref _tnMinusSmallBrand, value);
                TnMinusSmallBrandColor = (!string.IsNullOrEmpty(value) && value.StartsWith("-")) ? "#EF4444" : "#10B981";
            }
        }

        private string _tnMinusSmallBrandColor = "#10B981";
        public string TnMinusSmallBrandColor
        {
            get => _tnMinusSmallBrandColor;
            set => SetProperty(ref _tnMinusSmallBrandColor, value);
        }

        private string _cwMinusSmallBrand = "--";
        public string CwMinusSmallBrand
        {
            get => _cwMinusSmallBrand;
            set
            {
                SetProperty(ref _cwMinusSmallBrand, value);
                CwMinusSmallBrandColor = (!string.IsNullOrEmpty(value) && value.StartsWith("-")) ? "#EF4444" : "#10B981";
            }
        }

        private string _cwMinusSmallBrandColor = "#10B981";
        public string CwMinusSmallBrandColor
        {
            get => _cwMinusSmallBrandColor;
            set => SetProperty(ref _cwMinusSmallBrandColor, value);
        }
    }

    public partial class indexViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _currentAgentName = "";
        
        public indexViewModel()
        {
            // 构造函数中不加载数据，等待选择器初始化后回调
        }

        /// <summary>
        /// 当代理商选择变化时调用
        /// </summary>
        public async Task OnAgentChangedAsync(AgentItem? agent, AgentUser? manager, DeptInfo? region, DeptInfo? channel, DeptInfo? warZone)
        {            
            long agentId = agent?.AgentId ?? 0;
            
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
            if (agentId > 0)
            {
                // 重新加载数据
                await LoadAgentResourcesAsync(CurrentAgentId, CurrentYear);
                await LoadResourceDistributionAsync(CurrentAgentId, CurrentYear);
                await LoadBrandSalesComparisonAsync(CurrentAgentId, CurrentYear);
            }
            else
            {
                // 清空所有数据
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InitializeAgentResourcesData();
                    InitializeResourceDistributionData();
                    InitializeSalesComparisonData();
                });
            }
        }

        /// <summary>
        /// 初始化加载数据（当页面首次加载时调用）
        /// </summary>
        public async Task InitializeDataAsync()
        {
            // 只有当 CurrentAgentId 有效时才加载数据
            if (CurrentAgentId > 0)
            {
                await LoadDataAsync();
            }
        }

        private async Task LoadDataAsync()
        {
            // 只有当 CurrentAgentId 有效时才加载数据
            if (CurrentAgentId > 0)
            {
                await LoadAgentResourcesAsync(CurrentAgentId, CurrentYear);
                await LoadResourceDistributionAsync(CurrentAgentId, CurrentYear);
                await LoadBrandSalesComparisonAsync(CurrentAgentId, CurrentYear);
            }
        }
        [ObservableProperty]
        private long _cwEmployees;
        
        [ObservableProperty]
        private long _cwVehicles;
        
        [ObservableProperty]
        private decimal _cwSpecialFund;
        
        [ObservableProperty]
        private long _tnEmployees;
        
        [ObservableProperty]
        private long _tnVehicles;
        
        [ObservableProperty]
        private decimal _tnSpecialFund;

        [ObservableProperty]
        private decimal _channelCoverage;

        [ObservableProperty]
        private string _statusMessage = "就绪";

        [ObservableProperty]
        private int _currentYear = DateTime.Now.Year;

        public long CurrentAgentId { get; set; }

        [ObservableProperty]
        private long _employeeCwCount;

        [ObservableProperty]
        private long _employeeTnCount;

        [ObservableProperty]
        private long _employeeTotal;

        [ObservableProperty]
        private string _employeeCwRatio = "0%";

        [ObservableProperty]
        private string _employeeTnRatio = "0%";

        [ObservableProperty]
        private string _employeeCwPathData = "";

        [ObservableProperty]
        private string _vehicleCwPathData = "";

        [ObservableProperty]
        private string _fundCwPathData = "";

        public ObservableCollection<SalesComparisonItem> SalesComparisonItems { get; } = new ObservableCollection<SalesComparisonItem>();

        [ObservableProperty]
        private decimal _totalMarketTotal;

        [ObservableProperty]
        private decimal _totalCwSales;

        [ObservableProperty]
        private decimal _totalTnSales;

        [ObservableProperty]
        private decimal _totalSmallBrandSales;

        [ObservableProperty]
        private long _totalMarketCapacity;

        // 用于 TextBox 绑定的字符串版本
        public string CwEmployeesText => _cwEmployees.ToString();
        public string CwVehiclesText => _cwVehicles.ToString();
        public string CwSpecialFundText => _cwSpecialFund.ToString();
        public string TnEmployeesText => _tnEmployees.ToString();
        public string TnVehiclesText => _tnVehicles.ToString();
        public string TnSpecialFundText => _tnSpecialFund.ToString();
        public string ChannelCoverageText => _channelCoverage.ToString("F2");
        public string TotalMarketCapacityText => _totalMarketCapacity.ToString();

        // 当数值属性变化时，通知字符串属性也变化
        partial void OnCwEmployeesChanged(long value)
        {
            OnPropertyChanged(nameof(CwEmployeesText));
        }
        
        partial void OnCwVehiclesChanged(long value)
        {
            OnPropertyChanged(nameof(CwVehiclesText));
        }
        
        partial void OnCwSpecialFundChanged(decimal value)
        {
            OnPropertyChanged(nameof(CwSpecialFundText));
        }
        
        partial void OnTnEmployeesChanged(long value)
        {
            OnPropertyChanged(nameof(TnEmployeesText));
        }
        
        partial void OnTnVehiclesChanged(long value)
        {
            OnPropertyChanged(nameof(TnVehiclesText));
        }
        
        partial void OnTnSpecialFundChanged(decimal value)
        {
            OnPropertyChanged(nameof(TnSpecialFundText));
        }

        partial void OnChannelCoverageChanged(decimal value)
        {
            OnPropertyChanged(nameof(ChannelCoverageText));
        }

        partial void OnTotalMarketCapacityChanged(long value)
        {
            OnPropertyChanged(nameof(TotalMarketCapacityText));
        }

        public decimal TotalMarketSharePercent
        {
            get
            {
                if (TotalMarketTotal == 0)
                    return 0;
                //return ((TotalCwSales + TotalTnSales) / TotalMarketTotal) * 100;
                return ((TotalCwSales) / TotalMarketTotal) * 100;
            }
        }

        public string TotalMarketShareText => $"{TotalMarketSharePercent.ToString("F1")}%";

        public string MarketSharePathData
        {
            get
            {
                if (TotalMarketTotal == 0)
                    return "";
                
                double percentage = (double)((TotalCwSales + TotalTnSales) / TotalMarketTotal);
                if (percentage >= 1.0)
                    return "M80,80 L80,0 A80,80 0 0,1 80,160 A80,80 0 0,1 80,0 Z";
                
                double angle = percentage * 360;
                double radian = angle * Math.PI / 180;
                double x = 80 + 80 * Math.Sin(radian);
                double y = 80 - 80 * Math.Cos(radian);
                int largeArc = angle > 180 ? 1 : 0;
                
                return $"M80,80 L80,0 A80,80 0 {largeArc},1 {x},{y} L80,80 Z";
            }
        }

        [ObservableProperty]
        private long _vehicleCwCount;

        [ObservableProperty]
        private long _vehicleTnCount;

        [ObservableProperty]
        private long _vehicleTotal;

        [ObservableProperty]
        private string _vehicleCwRatio = "0%";

        [ObservableProperty]
        private string _vehicleTnRatio = "0%";

        [ObservableProperty]
        private decimal _fundCwAmount;

        [ObservableProperty]
        private decimal _fundTnAmount;

        [ObservableProperty]
        private decimal _fundTotal;

        [ObservableProperty]
        private string _fundCwRatio = "0%";

        [ObservableProperty]
        private string _fundTnRatio = "0%";

        public async Task LoadResourceDistributionAsync(long agentId, int year)
        {
            // 检查 agentId 是否有效
            if (agentId <= 0)
            {
                Logger.Warning($"agentId 无效，不加载数据: {agentId}");
                return;
            }
            
            try
            {
                Logger.Info($"开始加载资源分布数据 - agentId: {agentId}, year: {year}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/agentresources/getResourceDistributionComparison?agentId={agentId}&yearMonth={year}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    
                    if (data != null)
                    {
                        // 在 UI 线程上更新属性
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            var employeeData = data["employeeDistribution"] as Newtonsoft.Json.Linq.JObject;
                            if (employeeData != null)
                            {
                                EmployeeCwCount = employeeData["cwCount"]?.ToObject<long>() ?? 0;
                                EmployeeTnCount = employeeData["tnCount"]?.ToObject<long>() ?? 0;
                                EmployeeTotal = employeeData["total"]?.ToObject<long>() ?? 0;
                                EmployeeCwRatio = employeeData["cwRatio"]?.ToString() ?? "0%";
                                EmployeeTnRatio = employeeData["tnRatio"]?.ToString() ?? "0%";
                                EmployeeCwPathData = CalculatePiePathData(EmployeeCwCount, EmployeeTotal);
                            }

                            var vehicleData = data["vehicleDistribution"] as Newtonsoft.Json.Linq.JObject;
                            if (vehicleData != null)
                            {
                                VehicleCwCount = vehicleData["cwCount"]?.ToObject<long>() ?? 0;
                                VehicleTnCount = vehicleData["tnCount"]?.ToObject<long>() ?? 0;
                                VehicleTotal = vehicleData["total"]?.ToObject<long>() ?? 0;
                                VehicleCwRatio = vehicleData["cwRatio"]?.ToString() ?? "0%";
                                VehicleTnRatio = vehicleData["tnRatio"]?.ToString() ?? "0%";
                                VehicleCwPathData = CalculatePiePathData(VehicleCwCount, VehicleTotal);
                            }

                            var fundData = data["fundDistribution"] as Newtonsoft.Json.Linq.JObject;
                            if (fundData != null)
                            {
                                FundCwAmount = fundData["cwAmount"]?.ToObject<decimal>() ?? 0;
                                FundTnAmount = fundData["tnAmount"]?.ToObject<decimal>() ?? 0;
                                FundTotal = fundData["total"]?.ToObject<decimal>() ?? 0;
                                FundCwRatio = fundData["cwRatio"]?.ToString() ?? "0%";
                                FundTnRatio = fundData["tnRatio"]?.ToString() ?? "0%";
                                FundCwPathData = CalculatePiePathData((double)FundCwAmount, (double)FundTotal);
                            }
                            
                            Logger.Success("资源分布数据加载成功");
                        });
                    }
                }
                else
                {
                    Logger.Warning($"加载资源分布数据失败: {response.Message}");
                    // 清空数据
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InitializeResourceDistributionData();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载资源分布数据异常: {ex.Message}", ex);
                // 清空数据
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InitializeResourceDistributionData();
                });
            }
        }

        public void InitializeResourceDistributionData()
        {
            // 清空员工分布数据
            EmployeeCwCount = 0;
            EmployeeTnCount = 0;
            EmployeeTotal = 0;
            EmployeeCwRatio = "0%";
            EmployeeTnRatio = "0%";
            EmployeeCwPathData = "";
            
            // 清空车辆分布数据
            VehicleCwCount = 0;
            VehicleTnCount = 0;
            VehicleTotal = 0;
            VehicleCwRatio = "0%";
            VehicleTnRatio = "0%";
            VehicleCwPathData = "";
            
            // 清空资金分布数据
            FundCwAmount = 0;
            FundTnAmount = 0;
            FundTotal = 0;
            FundCwRatio = "0%";
            FundTnRatio = "0%";
            FundCwPathData = "";
        }

        private string CalculatePiePathData(double value, double total)
        {
            if (total == 0)
                return "";
            
            double percentage = value / total;
            if (percentage >= 1.0)
                return "M80,80 L80,0 A80,80 0 0,1 80,160 A80,80 0 0,1 80,0 Z";
            
            double angle = percentage * 360;
            double radian = angle * Math.PI / 180;
            double x = 80 + 80 * Math.Sin(radian);
            double y = 80 - 80 * Math.Cos(radian);
            int largeArc = angle > 180 ? 1 : 0;
            
            return $"M80,80 L80,0 A80,80 0 {largeArc},1 {x},{y} L80,80 Z";
        }

        public void InitializeSalesComparisonData()
        {
            SalesComparisonItems.Clear();              
            
            CalculateTotals();
        }

        public void InitializeAgentResourcesData()
        {
            // 清空代理商资源数据
            CwEmployees = 0;
            CwVehicles = 0;
            CwSpecialFund = 0;
            TnEmployees = 0;
            TnVehicles = 0;
            TnSpecialFund = 0;
            ChannelCoverage = 0;
        }
        
        public async Task LoadBrandSalesComparisonAsync(long agentId, int year)
        {
            // 检查 agentId 是否有效
            if (agentId <= 0)
            {
                Logger.Warning($"agentId 无效，不加载数据: {agentId}");
                return;
            }
            
            try
            {
                Logger.Info($"开始加载品牌销量对比数据 - agentId: {agentId}, year: {year}");
                
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/marketfact/getBrandSalesComparison?agentId={agentId}&yearMonth={year}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JArray;
                    if (data != null)
                    {
                        // 在 UI 线程上更新集合
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            SalesComparisonItems.Clear();
                            
                            foreach (var item in data)
                            {
                                var salesItem = new SalesComparisonItem
                                {
                                    SeriesName = item["seriesName"]?.ToString() ?? string.Empty,
                                    MarketTotal = item["marketTotal"]?.ToObject<decimal>() ?? 0,
                                    CwSales = item["cwSales"]?.ToObject<decimal>() ?? 0,
                                    CwRatio = item["cwRatio"]?.ToString() ?? "--",
                                    TnSales = item["tnSales"]?.ToObject<decimal>() ?? 0,
                                    TnRatio = item["tnRatio"]?.ToString() ?? "--",
                                    SmallBrandSales = item["smallBrandSales"]?.ToObject<decimal>() ?? 0,
                                    SmallBrandRatio = item["smallBrandRatio"]?.ToString() ?? "--",
                                    CwMinusTn = item["cwMinusTn"]?.ToString() ?? "--",
                                    TnMinusSmallBrand = item["tnMinusSmallBrand"]?.ToString() ?? "--",
                                    CwMinusSmallBrand = item["cwMinusSmallBrand"]?.ToString() ?? "--"
                                };
                                SalesComparisonItems.Add(salesItem);
                            }
                            
                            CalculateTotals();
                            Logger.Success("品牌销量对比数据加载成功");
                        });
                    }
                }
                else
                {
                    Logger.Warning($"品牌销量对比数据加载失败: {response.Message}");
                    // 清空数据
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InitializeSalesComparisonData();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载品牌销量对比数据异常: {ex.Message}", ex);
                // 清空数据
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InitializeSalesComparisonData();
                });
            }
        }
        
        private void CalculateTotals()
        {
            TotalMarketTotal = SalesComparisonItems.Sum(item => item.MarketTotal);
            TotalCwSales = SalesComparisonItems.Sum(item => item.CwSales);
            TotalTnSales = SalesComparisonItems.Sum(item => item.TnSales);
            TotalSmallBrandSales = SalesComparisonItems.Sum(item => item.SmallBrandSales);

            TotalMarketCapacity = (long)TotalMarketTotal;

            OnPropertyChanged(nameof(TotalMarketSharePercent));
            OnPropertyChanged(nameof(TotalMarketShareText));
            OnPropertyChanged(nameof(MarketSharePathData));
        }

        [RelayCommand]
        private async Task ClearAgentResourcesAsync()
        {
            // 检查 CurrentAgentId 是否有效
            if (CurrentAgentId <= 0)
            {
                Logger.Warning($"CurrentAgentId 无效，不清空数据: {CurrentAgentId}");
                StatusMessage = "请先选择代理商";
                return;
            }
            
            try
            {
                StatusMessage = "正在清空代理商年度数据...";
                Logger.Info($"开始清空代理商年度数据 - agentId: {CurrentAgentId}, year: {CurrentYear}");
                
                var requestData = new
                {
                    agentId = CurrentAgentId,
                    yearMonth = CurrentYear
                };
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/agentresources/removeAgentResourcesInfo", 
                    requestData);
                
                if (response.Code == 200)
                {
                    // 清空所有数据
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InitializeAgentResourcesData();
                        InitializeResourceDistributionData();
                        InitializeSalesComparisonData();
                    });
                    
                    Logger.Success("代理商年度数据清空成功");
                    StatusMessage = "数据清空成功";
                }
                else
                {
                    Logger.Warning($"清空代理商年度数据失败: {response.Message}");
                    StatusMessage = $"清空失败: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"清空数据失败: {ex.Message}";
                Logger.Error($"清空代理商年度数据异常: {ex.Message}", ex);
            }
        }

        public async Task LoadAgentResourcesAsync(long agentId, int year)
        {
            // 检查 agentId 是否有效
            if (agentId <= 0)
            {
                Logger.Warning($"agentId 无效，不加载数据: {agentId}");
                return;
            }
            
            try
            {
                StatusMessage = "正在加载代理商资源数据...";
                Logger.Info($"开始加载代理商资源数据 - agentId: {agentId}, year: {year}");
                
                // 使用 NewApiClient 调用后端API
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/agentresources/getAgentResourcesInfo?agentId={agentId}&yearMonth={year}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    
                    if (data != null)
                    {
                       
                        // 使用 Dispatcher 确保在 UI 线程上更新属性
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            // 解析响应数据
                            var cwEmployees = data["cwEmployees"]?.ToObject<long>() ?? 0;
                            var cwVehicles = data["cwVehicles"]?.ToObject<long>() ?? 0;
                            var cwSpecialFund = data["cwSpecialFund"]?.ToObject<decimal>() ?? 0;
                            var tnEmployees = data["tnEmployees"]?.ToObject<long>() ?? 0;
                            var tnVehicles = data["tnVehicles"]?.ToObject<long>() ?? 0;
                            var tnSpecialFund = data["tnSpecialFund"]?.ToObject<decimal>() ?? 0;
                            var channelCoverage = data["channelCoverage"]?.ToObject<decimal>() ?? 0;
                                                       
                            CwEmployees = cwEmployees;                            
                            CwVehicles = cwVehicles;                          
                            CwSpecialFund = cwSpecialFund;                          
                            TnEmployees = tnEmployees;                            
                            TnVehicles = tnVehicles;                           
                            TnSpecialFund = tnSpecialFund;                           
                            ChannelCoverage = channelCoverage;                           
                            StatusMessage = "数据加载成功";
                        });
                    }
                    else
                    {
                        Logger.Warning("响应数据为空");
                        StatusMessage = "响应数据为空";
                        // 清空数据
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            InitializeAgentResourcesData();
                        });
                    }
                }
                else
                {
                    Logger.Warning($"加载代理商资源失败: {response.Message}");
                    StatusMessage = $"加载失败: {response.Message}";
                    // 清空数据
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        InitializeAgentResourcesData();
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据失败: {ex.Message}";
                Logger.Error($"加载代理商资源异常: {ex.Message}", ex);
                // 清空数据
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    InitializeAgentResourcesData();
                });
            }
        }

        public async Task SaveAgentResourceAsync(string fieldName, object value)
        {
            // 检查 CurrentAgentId 是否有效
            if (CurrentAgentId <= 0)
            {
                Logger.Warning($"CurrentAgentId 无效，不保存数据: {CurrentAgentId}");
                StatusMessage = "请先选择代理商";
                return;
            }
            
            try
            {
                StatusMessage = $"正在保存 {fieldName}...";
                Logger.Info($"开始保存代理商资源 - field: {fieldName}, value: {value}, agentId: {CurrentAgentId}, yearMonth: {CurrentYear}");
                
                var requestData = new Dictionary<string, object>
                {
                    ["agentId"] = CurrentAgentId,
                    ["yearMonth"] = CurrentYear,
                    [fieldName] = value
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/agentresources/saveRateAgentResources", 
                    requestData);
                
                if (response.Code == 200)
                {
                    Logger.Success($"{fieldName} 保存成功");
                    StatusMessage = "保存成功";
                    await LoadResourceDistributionAsync(CurrentAgentId, CurrentYear);
                }
                else
                {
                    Logger.Warning($"{fieldName} 保存失败: {response.Message}");
                    StatusMessage = $"保存失败: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                Logger.Error($"{fieldName} 保存异常: {ex.Message}", ex);
            }
        }

        public async Task SaveBrandSalesComparisonAsync(string seriesName, decimal cwSales, decimal tnSales, decimal smallBrandSales)
        {
            // 检查 CurrentAgentId 是否有效
            if (CurrentAgentId <= 0)
            {
                Logger.Warning($"CurrentAgentId 无效，不保存数据: {CurrentAgentId}");
                StatusMessage = "请先选择代理商";
                return;
            }
            
            try
            {
                StatusMessage = "正在保存品牌销量数据...";
                Logger.Info($"开始保存品牌销量对比数据 - seriesName: {seriesName}, cwSales: {cwSales}, tnSales: {tnSales}, smallBrandSales: {smallBrandSales}, agentId: {CurrentAgentId}, yearMonth: {CurrentYear}");
                
                var requestData = new Dictionary<string, object>
                {
                    ["agentId"] = CurrentAgentId,
                    ["yearMonth"] = CurrentYear,
                    ["seriesName"] = seriesName,
                    ["cwSales"] = cwSales,
                    ["tnSales"] = tnSales,
                    ["smallBrandSales"] = smallBrandSales
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/marketfact/saveBrandSalesComparison", 
                    requestData);
                
                if (response.Code == 200)
                {
                    Logger.Success("品牌销量数据保存成功");
                    StatusMessage = "保存成功";
                    await LoadBrandSalesComparisonAsync(CurrentAgentId, CurrentYear);
                }
                else
                {
                    Logger.Warning($"品牌销量数据保存失败: {response.Message}");
                    StatusMessage = $"保存失败: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败: {ex.Message}";
                Logger.Error($"品牌销量数据保存异常: {ex.Message}", ex);
            }
        }

        [RelayCommand]
        public async Task DownloadTemplateAsync()
        {
            try
            {
                StatusMessage = "正在下载模板...";
                Logger.Info("开始下载市场事实导入模板");

                byte[]? fileBytes = await NewApiClient.PostAsyncBytes(
                    "/rate/marketfact/importTemplate",null
                );

                if (fileBytes != null && fileBytes.Length > 0)
                {
                    string fileName = $"marketfact_import_template.xlsx";

                    // 显示保存文件对话框，让用户选择保存位置
                    var filePath = await FileService.ShowSaveFileDialogAsync(
                        "保存模板",
                        fileName,
                        "Excel", "All"
                    );

                    if (string.IsNullOrEmpty(filePath))
                    {
                        // 用户取消了保存
                        Logger.Info("用户取消了保存操作");
                        StatusMessage = "已取消";
                        return;
                    }

                    // 写入文件
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
