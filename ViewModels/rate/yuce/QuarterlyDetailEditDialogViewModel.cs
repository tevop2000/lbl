using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AgentManagement.Avalonia.Models;
using AgentManagement.Avalonia.Services;
using AgentManagement.Avalonia.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AgentManagement.Avalonia.ViewModels.rate.yuce
{
    public partial class QuarterlyDetailEditDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private RateMonthlyDetailQuarterly _data;

        [ObservableProperty]
        private long _targetSales;

        [ObservableProperty]
        private ObservableCollection<FixedCostItem> _fixedCostItems = new();

        [ObservableProperty]
        private ObservableCollection<FixedCostItem> _incomeItems = new();

        [ObservableProperty]
        private ObservableCollection<ProductStructure> _productStructures = new();

        [ObservableProperty]
        private double _growthRate;

        [ObservableProperty]
        private double _salesAchievementRate;

        [ObservableProperty]
        private string _achievementRateText = "销售量达成率: 100.00%";

        [ObservableProperty]
        private string _achievementRateBackground = "#D1FAE5";

        [ObservableProperty]
        private string _achievementRateBorder = "#BBF7D0";

        [ObservableProperty]
        private string _achievementRateForeground = "#059669";

        [ObservableProperty]
        private string _proportionSumText = "产品占比总和: 0.00";

        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4";

        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0";

        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A";

        [ObservableProperty]
        private string _optimizationSumText = "结构优化总和: 0.00";

        [ObservableProperty]
        private string _optimizationSumBackground = "#D1FAE5";

        [ObservableProperty]
        private string _optimizationSumBorder = "#BBF7D0";

        [ObservableProperty]
        private string _optimizationSumForeground = "#059669";

        public IRelayCommand ApplyCommand { get; }

        public Action? OnSave;
        public Action? OnCancel;
        public Action? OnClose;

        public QuarterlyDetailEditDialogViewModel(RateMonthlyDetailQuarterly data)
        {
            Data = data;
            TargetSales = data.TargetSales ?? 0;

            ApplyCommand = new RelayCommand(Apply);
        }

        public async Task LoadDataAsync()
        {
            if (Data.AgentId == null || Data.AgentId <= 0)
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] AgentId无效，不加载数据");
                InitializeDefaultData();
                return;
            }

            var tasks = new List<Task>
            {
                LoadExpenseDataAsync(Data.AgentId.Value, Data.YearMonth ?? string.Empty),
                LoadProductStructureDataAsync(Data.AgentId.Value, Data.YearMonth ?? string.Empty),
                LoadSalesTargetDataAsync(Data.AgentId.Value, Data.YearMonth ?? string.Empty)
            };

            await Task.WhenAll(tasks);

            UpdateAchievementRate();
            UpdateProportionSum();
            UpdateOptimizationSum();
        }

        private async Task LoadExpenseDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] 开始加载费用数据");
                var quarterly = Data.Quarterly ?? string.Empty;
                var response = await NewApiClient.GetAsync<List<ExpenseDto>>(
                    $"/rate/expenseQuarterly/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}");

                FixedCostItems.Clear();
                IncomeItems.Clear();

                if (response?.Code == 200 && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        var expenseItem = new FixedCostItem
                        {
                            ExpenseType = item.ExpenseType,
                            Amount = item.Amount
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

                    if (FixedCostItems.Count == 0 && IncomeItems.Count == 0)
                    {
                        InitializeDefaultExpenseData();
                    }
                }
                else
                {
                    InitializeDefaultExpenseData();
                }

                Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载费用数据成功，固定成本{FixedCostItems.Count}条，收益项目{IncomeItems.Count}条");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载费用数据失败: {ex.Message}");
                InitializeDefaultExpenseData();
            }
        }

        private async Task LoadProductStructureDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] 开始加载产品结构数据");
                var quarterly = Data.Quarterly ?? string.Empty;
                var response = await NewApiClient.GetAsync<List<ProductStructureDto>>(
                    $"/rate/structureQuarterly/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}");

                ProductStructures.Clear();

                if (response?.Code == 200 && response.Data != null)
                {
                    foreach (var item in response.Data)
                    {
                        var structure = new ProductStructure
                        {
                            ModelId = item.ModelId,
                            Model = item.ItemModel ?? string.Empty,
                            Proportion = (double)item.StructureRatio,
                            SellingPrice = item.RemiumPrice,
                            PurchasePrice = item.RemiumCost,
                            Commission = item.Commission,
                            PriceAdjustment = (double)item.PriceAdjustment,
                            PremiumDiscount = (double)item.PremiumDiscount
                        };
                        structure.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(ProductStructure.Proportion))
                            {
                                UpdateProportionSum();
                            }
                            if (args.PropertyName == nameof(ProductStructure.PriceAdjustment))
                            {
                                UpdateOptimizationSum();
                            }
                        };
                        ProductStructures.Add(structure);
                    }
                    Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载产品结构数据成功，共 {ProductStructures.Count} 条");
                }
                else
                {
                    InitializeDefaultProductStructures();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载产品结构数据失败: {ex.Message}");
                InitializeDefaultProductStructures();
            }
        }

        private async Task LoadSalesTargetDataAsync(long agentId, string yearMonth)
        {
            try
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] 开始加载销售目标数据");

                var quarterly = Data.Quarterly ?? string.Empty;
                var response = await NewApiClient.GetAsync<dynamic>(
                    $"/rate/saletargetQuarterly/getByAgentIdAndYearMonth?agentId={agentId}&yearMonth={yearMonth}&quarterly={quarterly}");

                if (response?.Code == 200 && response.Data != null)
                {
                    var data = response.Data as Newtonsoft.Json.Linq.JObject;
                    if (data != null)
                    {
                        TargetSales = data["targetSales"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? (long)data["targetSales"].ToObject<decimal>() : 0;
                        GrowthRate = data["growthRate"]?.Type != Newtonsoft.Json.Linq.JTokenType.Null ? (double)data["growthRate"].ToObject<decimal>() : 0;
                        SalesAchievementRate = GrowthRate + 100;
                    }
                }
                else
                {
                    Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载销售目标数据失败: {response?.Message}");
                    ResetSalesTargetData();
                }

                Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载销售目标数据成功，TargetSales={TargetSales}, GrowthRate={GrowthRate}, SalesAchievementRate={SalesAchievementRate}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 加载销售目标数据失败: {ex.Message}");
                ResetSalesTargetData();
            }
        }

        private void ResetSalesTargetData()
        {
            TargetSales = 0;
            GrowthRate = 0;
            SalesAchievementRate = 100;
        }

        private void InitializeDefaultData()
        {
            InitializeDefaultExpenseData();
            InitializeDefaultProductStructures();
        }

        private void InitializeDefaultExpenseData()
        {
            FixedCostItems.Add(new FixedCostItem { ExpenseType = "投入电池资金成本（贷款的资金利息）", Amount = 0 });
            FixedCostItems.Add(new FixedCostItem { ExpenseType = "门面或仓库租金", Amount = 0 });
            FixedCostItems.Add(new FixedCostItem { ExpenseType = "固定工资（内勤、业务、售后等）", Amount = 0 });
            FixedCostItems.Add(new FixedCostItem { ExpenseType = "车辆油费、保险", Amount = 0 });
            FixedCostItems.Add(new FixedCostItem { ExpenseType = "水电、招待费", Amount = 0 });

            IncomeItems.Add(new FixedCostItem { ExpenseType = "承兑收益", Amount = 0 });
            IncomeItems.Add(new FixedCostItem { ExpenseType = "售后收益", Amount = 0 });
        }

        private void InitializeDefaultProductStructures()
        {
            var structure1 = new ProductStructure { Model = "12系列", Proportion = 0.45, SellingPrice = 760, PurchasePrice = 620, Commission = 0 };
            var structure2 = new ProductStructure { Model = "其他", Proportion = 0.55, SellingPrice = 0, PurchasePrice = 0, Commission = 0 };
            AddProductStructurePropertyChanged(structure1);
            AddProductStructurePropertyChanged(structure2);
            ProductStructures.Add(structure1);
            ProductStructures.Add(structure2);
        }

        private void AddProductStructurePropertyChanged(ProductStructure structure)
        {
            structure.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(ProductStructure.Proportion))
                {
                    UpdateProportionSum();
                }
                if (args.PropertyName == nameof(ProductStructure.PriceAdjustment))
                {
                    UpdateOptimizationSum();
                }
            };
        }

        private void UpdateAchievementRate()
        {
            double achievementRate = SalesAchievementRate;
            AchievementRateText = $"销售量达成率: {achievementRate:F2}%";

            if (Math.Abs(achievementRate - 100) < 0.005)
            {
                AchievementRateBackground = "#D1FAE5";
                AchievementRateBorder = "#BBF7D0";
                AchievementRateForeground = "#059669";
                AchievementRateText = $"✓ {AchievementRateText}";
            }
            else if (achievementRate > 100)
            {
                AchievementRateBackground = "#D1FAE5";
                AchievementRateBorder = "#BBF7D0";
                AchievementRateForeground = "#059669";
                AchievementRateText = $"✓ {AchievementRateText}";
            }
            else
            {
                AchievementRateBackground = "#FEF3C7";
                AchievementRateBorder = "#FDE68A";
                AchievementRateForeground = "#D97706";
                AchievementRateText = $"⚠ {AchievementRateText}";
            }
        }

        private void UpdateProportionSum()
        {
            var sum = ProductStructures.Sum(p => p.Proportion);
            ProportionSumText = $"产品占比总和: {sum:F2}";

            if (Math.Abs(sum - 1.0) < 0.005)
            {
                ProportionSumBackground = "#D1FAE5";
                ProportionSumBorder = "#BBF7D0";
                ProportionSumForeground = "#059669";
                ProportionSumText = $"✓ {ProportionSumText} 已达标";
            }
            else if (sum > 1.0)
            {
                ProportionSumBackground = "#FEE2E2";
                ProportionSumBorder = "#FECACA";
                ProportionSumForeground = "#DC2626";
                ProportionSumText = $"✗ {ProportionSumText} 超出{(sum - 1.0):F4}";
            }
            else
            {
                ProportionSumBackground = "#FEF3C7";
                ProportionSumBorder = "#FDE68A";
                ProportionSumForeground = "#D97706";
                ProportionSumText = $"⚠ {ProportionSumText} 还差{(1.0 - sum):F4}";
            }
        }

        private void UpdateOptimizationSum()
        {
            var sum = ProductStructures.Sum(p => p.PriceAdjustment);
            OptimizationSumText = $"结构优化总和: {sum:F2}";

            if (Math.Abs(sum - 0.0) < 0.005)
            {
                OptimizationSumBackground = "#D1FAE5";
                OptimizationSumBorder = "#BBF7D0";
                OptimizationSumForeground = "#059669";
                OptimizationSumText = $"✓ {OptimizationSumText} 已达标";
            }
            else if (sum > 0.0)
            {
                OptimizationSumBackground = "#FEE2E2";
                OptimizationSumBorder = "#FECACA";
                OptimizationSumForeground = "#DC2626";
                OptimizationSumText = $"✗ {OptimizationSumText} 超出{sum:F4}";
            }
            else
            {
                OptimizationSumBackground = "#FEF3C7";
                OptimizationSumBorder = "#FDE68A";
                OptimizationSumForeground = "#D97706";
                OptimizationSumText = $"⚠ {OptimizationSumText} 还差{Math.Abs(sum):F4}";
            }
        }

        private async void Apply()
        {
            await SaveAllDataAsync();
            
            if (Data.AgentId.HasValue && Data.AgentId.Value > 0)
            {
                await CalculateQuarterlyCvpAsync(Data.AgentId.Value, Data.YearMonth ?? string.Empty);
            }
            
            OnSave?.Invoke();
            OnClose?.Invoke();
        }

        private async Task CalculateQuarterlyCvpAsync(long agentId, string yearMonth)
        {
            try
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 开始调用 calculateQuarterlyCvp 接口: agentId={agentId}, yearMonth={yearMonth}");
                
                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth
                };
                
                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/detailQuarterly/calculateQuarterlyCvp",
                    requestData);
                
                Debug.WriteLine($"[QuarterlyDetailEditDialog] calculateQuarterlyCvp 响应码: {response?.Code}, 响应消息: {response?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 调用 calculateQuarterlyCvp 失败: {ex.Message}");
            }
        }

        private async Task SaveAllDataAsync()
        {
            if (Data.AgentId == null || Data.AgentId <= 0)
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] AgentId无效，不保存数据");
                return;
            }

            var agentId = Data.AgentId.Value;
            var yearMonth = Data.YearMonth ?? string.Empty;
            var quarterly = Data.Quarterly ?? string.Empty;

            try
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] 开始保存所有数据");

                await SaveSalesTargetAsync(agentId, yearMonth, quarterly, TargetSales, GrowthRate);

                foreach (var item in FixedCostItems)
                {
                    await SaveExpenseAsync(agentId, yearMonth, item.ExpenseType, item.Amount, 0);
                }

                foreach (var item in IncomeItems)
                {
                    await SaveExpenseAsync(agentId, yearMonth, item.ExpenseType, item.Amount, 1);
                }

                foreach (var item in ProductStructures)
                {
                    await SaveProductStructureAsync(agentId, yearMonth, item);
                }

                Debug.WriteLine("[QuarterlyDetailEditDialog] 所有数据保存成功");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存数据失败: {ex.Message}");
            }
        }

        public async Task SaveSalesTargetAsync()
        {
            if (Data.AgentId == null || Data.AgentId <= 0)
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] AgentId无效，不保存销售目标");
                return;
            }

            var agentId = Data.AgentId.Value;
            var yearMonth = Data.YearMonth ?? string.Empty;
            var quarterly = Data.Quarterly ?? string.Empty;

            await SaveSalesTargetAsync(agentId, yearMonth, quarterly, TargetSales, GrowthRate);
        }

        private async Task SaveSalesTargetAsync(long agentId, string yearMonth, string quarterly, long targetSales, double growthRate)
        {
            try
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] 开始保存销售目标数据");

                decimal growthRateValue = (decimal)growthRate;

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    quarterly = quarterly,
                    targetSales = targetSales,
                    growthRate = growthRateValue
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/saletargetQuarterly/saveRateSalesTarget",
                    requestData);

                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存销售目标响应码: {response?.Code}, 响应消息: {response?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存销售目标失败: {ex.Message}");
            }
        }

        public async Task SaveExpenseAsync(string expenseType, decimal amount, int isIncome)
        {
            if (Data.AgentId == null || Data.AgentId <= 0)
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] AgentId无效，不保存费用数据");
                return;
            }

            var agentId = Data.AgentId.Value;
            var yearMonth = Data.YearMonth ?? string.Empty;

            await SaveExpenseAsync(agentId, yearMonth, expenseType, amount, isIncome);
        }

        private async Task SaveExpenseAsync(long agentId, string yearMonth, string expenseType, decimal amount, int isIncome)
        {
            try
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 开始保存费用数据: {expenseType}");

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    expenseType = expenseType,
                    amount = amount,
                    isIncome = isIncome
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/expenseQuarterly/saveRateMonthlyExpense",
                    requestData);

                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存费用响应码: {response?.Code}, 响应消息: {response?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存费用失败: {ex.Message}");
            }
        }

        public async Task SaveProductStructureAsync(ProductStructure item)
        {
            if (Data.AgentId == null || Data.AgentId <= 0)
            {
                Debug.WriteLine("[QuarterlyDetailEditDialog] AgentId无效，不保存产品结构数据");
                return;
            }

            var agentId = Data.AgentId.Value;
            var yearMonth = Data.YearMonth ?? string.Empty;

            await SaveProductStructureAsync(agentId, yearMonth, item);
        }

        private async Task SaveProductStructureAsync(long agentId, string yearMonth, ProductStructure item)
        {
            try
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 开始保存产品结构数据: {item.Model}");

                var requestData = new
                {
                    agentId = agentId,
                    yearMonth = yearMonth,
                    modelId = item.ModelId,
                    structureRatio = (decimal)item.Proportion,
                    remiumPrice = item.SellingPrice,
                    remiumCost = item.PurchasePrice,
                    commission = item.Commission,
                    priceAdjustment = (decimal)item.PriceAdjustment,
                    premiumDiscount = (decimal)item.PremiumDiscount
                };

                var response = await NewApiClient.PostAsync<dynamic>(
                    "/rate/structureQuarterly/saveRateProductStructure",
                    requestData);

                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存产品结构响应码: {response?.Code}, 响应消息: {response?.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[QuarterlyDetailEditDialog] 保存产品结构失败: {ex.Message}");
            }
        }
    }

    public class FixedCostItem
    {
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class ProductStructure : INotifyPropertyChanged
    {
        private long _modelId;
        private string _model = string.Empty;
        private double _proportion;
        private decimal _sellingPrice;
        private decimal _purchasePrice;
        private decimal _commission;
        private double _priceAdjustment;
        private double _premiumDiscount;

        public long ModelId
        {
            get => _modelId;
            set
            {
                _modelId = value;
                OnPropertyChanged();
            }
        }

        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged();
            }
        }

        public double Proportion
        {
            get => _proportion;
            set
            {
                _proportion = value;
                OnPropertyChanged();
            }
        }

        public decimal SellingPrice
        {
            get => _sellingPrice;
            set
            {
                _sellingPrice = value;
                OnPropertyChanged();
            }
        }

        public decimal PurchasePrice
        {
            get => _purchasePrice;
            set
            {
                _purchasePrice = value;
                OnPropertyChanged();
            }
        }

        public decimal Commission
        {
            get => _commission;
            set
            {
                _commission = value;
                OnPropertyChanged();
            }
        }

        public double PriceAdjustment
        {
            get => _priceAdjustment;
            set
            {
                _priceAdjustment = value;
                OnPropertyChanged();
            }
        }

        public double PremiumDiscount
        {
            get => _premiumDiscount;
            set
            {
                _premiumDiscount = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}