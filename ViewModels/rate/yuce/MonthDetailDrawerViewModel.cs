using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AgentManagement.Avalonia.Utils;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AgentManagement.Avalonia.ViewModels.rate.yuce;

/// <summary>
/// 月度详情抽屉面板 ViewModel
/// </summary>
public partial class MonthDetailDrawerViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _windowTitle = "月度详情";

    // 固定成本
    [ObservableProperty]
    private double _batteryCapitalCost = 0; // 电池资金成本

    [ObservableProperty]
    private double _rent = 0; // 租金

    [ObservableProperty]
    private double _salaries = 0; // 工资

    [ObservableProperty]
    private double _vehicleCost = 0; // 车辆费用

    [ObservableProperty]
    private double _utilities = 0; // 水电费

    [ObservableProperty]
    private double _acceptanceIncome = 0; // 承兑收益

    [ObservableProperty]
    private double _afterSalesIncome = 0; // 售后收益

    // 产品列表
    [ObservableProperty]
    private ObservableCollection<ProductItem> _productList = new();

    // 利润贡献
    [ObservableProperty]
    private double _salesGrowthProfit = 0; // 销量增长毛利

    [ObservableProperty]
    private double _structureOptimizeProfit = 0; // 结构优化毛利

    [ObservableProperty]
    private double _premiumProfit = 0; // 溢价毛利

    [ObservableProperty]
    private double _totalCommission = 0; // 总提成

    [ObservableProperty]
    private double _adjustedNetProfit = 0; // 调整后净利润

    [ObservableProperty]
    private double _baseNetProfit = 0; // 起点净利润

    // LiveCharts2 瀑布图数据
    [ObservableProperty]
    private ISeries[] _waterfallSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = Array.Empty<Axis>();

    [ObservableProperty]
    private Axis[] _yAxes = Array.Empty<Axis>();

    /// <summary>
    /// 关闭命令
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        Logger.Info("关闭月度详情");
    }

    /// <summary>
    /// 加载模拟数据
    /// </summary>
    public void LoadMockData(MonthlyDataItem monthData)
    {
        WindowTitle = $"代理商 · {monthData.Month}月详情";

        // 固定成本（模拟数据）
        BatteryCapitalCost = -15000;
        Rent = -8000;
        Salaries = -12000;
        VehicleCost = -3000;
        Utilities = -2000;
        AcceptanceIncome = 1500;
        AfterSalesIncome = 2000;

        // 产品明细（模拟数据）
        ProductList.Clear();
        ProductList.Add(new ProductItem { Model = "6-DZF-12", Proportion = 0.3, UnitProfit = 45, GroupPrice = 100, PurchasePrice = 55 });
        ProductList.Add(new ProductItem { Model = "6-DZF-20", Proportion = 0.4, UnitProfit = 55, GroupPrice = 120, PurchasePrice = 65 });
        ProductList.Add(new ProductItem { Model = "6-DZF-32", Proportion = 0.3, UnitProfit = 65, GroupPrice = 140, PurchasePrice = 75 });

        // 利润贡献
        SalesGrowthProfit = monthData.SalesGrowthProfit;
        StructureOptimizeProfit = monthData.StructureOptimizeProfit;
        PremiumProfit = monthData.PremiumProfit;
        
        // 计算提成（假设提成为利润的 10%）
        TotalCommission = (SalesGrowthProfit + StructureOptimizeProfit + PremiumProfit) * 0.1;
        
        AdjustedNetProfit = monthData.AdjustedNetProfit;
        
        // 计算起点净利润
        BaseNetProfit = AdjustedNetProfit - SalesGrowthProfit - StructureOptimizeProfit - PremiumProfit + TotalCommission;

        // 构建 LiveCharts2 瀑布图
        BuildWaterfallChart();

        Logger.Success($"加载月度详情数据: {WindowTitle}");
    }

    /// <summary>
    /// 构建 LiveCharts2 柱状图数据
    /// </summary>
    private void BuildWaterfallChart()
    {
        // 定义类别
        var categories = new[] { "起点", "销量增长", "结构优化", "溢价", "提成", "最终" };
        
        // 计算每个类别的值
        var values = new[] 
        { 
            BaseNetProfit,
            SalesGrowthProfit,
            StructureOptimizeProfit,
            PremiumProfit,
            -TotalCommission, // 提成是扣减，所以取负值
            AdjustedNetProfit
        };

        // 定义每个柱子的颜色
        var colors = new SKColor[]
        {
            new SKColor(0x64, 0x74, 0x8B), // 起点 - 灰色
            new SKColor(0x10, 0xB9, 0x81), // 销量增长 - 绿色
            new SKColor(0x10, 0xB9, 0x81), // 结构优化 - 绿色
            new SKColor(0x10, 0xB9, 0x81), // 溢价 - 绿色
            new SKColor(0xEF, 0x44, 0x44), // 提成 - 红色（扣减）
            new SKColor(0x64, 0x74, 0x8B)  // 最终 - 灰色
        };

        // 为每个类别创建独立的 ColumnSeries，所有柱子从 Y=0 开始
        var seriesList = new List<ISeries>();
        
        for (int i = 0; i < categories.Length; i++)
        {
            // 每个系列只在自己的 X 位置有值
            var columnValues = new double[categories.Length];
            columnValues[i] = values[i];
            
            var columnSeries = new ColumnSeries<double>
            {
                Values = columnValues,
                Fill = new SolidColorPaint(colors[i]),
                Stroke = null,
                Name = categories[i],
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 10,
                DataLabelsFormatter = point => FormatCurrency(point.PrimaryValue)
            };
            seriesList.Add(columnSeries);
        }
        
        WaterfallSeries = seriesList.ToArray();

        // 配置 X 轴 - 使用微软雅黑字体
        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = categories,
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 231, 235)),
                LabelsPaint = new SolidColorPaint(new SKColor(107, 114, 128))
                {
                    SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")
                },
                ForceStepToMin = true,
                MinStep = 1
            }
        };

        // 配置 Y 轴 - 使用微软雅黑字体
        YAxes = new Axis[]
        {
            new Axis
            {
                Labeler = value => FormatCurrency(value),
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 231, 235, 100)),
                LabelsPaint = new SolidColorPaint(new SKColor(107, 114, 128))
                {
                    SKTypeface = SKTypeface.FromFamilyName("Microsoft YaHei")
                }
            }
        };
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
}

/// <summary>
/// 产品项
/// </summary>
public class ProductItem
{
    public string Model { get; set; } = string.Empty;
    public double Proportion { get; set; }
    public double UnitProfit { get; set; }
    public double GroupPrice { get; set; }
    public double PurchasePrice { get; set; }
}
