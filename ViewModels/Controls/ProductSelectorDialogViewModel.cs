using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AgentManagement.Avalonia.Utils;
using AgentManagement.Avalonia.Models;
using Newtonsoft.Json.Linq;

namespace AgentManagement.Avalonia.ViewModels.Controls
{
    /// <summary>
    /// 产品型号项（包含 ID 和名称）
    /// </summary>
    public class ProductModelItem
    {
        public long ItemId { get; set; }
        public string Model { get; set; } = string.Empty;
        
        public override string ToString() => Model;
    }

    /// <summary>
    /// 产品选择器结果
    /// </summary>
    public class ProductSelectorResult
    {
        public long ItemId { get; set; } // 产品ID
        public string Model { get; set; } = string.Empty;
        public double Proportion { get; set; }
        public double GroupPrice { get; set; }
        public double PurchasePrice { get; set; }
        public double Commission { get; set; }
    }

    /// <summary>
    /// 产品目录项（三级结构）
    /// </summary>
    public class ProductCatalogItem
    {
        public string SeriesName { get; set; } = string.Empty;      // 一级：系列名称
        public string ProductSeries { get; set; } = string.Empty;   // 二级：产品系列
        public string Model { get; set; } = string.Empty;           // 三级：产品型号
    }

    /// <summary>
    /// 产品选择器 ViewModel（可复用组件）
    /// </summary>
    public partial class ProductSelectorDialogViewModel : ObservableObject
    {
        /// <summary>
        /// 选择完成事件
        /// </summary>
        public event Action<ProductSelectorResult?>? ProductSelected;

        /// <summary>
        /// 关闭请求事件
        /// </summary>
        public event Action? RequestClose;

        // 产品目录数据
        [ObservableProperty]
        private ObservableCollection<string> _level1List = new(); // 一级：系列名称

        [ObservableProperty]
        private ObservableCollection<string> _level2List = new(); // 二级：产品系列

        [ObservableProperty]
        private ObservableCollection<ProductModelItem> _level3List = new(); // 三级：产品型号

        // 选中项
        [ObservableProperty]
        private string? _selectedLevel1;

        [ObservableProperty]
        private string? _selectedLevel2;

        [ObservableProperty]
        private ProductModelItem? _selectedLevel3;

        // 详细参数
        [ObservableProperty]
        private double _proportion = 0;

        [ObservableProperty]
        private double _groupPrice = 0;

        [ObservableProperty]
        private double _purchasePrice = 0;

        [ObservableProperty]
        private double _commission = 0;

        // 是否显示第二步
        [ObservableProperty]
        private bool _showDetailPanel = false;

        // 是否正在加载
        [ObservableProperty]
        private bool _isLoading = false;

        // 产品目录数据（三级结构）
        // 一级：系列名称 -> 二级：产品系列 -> 三级：产品型号列表（包含 ItemId 和 Model）
        private Dictionary<string, Dictionary<string, List<ProductModelItem>>> _productCatalog = new();

        public ProductSelectorDialogViewModel()
        {
            // 从 API 加载产品目录数据
            _ = LoadProductCatalogAsync();
        }

