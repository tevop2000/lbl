using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.ViewModels.Controls;

namespace AgentManagement.Avalonia.ViewModels
{
    /// <summary>
    /// 产品项模型
    /// </summary>
    public partial class ProductItem : ObservableObject
    {
        [ObservableProperty]
        private long _modelId; // 产品ID（对应API的modelId），保存时需要

        [ObservableProperty]
        private string _model = string.Empty;

        [ObservableProperty]
        private double _proportion;

        [ObservableProperty]
        private double _groupPrice;

        [ObservableProperty]
        private double _purchasePrice;

        [ObservableProperty]
        private double _commission;
    }

    /// <summary>
    /// 代理商产品编辑ViewModel
    /// </summary>
    public partial class AgentProductEditViewModel : ViewModelBase
    {
        private readonly string _agentName;
        private readonly int _agentId;
        
        public event Action? RequestClose;
        
        /// <summary>
        /// 保存成功事件（用于通知外部刷新代理商列表）
        /// </summary>
        public event Action<int>? SaveSuccess;
        
        /// <summary>
        /// 请求打开产品选择器事件
        /// </summary>
        public event Action? RequestOpenProductSelector;

        [ObservableProperty]
        private string _windowTitle;

        [ObservableProperty]
        private ObservableCollection<ProductItem> _products = new();

        [ObservableProperty]
        private string _proportionSumText = "✓ 产品占比总和: 0.00%";

        [ObservableProperty]
        private string _proportionSumBackground = "#F0FDF4"; // green-50

        [ObservableProperty]
        private string _proportionSumBorder = "#BBF7D0"; // green-200

        [ObservableProperty]
        private string _proportionSumForeground = "#16A34A"; // green-600

        public AgentProductEditViewModel(string agentName, int agentId)
        {
            _agentName = agentName;
            _agentId = agentId;
            WindowTitle = $"编辑产品配置 - {agentName}";
                    
            Console.WriteLine($"[DEBUG] AgentProductEditViewModel 构造函数被调用: agentName={agentName}, agentId={agentId}");
            Console.WriteLine($"[DEBUG] _agentId 字段值: {_agentId}");
                    
            // 从 API 加载产品数据
            _ = LoadProductsAsync();
                    
            Console.WriteLine($"[DEBUG] Products 初始化完成，数量: {Products.Count}");
                    
            // 监听产品列表变化
            Products.CollectionChanged += (s, e) => 
            {
                // 处理新增项
                if (e.NewItems != null)
                {
                    foreach (ProductItem item in e.NewItems)
                    {
                        item.PropertyChanged += (sender, args) => UpdateProportionSum();
                    }
                }
                
                // 处理删除项（不需要取消订阅，因为对象会被回收）
                
                UpdateProportionSum();
            };
            
            // 监听每个产品的属性变化
            Products.CollectionChanged += (s, e) =>
            {
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
            };
        }

        private void LoadSampleData()
        {
            // 示例数据，实际应从API加载
            Products.Add(new ProductItem 
            { 
                Model = "60V20AH", 
                Proportion = 40.00, 
                GroupPrice = 1200.00, 
                PurchasePrice = 900.00, 
                Commission = 50.00 
            });
            Products.Add(new ProductItem 
            { 
                Model = "48V12AH", 
                Proportion = 35.00, 
                GroupPrice = 800.00, 
                PurchasePrice = 600.00, 
                Commission = 30.00 
            });
            Products.Add(new ProductItem 
            { 
                Model = "72V32AH", 
                Proportion = 25.00, 
                GroupPrice = 1800.00, 
                PurchasePrice = 1350.00, 
                Commission = 80.00 
            });
            
            UpdateProportionSum();
        }

