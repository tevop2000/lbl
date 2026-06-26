using System;
using System.Collections.ObjectModel;
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
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.ViewModels
{
    public partial class CVPAnalysisViewModel : ViewModelBase
    {
        // KPI 卡片数据
        [ObservableProperty]
        private double _monthTargetSales = 0;

        [ObservableProperty]
        private double _achievementRate = 0;

        [ObservableProperty]
        private double _achievementRatePercent = 0;  // 用于进度条宽度

        [ObservableProperty]
        private double _actualSales = 0;

        [ObservableProperty]
        private double _netProfit = 0;

        // 月度数据列表
        [ObservableProperty]
        private ObservableCollection<MonthlyDataItem> _monthlyDataList = new();

        // 基础配置
        [ObservableProperty]
        private ObservableCollection<int> _monthList = new() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        [ObservableProperty]
        private int _selectedMonth = DateTime.Now.Month;

        [ObservableProperty]
        private double _editableTargetSales = 0;

        [ObservableProperty]
        private double _editableActualSales = 0;

        // 固定成本
        [ObservableProperty]
        private double _batteryCapitalCost = 0;

        [ObservableProperty]
        private double _rent = 0;

        [ObservableProperty]
        private double _salaries = 0;

        [ObservableProperty]
        private double _vehicleCost = 0;

        [ObservableProperty]
        private double _utilities = 0;

        // 收益项目
        [ObservableProperty]
        private double _acceptanceIncome = 0;

        [ObservableProperty]
        private double _afterSalesIncome = 0;

        // 产品列表
        [ObservableProperty]
        private ObservableCollection<ProductItem> _productList = new();
        
        // 产品占比总和显示
        [ObservableProperty]
        private string _proportionSumText = "产品占比总和: 0.00%";
        
        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4"; // green-50
        
        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0"; // green-200
        
        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A"; // green-600

        // 代理商列表
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
            // 当大区经理改变时，清空下级选择并更新业务经理列表
            SelectedManager = null;
            ManagerList.Clear();
            
            if (value != null)
            {
                // 加载该大区下的业务经理
                foreach (var manager in value.Children)
                {
                    ManagerList.Add(manager);
                }
            }
            
            // 如果选择了大区，可以触发查询业务经理的接口
            _ = LoadManagersByDeptAsync();
        }
        
        /// <summary>
        /// 根据选择的部门查询业务经理列表
        /// </summary>
        private async Task LoadManagersByDeptAsync()
        {
            // TODO: 根据 SelectedWarZone, SelectedChannelDept, SelectedRegionManager 调用接口查询业务经理
            // 示例：/system/user/list?deptId={selectedDeptId}
            Logger.Info("TODO: 实现根据部门查询业务经理的接口调用");
        }

        [ObservableProperty]
        private ObservableCollection<DeptInfo> _managerList = new();

        [ObservableProperty]
        private DeptInfo? _selectedManager;

        // 视图模式
        [ObservableProperty]
        private string _viewMode = "monthly"; // monthly 或 yearly

        public CVPAnalysisViewModel()
        {
            Logger.Info("CVPAnalysisViewModel 构造函数被调用");
            LoadMockData();
            // _ = LoadAgentListAsync(); // 暂时不调用代理商列表接口
            // 部门树加载已移至 CascadingDeptSelector 控件内部
            
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
                            if (args.PropertyName == nameof(ProductItem.Proportion))
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

        private void LoadMockData()
        {
            // 加载模拟数据（实际应该从 API 获取）
            MonthTargetSales = 10000;
            ActualSales = 8500;
            AchievementRate = 85.0;
            AchievementRatePercent = 85.0;
            NetProfit = 125000;

            // 月度数据
            MonthlyDataList.Clear();
            for (int i = 1; i <= 12; i++)
            {
                MonthlyDataList.Add(new MonthlyDataItem
                {
                    Month = i,
                    TargetSales = 10000,
                    ActualSales = 8000 + i * 100,
                    AchievementRate = 80 + i,
                    SalesAmount = 500000 + i * 10000,
                    TotalExpense = 400000 + i * 5000,
                    NetProfit = 100000 + i * 5000
                });
            }

            // 基础配置
            EditableTargetSales = 10000;
            EditableActualSales = 8500;
            BatteryCapitalCost = 50000;
            Rent = 30000;
            Salaries = 80000;
            VehicleCost = 15000;
            Utilities = 10000;
            AcceptanceIncome = 5000;
            AfterSalesIncome = 8000;

            // 产品列表
            ProductList.Clear();
            ProductList.Add(new ProductItem { Model = "6-DZF-12", Proportion = 30, GroupPrice = 180, PurchasePrice = 150 });
            ProductList.Add(new ProductItem { Model = "6-DZF-20", Proportion = 40, GroupPrice = 280, PurchasePrice = 230 });
            ProductList.Add(new ProductItem { Model = "6-DZF-32", Proportion = 30, GroupPrice = 420, PurchasePrice = 350 });
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
                if (SelectedAgent == null || SelectedAgent.Id <= 0)
                {
                    Logger.Warning("未选择代理商，无法添加产品");
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
                        Model = result.Model,
                        Proportion = result.Proportion,
                        GroupPrice = result.GroupPrice,
                        PurchasePrice = result.PurchasePrice,
                        Commission = result.Commission
                    };
                    
                    Logger.Info($"创建 ProductItem 成功，准备添加到列表");
                    
                    // 检查是否在 UI 线程
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        Logger.Info("当前在 UI 线程，直接添加");
                        ProductList.Add(newProduct);
                        
                        Logger.Success($"已添加产品: {result.Model}，当前产品数量: {ProductList.Count}");
                    }
                    else
                    {
                        Logger.Info("不在 UI 线程，使用 InvokeAsync");
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ProductList.Add(newProduct);
                            
                            Logger.Success($"异步添加产品: {result.Model}，当前产品数量: {ProductList.Count}");
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
            }
        }

        [RelayCommand]
        private void DeleteProduct(ProductItem product)
        {
            if (product != null && ProductList.Contains(product))
            {
                ProductList.Remove(product);
                Logger.Info($"已删除产品: {product.Model}");
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

        /// <summary>
        /// 更新产品占比总和显示
        /// </summary>
        private void UpdateProportionSum()
        {
            var sum = ProductList.Sum(p => p.Proportion);
            ProportionSumText = $"产品占比总和: {sum:F2}%";
            
            if (Math.Abs(sum - 100.0) < 0.01)
            {
                // 正好100%
                ProportionSumBackground = "#D1FAE5"; // green-100
                ProportionSumBorder = "#BBF7D0"; // green-200
                ProportionSumForeground = "#059669"; // green-600
                ProportionSumText = $"✓ {ProportionSumText} 已达标";
            }
            else if (sum > 100.0)
            {
                // 超过100%
                ProportionSumBackground = "#FEE2E2"; // red-100
                ProportionSumBorder = "#FECACA"; // red-200
                ProportionSumForeground = "#DC2626"; // red-600
                ProportionSumText = $"✗ {ProportionSumText} 超出{(sum - 100):F2}%";
            }
            else
            {
                // 不足100%
                ProportionSumBackground = "#FEF3C7"; // amber-100
                ProportionSumBorder = "#FDE68A"; // amber-200
                ProportionSumForeground = "#D97706"; // amber-600
                ProportionSumText = $"⚠ {ProportionSumText} 还差{(100 - sum):F2}%";
            }
        }
    }

    public class MonthlyDataItem
    {
        public int Month { get; set; }
        public double TargetSales { get; set; }
        public double ActualSales { get; set; }
        public double AchievementRate { get; set; }
        public double SalesAmount { get; set; }
        public double TotalExpense { get; set; }
        public double NetProfit { get; set; }
    }
}