        /// <summary>
        /// 从 API 加载产品目录数据
        /// </summary>
        private async Task LoadProductCatalogAsync()
        {
            try
            {
                IsLoading = true;
                Logger.Info("开始加载产品目录数据...");
                
                var response = await NewApiClient.GetAsync<List<Dictionary<string, object>>>("/sanjia/productitems/getProductModelsList");
                
                if (response.Code == 200 && response.Data != null)
                {
                    // 解析 API 返回的数据
                    ParseProductCatalog(response.Data);
                    
                    Logger.Success($"成功加载产品目录，共 {_productCatalog.Count} 个系列");
                }
                else
                {
                    Logger.Error($"加载产品目录失败: {response.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"加载产品目录异常: {ex.Message}", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 解析产品目录数据
        /// </summary>
        private void ParseProductCatalog(List<Dictionary<string, object>> data)
        {
            _productCatalog.Clear();
            Level1List.Clear();
            Level2List.Clear();
            Level3List.Clear();

            // API 返回格式:
            // [
            //   {
            //     "seriesName": "普品",
            //     "itemGroups": [
            //       {
            //         "itemName": "主销12系列",
            //         "models": [
            //           { "itemId": 2, "itemModel": "33", "productId": null },
            //           { "itemId": 1, "itemModel": "6-DZF-12.2LX", "productId": 354 }
            //         ]
            //       }
            //     ]
            //   }
            // ]

            if (data == null || data.Count == 0)
            {
                Console.WriteLine("[ERROR] 产品目录数据为空");
                return;
            }

            Console.WriteLine($"[DEBUG] 开始解析 {data.Count} 个系列");

            foreach (var series in data)
            {
                if (!series.TryGetValue("seriesName", out var seriesNameObj))
                {
                    Console.WriteLine("[WARNING] 系列缺少 seriesName 字段");
                    continue;
                }

                var seriesName = seriesNameObj?.ToString() ?? "";
                if (string.IsNullOrEmpty(seriesName))
                    continue;

                Console.WriteLine($"[DEBUG] 解析系列: {seriesName}");

                // 初始化一级
                if (!_productCatalog.ContainsKey(seriesName))
                {
                    _productCatalog[seriesName] = new Dictionary<string, List<ProductModelItem>>();
                }

                // 遍历 itemGroups (二级)
                if (series.TryGetValue("itemGroups", out var itemGroupsObj))
                {
                    Console.WriteLine($"[DEBUG]   itemGroups 类型: {itemGroupsObj?.GetType().Name}");
                    
                    // 尝试多种类型转换
                    System.Collections.IEnumerable? groups = null;
                    
                    if (itemGroupsObj is JArray jArray)
                    {
                        // Newtonsoft.Json 的 JArray
                        groups = jArray;
                    }
                    else if (itemGroupsObj is System.Collections.IEnumerable enumerable && !(itemGroupsObj is string))
                    {
                        groups = enumerable;
                    }
                    else if (itemGroupsObj is List<object> listObj)
                    {
                        groups = listObj;
                    }
                    
                    if (groups != null)
                    {
                        int groupCount = 0;
                        foreach (var groupObj in groups)
                        {
                            Dictionary<string, object>? group = null;
                            
                            // 处理 JObject
                            if (groupObj is JObject jObject)
                            {
                                group = jObject.ToObject<Dictionary<string, object>>();
                            }
                            // 处理 Dictionary
                            else if (groupObj is Dictionary<string, object> dict)
                            {
                                group = dict;
                            }
                            
                            if (group == null)
                            {
                                Console.WriteLine($"[WARNING] groupObj 类型: {groupObj?.GetType().Name}");
                                continue;
                            }

                            if (!group.TryGetValue("itemName", out var itemNameObj))
                                continue;

                            var itemName = itemNameObj?.ToString() ?? "";
                            if (string.IsNullOrEmpty(itemName))
                                continue;

                            Console.WriteLine($"[DEBUG]     - 产品系列: {itemName}");
                            groupCount++;

                            // 初始化二级
                            if (!_productCatalog[seriesName].ContainsKey(itemName))
                            {
                                _productCatalog[seriesName][itemName] = new List<ProductModelItem>();
                            }

                            // 遍历 models (三级)
                            if (group.TryGetValue("models", out var modelsObj))
                            {
                                System.Collections.IEnumerable? models = null;
                                
                                if (modelsObj is JArray modelsJArray)
                                {
                                    models = modelsJArray;
                                }
                                else if (modelsObj is System.Collections.IEnumerable modelEnum && !(modelsObj is string))
                                {
                                    models = modelEnum;
                                }
                                else if (modelsObj is List<object> modelList)
                                {
                                    models = modelList;
                                }
                                
                                if (models != null)
                                {
                                    foreach (var modelObj in models)
                                    {
                                        Dictionary<string, object>? model = null;
                                        
                                        // 处理 JObject
                                        if (modelObj is JObject modelJObject)
                                        {
                                            model = modelJObject.ToObject<Dictionary<string, object>>();
                                        }
                                        // 处理 Dictionary
                                        else if (modelObj is Dictionary<string, object> modelDict)
                                        {
                                            model = modelDict;
                                        }
                                        
                                        if (model == null)
                                            continue;

                                        var itemModel = model.ContainsKey("itemModel") ? model["itemModel"]?.ToString() ?? "" : "";
                                        var itemId = model.ContainsKey("itemId") ? Convert.ToInt64(model["itemId"]) : 0;
                                        
                                        if (!string.IsNullOrEmpty(itemModel))
                                        {
                                            var modelItem = new ProductModelItem
                                            {
                                                ItemId = itemId,
                                                Model = itemModel
                                            };
                                            _productCatalog[seriesName][itemName].Add(modelItem);
                                        }
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine($"[DEBUG]   共解析 {groupCount} 个产品系列");
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] itemGroups 无法转换为 IEnumerable");
                    }
                }
                else
                {
                    Console.WriteLine($"[WARNING] 系列 '{seriesName}' 缺少 itemGroups 字段");
                }
            }

            // 加载一级列表
            foreach (var key in _productCatalog.Keys)
            {
                Level1List.Add(key);
            }

            Console.WriteLine($"[DEBUG] 解析完成: {_productCatalog.Count} 个系列");
            foreach (var kvp in _productCatalog)
            {
                Console.WriteLine($"[DEBUG]   - {kvp.Key}: {kvp.Value.Count} 个产品系列");
            }
        }

        /// <summary>
        /// 选择一级（系列名称）
        /// </summary>
        [RelayCommand]
        private void SelectLevel1(string level1)
        {
            Console.WriteLine($"[DEBUG] SelectLevel1 被调用: {level1}");
            
            SelectedLevel1 = level1;
            SelectedLevel2 = null;
            SelectedLevel3 = null;

            // 加载二级列表
            Level2List.Clear();
            Level3List.Clear();

            if (_productCatalog.ContainsKey(level1))
            {
                Console.WriteLine($"[DEBUG] 找到系列 '{level1}'，包含 {_productCatalog[level1].Count} 个产品系列");
                
                foreach (var key in _productCatalog[level1].Keys)
                {
                    Level2List.Add(key);
                    Console.WriteLine($"[DEBUG]   - 添加产品系列: {key}");
                }
            }
            else
            {
                Console.WriteLine($"[ERROR] 未找到系列 '{level1}'");
            }
        }

        /// <summary>
        /// 选择二级（产品系列）
        /// </summary>
        [RelayCommand]
        private void SelectLevel2(string level2)
        {
            SelectedLevel2 = level2;
            SelectedLevel3 = null;

            // 加载三级列表
            Level3List.Clear();

            if (SelectedLevel1 != null && _productCatalog.ContainsKey(SelectedLevel1))
            {
                var series = _productCatalog[SelectedLevel1];
                if (series.ContainsKey(level2))
                {
                    foreach (var model in series[level2])
                    {
                        Level3List.Add(model);
                    }
                }
            }
        }

        /// <summary>
        /// 选择三级（产品型号）
        /// </summary>
        [RelayCommand]
        private void SelectLevel3(ProductModelItem model)
        {
            SelectedLevel3 = model;
            
            // 显示第二步：设置详细参数
            ShowDetailPanel = true;
        }

        /// <summary>
        /// 确认添加产品
        /// </summary>
        [RelayCommand]
        private void ConfirmAddProduct()
        {
            if (SelectedLevel3 == null)
            {
                Logger.Warning("未选择产品型号");
                return;
            }

            Console.WriteLine($"[DEBUG] ConfirmAddProduct - ItemId: {SelectedLevel3.ItemId}");
            Console.WriteLine($"[DEBUG] ConfirmAddProduct - Model: {SelectedLevel3.Model}");
            Console.WriteLine($"[DEBUG] ConfirmAddProduct - Proportion: {Proportion}");
            Console.WriteLine($"[DEBUG] ConfirmAddProduct - GroupPrice: {GroupPrice}");
            Console.WriteLine($"[DEBUG] ConfirmAddProduct - PurchasePrice: {PurchasePrice}");
            Console.WriteLine($"[DEBUG] ConfirmAddProduct - Commission: {Commission}");

            var result = new ProductSelectorResult
            {
                ItemId = SelectedLevel3.ItemId,
                Model = SelectedLevel3.Model,
                Proportion = Proportion,
                GroupPrice = GroupPrice,
                PurchasePrice = PurchasePrice,
                Commission = Commission
            };

            Logger.Success($"已选择产品: {result.Model}, 组价: {result.GroupPrice}, 进价: {result.PurchasePrice}, 提成: {result.Commission}");
            
            // 触发事件
            ProductSelected?.Invoke(result);
            RequestClose?.Invoke();
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            RequestClose?.Invoke();
        }

        /// <summary>
        /// 关闭详细参数面板（返回选择器）
        /// </summary>
        [RelayCommand]
        private void CloseDetailPanel()
        {
            ShowDetailPanel = false;
            SelectedLevel3 = null;
        }
    }
}