        /// <summary>
        /// 从 API 加载产品数据
        /// </summary>
        private async Task LoadProductsAsync()
        {
            try
            {
                Console.WriteLine($"[DEBUG] 开始加载代理商 {_agentId} 的产品配置...");
                
                var response = await NewApiClient.GetAsync<List<Dictionary<string, object>>>(
                    $"/rate/default/getByAgentId?agentId={_agentId}");
                
                if (response.Code == 200 && response.Data != null)
                {
                    Console.WriteLine($"[DEBUG] API 返回 {response.Data.Count} 条产品数据");
                    
                    // 清空现有数据
                    Products.Clear();
                    
                    // 解析数据
                    foreach (var item in response.Data)
                    {
                        var product = new ProductItem
                        {
                            ModelId = item.ContainsKey("modelId") ? Convert.ToInt64(item["modelId"]) : 0,
                            Model = item.ContainsKey("itemModel") ? item["itemModel"]?.ToString() ?? "" : "",
                            Proportion = item.ContainsKey("structureRatio") ? Convert.ToDouble(item["structureRatio"]) : 0,
                            GroupPrice = item.ContainsKey("remiumPrice") ? Convert.ToDouble(item["remiumPrice"]) : 0,
                            PurchasePrice = item.ContainsKey("remiumCost") ? Convert.ToDouble(item["remiumCost"]) : 0,
                            Commission = item.ContainsKey("commission") ? Convert.ToDouble(item["commission"]) : 0
                        };
                        
                        // 订阅属性变化事件
                        product.PropertyChanged += (sender, args) => UpdateProportionSum();
                        
                        Products.Add(product);
                        Console.WriteLine($"[DEBUG]   - 加载产品: {product.Model} (ModelId={product.ModelId}), 占比: {product.Proportion}, 组价: {product.GroupPrice}, 进价: {product.PurchasePrice}, 提成: {product.Commission}");
                    }
                    
                    Console.WriteLine($"[SUCCESS] 成功加载 {Products.Count} 个产品");
                    
                    // 更新占比总和
                    UpdateProportionSum();
                }
                else
                {
                    Console.WriteLine($"[ERROR] 加载产品失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] 加载产品异常: {ex.Message}");
                Logger.Error($"加载产品异常: {ex.Message}", ex);
            }
        }

        private void UpdateProportionSum()
        {
            var sum = Products.Sum(p => p.Proportion);
            ProportionSumText = $"产品占比总和: {sum:F2}%";
            
            if (Math.Abs(sum - 100.0) < 0.01)
            {
                // 正好100%
                ProportionSumBackground = "#F0FDF4"; // green-50
                ProportionSumBorder = "#BBF7D0"; // green-200
                ProportionSumForeground = "#16A34A"; // green-600
                ProportionSumText = $"✓ {ProportionSumText} 已达标";
            }
            else if (sum > 100.0)
            {
                // 超过100%
                ProportionSumBackground = "#FEF2F2"; // red-50
                ProportionSumBorder = "#FECACA"; // red-200
                ProportionSumForeground = "#DC2626"; // red-600
                ProportionSumText = $"✗ {ProportionSumText} 超出{(sum - 100):F2}%";
            }
            else
            {
                // 不足100%
                ProportionSumBackground = "#FFFBEB"; // amber-50
                ProportionSumBorder = "#FDE68A"; // amber-200
                ProportionSumForeground = "#D97706"; // amber-600
                ProportionSumText = $"⚠ {ProportionSumText} 还差{(100 - sum):F2}%";
            }
        }

        [RelayCommand]
        private void AddProduct()
        {
            Logger.Info("打开产品选择器...");
            
            // 触发事件，由 View 层处理打开产品选择器对话框
            RequestOpenProductSelector?.Invoke();
        }
        
        /// <summary>
        /// 从产品选择器添加产品（公共方法，供 View 层调用）
        /// </summary>
        public void AddProductFromSelector(ProductSelectorResult result)
        {
            Console.WriteLine($"[DEBUG] AddProductFromSelector 被调用");
            
            if (result != null)
            {
                Console.WriteLine($"[DEBUG]   Model: {result.Model}");
                Console.WriteLine($"[DEBUG]   Proportion: {result.Proportion}");
                Console.WriteLine($"[DEBUG]   GroupPrice: {result.GroupPrice}");
                Console.WriteLine($"[DEBUG]   PurchasePrice: {result.PurchasePrice}");
                Console.WriteLine($"[DEBUG]   Commission: {result.Commission}");
                
                Logger.Info($"添加产品: {result.Model}");
                
                var newProduct = new ProductItem
                {
                    ModelId = result.ItemId,
                    Model = result.Model,
                    Proportion = result.Proportion,
                    GroupPrice = result.GroupPrice,
                    PurchasePrice = result.PurchasePrice,
                    Commission = result.Commission
                };
                
                // 订阅属性变化事件
                newProduct.PropertyChanged += (sender, args) => UpdateProportionSum();
                
                Console.WriteLine($"[DEBUG]   创建 ProductItem 成功");
                
                // 检查是否在 UI 线程
                if (Dispatcher.UIThread.CheckAccess())
                {
                    Console.WriteLine($"[DEBUG]   当前在 UI 线程，直接添加");
                    Products.Add(newProduct);
                    
                    // 强制刷新 - 通过重新赋值触发 PropertyChanged
                    var temp = Products;
                    Products = new ObservableCollection<ProductItem>(temp);
                    Console.WriteLine($"[DEBUG]   已触发 Products 属性变化通知");
                }
                else
                {
                    Console.WriteLine($"[DEBUG]   不在 UI 线程，使用 InvokeAsync");
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Products.Add(newProduct);
                        
                        // 强制刷新
                        var temp = Products;
                        Products = new ObservableCollection<ProductItem>(temp);
                        
                        Console.WriteLine($"[DEBUG]   异步添加到 Products 列表，当前列表数量: {Products.Count}");
                        UpdateProportionSum();
                    });
                    return; // 提前返回，避免重复添加
                }
                
                Console.WriteLine($"[DEBUG]   添加到 Products 列表，当前列表数量: {Products.Count}");
                
                // 强制刷新占比总和
                UpdateProportionSum();
            }
            else
            {
                Console.WriteLine($"[ERROR] result 为 null");
            }
        }

