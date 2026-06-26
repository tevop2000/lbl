using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using AgentManagement.Avalonia.ViewModels;
using AgentManagement.Avalonia.ViewModels.Controls;
using AgentManagement.Avalonia.Controls;
using AgentManagement.Avalonia.Utils;

namespace AgentManagement.Avalonia.Views;

public partial class AgentProductEditDialog : Window
{
    public AgentProductEditDialog()
    {
        InitializeComponent();
        // DataContext 会在外部设置
    }
    
    /// <summary>
    /// 保存成功事件
    /// </summary>
    public event Action<int>? SaveSuccess;
    
    public AgentProductEditDialog(string agentName, int agentId)
    {
        InitializeComponent();
        DataContext = new AgentProductEditViewModel(agentName, agentId);
        
        Console.WriteLine($"[DEBUG] AgentProductEditDialog 构造函数被调用: {agentName}, agentId={agentId}");
        
        // 监听ViewModel的关闭请求
        if (DataContext is AgentProductEditViewModel viewModel)
        {
            Console.WriteLine("[DEBUG] 开始设置事件监听...");
            
            viewModel.RequestClose += () => 
            {
                Console.WriteLine("[DEBUG] 收到关闭请求");
                Close();
            };
            
            // 监听打开产品选择器请求
            viewModel.RequestOpenProductSelector += async () =>
            {
                Console.WriteLine("[DEBUG] ✅ 收到打开产品选择器事件！");
                await ShowProductSelectorAsync(viewModel);
            };
            
            // 监听保存成功事件
            viewModel.SaveSuccess += (agentId) =>
            {
                Console.WriteLine($"[DEBUG] 收到保存成功事件: agentId={agentId}");
                SaveSuccess?.Invoke(agentId);
            };
            
            Console.WriteLine("[DEBUG] 事件监听设置完成");
        }
        else
        {
            Console.WriteLine("[ERROR] DataContext 不是 AgentProductEditViewModel 类型！");
        }
    }

    private void OnDeleteClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductItem product)
        {
            if (DataContext is AgentProductEditViewModel viewModel)
            {
                viewModel.DeleteProductCommand.Execute(product);
            }
        }
    }
    
    /// <summary>
    /// 显示产品选择器对话框
    /// </summary>
    private async Task ShowProductSelectorAsync(AgentProductEditViewModel editViewModel)
    {
        try
        {
            Console.WriteLine("[DEBUG] 开始创建 ProductSelectorDialog...");
            
            // 创建产品选择器对话框
            var selector = new ProductSelectorDialog();
            Console.WriteLine("[DEBUG] ProductSelectorDialog 创建成功");
            
            // 显示对话框
            Console.WriteLine($"[DEBUG] 准备显示对话框，父窗口: {this.Title}");
            var result = await selector.ShowDialog(this);
            Console.WriteLine($"[DEBUG] 对话框关闭，结果: {result?.Model ?? "null"}");
            
            // 如果用户选择了产品，添加到列表
            if (result != null)
            {
                editViewModel.AddProductFromSelector(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] 显示产品选择器失败: {ex.Message}");
            Console.WriteLine($"[ERROR] 堆栈跟踪: {ex.StackTrace}");
        }
    }
}