        [RelayCommand]
        public void DeleteProduct(ProductItem product)
        {
            if (product != null && Products.Contains(product))
            {
                Console.WriteLine($"[DEBUG] 删除产品: ModelId={product.ModelId}, Model={product.Model}");
                Logger.Info($"删除产品: {product.Model}");
                Products.Remove(product);
                Console.WriteLine($"[DEBUG] 删除后 Products 数量: {Products.Count}");
            }
        }

        [RelayCommand]
        private async void Save()
        {
            try
            {
                Logger.Info($"保存产品配置 - 代理商: {_agentName}");
                
                // 验证占比总和
                var sum = Products.Sum(p => p.Proportion);
                if (Math.Abs(sum - 100.0) > 0.01)
                {
                    Logger.Warning($"占比总和不为100%: {sum:F2}%");
                    StatusMessage = $"保存失败：产品占比总和必须为100%，当前为{sum:F2}%";
                    return;
                }
                
                // 构建保存数据
                var saveData = new List<Dictionary<string, object>>();
                Console.WriteLine($"[DEBUG] 当前 Products 数量: {Products.Count}");
                
                foreach (var product in Products)
                {
                    Console.WriteLine($"[DEBUG]   - 准备保存: ModelId={product.ModelId}, Model={product.Model}");
                    
                    saveData.Add(new Dictionary<string, object>
                    {
                        { "modelId", product.ModelId },
                        { "agentId", _agentId },
                        { "structureRatio", product.Proportion },
                        { "remiumPrice", product.GroupPrice },
                        { "remiumCost", product.PurchasePrice },
                        { "premiumDiscount", 1.0 }, // 默认值
                        { "priceAdjustment", 1.0 }, // 默认值
                        { "commission", product.Commission }
                    });
                }
                
                Console.WriteLine($"[DEBUG] 准备保存 {saveData.Count} 个产品配置");
                
                // 调用批量保存接口
                var response = await NewApiClient.PostAsync<object>("/rate/default/batchSave", saveData);
                
                if (response.Code == 200)
                {
                    Logger.Success("产品配置保存成功");
                    StatusMessage = "保存成功！";
                    
                    // 触发保存成功事件，通知外部刷新代理商列表
                    SaveSuccess?.Invoke(_agentId);
                    
                    // 请求关闭窗口
                    RequestClose?.Invoke();
                }
                else
                {
                    Logger.Error($"保存失败: {response.Message}");
                    StatusMessage = $"保存失败: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"保存产品配置失败: {ex.Message}", ex);
                StatusMessage = $"保存失败: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            Logger.Info("取消编辑");
            RequestClose?.Invoke();
        }
    }
}
